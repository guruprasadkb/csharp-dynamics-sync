# Dynamics 365 CRM Migration Sync Engine

## Background

Contoso Financial Services is migrating from their legacy CRM ("LegacyPro") to Microsoft Dynamics 365. You are the lead developer building the real-time sync engine that bridges the two systems during the 6-month parallel-run period.

**The legacy system cannot be modified.** It pushes contact, company, and deal records to your sync engine. Your engine must:

- Map flat legacy records into proper Dynamics 365 entity shapes
- Upsert into CRM (create or update, never duplicate)
- Handle conflicts when both systems modify the same record
- Publish domain events for downstream consumers
- Cache frequently accessed entities to reduce CRM round-trips

The Dynamics 365 entity schemas and legacy schemas are pre-defined in `src/CrmSync/Models/` -- **do not modify them**. An in-memory CRM repository and message bus are provided in `src/CrmSync/Infrastructure/` -- **do not modify those either**.

Your job is to implement the **services** that wire everything together in `src/CrmSync/Services/`.

---

## Getting Started

```bash
# Restore packages
dotnet restore

# Run all tests (most will fail -- the implementations don't exist yet)
dotnet test --verbosity normal

# Run tests for a specific phase
dotnet test --filter "FullyQualifiedName~Phase1"
dotnet test --filter "FullyQualifiedName~Phase2"
dotnet test --filter "FullyQualifiedName~Phase3"
dotnet test --filter "FullyQualifiedName~Phase4"
```

### Required Class Names

The test fixture expects these exact class names in the `CrmSync.Services` namespace:

| Class | Interface | Constructor |
|---|---|---|
| `EntityMapper` | `IEntityMapper` | Parameterless |
| `ConflictResolver` | `IConflictResolver` | Parameterless |
| `SyncService` | `ISyncService` | `(ICrmRepository, IEntityMapper, IConflictResolver, IMessageBus, ConflictStrategy)` |
| `InMemoryCacheProvider` | `ICacheProvider` | Parameterless |

---

## Phase 1: Entity Mapping + Basic Sync (~25 min)

Implement `EntityMapper` and the create/update path in `SyncService`.

### Mapping Rules: LegacyCustomer -> Contact

| Legacy Field | Dynamics Field | Transform |
|---|---|---|
| `FullName` | `FirstName` + `LastName` | Split on first space. `"Jane Doe"` -> first=`"Jane"`, last=`"Doe"`. Single name -> first=name, last=`""` |
| `Email` | `EmailAddress1` | Direct |
| `SecondaryEmail` | `EmailAddress2` | Direct |
| `Phone` | `Telephone1` | Direct |
| `Mobile` | `MobilePhone` | Direct |
| `Street` | `Address1_Line1` | Direct |
| `City` | `Address1_City` | Direct |
| `State` | `Address1_StateOrProvince` | Direct |
| `Zip` | `Address1_PostalCode` | Direct |
| `Country` | `Address1_Country` | Direct |
| `Title` | `JobTitle` | Direct |
| `Dept` | `Department` | Direct |
| `CustomerId` | `Crm_LegacyId` | Direct (used for upsert matching) |
| `Status` | `StateCode` + `StatusCode` | `"active"` -> Active/Active, `"inactive"` -> Inactive/Inactive, `"deleted"` -> Inactive/Inactive |
| `LastUpdatedEpoch` | `ModifiedOn` | Unix epoch seconds -> `DateTime` UTC |

### Mapping Rules: LegacyCompany -> Account

| Legacy Field | Dynamics Field | Transform |
|---|---|---|
| `CompanyName` | `Name` | Direct |
| `MainPhone` | `Telephone1` | Direct |
| `FaxNumber` | `Fax` | Direct |
| `Website` | `WebSiteUrl` | Direct |
| `ContactEmail` | `EmailAddress1` | Direct |
| `Street/City/State/Zip/Country` | `Address1_*` | Direct mapping |
| `EmployeeCount` | `NumberOfEmployees` | Direct |
| `AnnualRevenue` | `Revenue` | Direct |
| `Industry` | `IndustryCode` | Direct |
| `CompanyId` | `Crm_LegacyId` | Direct |
| `Status` | `StateCode/StatusCode` | Same as Contact |
| `LastUpdatedEpoch` | `ModifiedOn` | Unix epoch -> DateTime UTC |

### Mapping Rules: LegacyDeal -> Opportunity

| Legacy Field | Dynamics Field | Transform |
|---|---|---|
| `DealName` | `Name` | Direct |
| `Notes` | `Description` | Direct |
| `ExpectedAmount` | `EstimatedValue` | Direct |
| `FinalAmount` | `ActualValue` | Direct |
| `ExpectedCloseDate` | `EstimatedCloseDate` | ISO date string parse |
| `ActualCloseDate` | `ActualCloseDate` | ISO date string parse |
| `Stage` | `SalesStage` | `"qualifying"`->0, `"developing"`->1, `"proposing"`->2, `"closing"`->3 |
| `WinProbability` | `CloseProbability` | Direct |
| `DealId` | `Crm_LegacyId` | Direct |
| `Status` | `StateCode/StatusCode` | `"open"`->Active/Active, `"won"`->Inactive/Won, `"lost"`->Inactive/Lost, `"canceled"`->Inactive/Canceled |
| `LastUpdatedEpoch` | `ModifiedOn` | Unix epoch -> DateTime UTC |

### Sync Logic (Upsert)

1. Receive a legacy record
2. Look up existing CRM entity by `Crm_LegacyId`
3. **Not found** -> Create (map all fields, set `Crm_LegacyId`, set `Crm_LastSyncedOn` to now)
4. **Found** -> Update (map all fields, preserve `Id` and `VersionNumber`, set `Crm_LastSyncedOn`)
5. Return a `SyncResult` indicating success and the operation type

**Tests to pass:** `Phase1_MappingTests` (6 tests) + `Phase1_SyncTests` (2 tests)

---

## Phase 2: Conflict Resolution + Idempotency (~15 min)

Implement `ConflictResolver` and add conflict detection to the sync update path.

### Conflict Detection

When updating an existing CRM record, detect a conflict if the CRM record's `ModifiedOn` is **after** the legacy record's `LastUpdatedEpoch` (converted to DateTime).

Use the `ConflictStrategy` (passed to `SyncService` constructor) to resolve:

| Strategy | Behavior |
|---|---|
| `SourceWins` | Always apply the legacy change |
| `TargetWins` | Skip the update (return `SyncOperation.Skipped`) |
| `LastWriterWins` | Compare `ModifiedOn` timestamps; most recent wins |

### Idempotency

- If the incoming legacy timestamp (`LastUpdatedEpoch` converted to DateTime) equals or is older than `Crm_LastSyncedOn`, skip the update (return `SyncOperation.Skipped`)
- This prevents reprocessing the same record

### Version Concurrency

- The `InMemoryCrmRepository` enforces `VersionNumber` optimistic concurrency
- When updating, pass the existing record's `VersionNumber` to avoid concurrency exceptions

**Tests to pass:** `Phase2_ConflictTests` (3 tests) + `Phase2_IdempotencyTests` (2 tests)

---

## Phase 3: Dynamics 365 Plugin Implementation (~15 min)

Instead of a generic event bus, Contoso wants to use **Dynamics 365 Plugins** to handle post-sync logic directly within the CRM execution pipeline.

### The Task: Account Auto-Link Plugin

Implement a class `AccountLinkPlugin` in `CrmSync.Services.Plugins` that implements the `IPlugin` interface.

**Plugin Logic:**
1.  The plugin is intended to run in the **Post-Operation** stage of an `Account` sync.
2.  Retrieve the `IPluginExecutionContext` from the `IServiceProvider`.
3.  Retrieve the `IOrganizationService` from the `IServiceProvider`.
4.  The `InputParameters` contains the synced `Account` entity (key: `"Target"`).
5.  If the `Account.PrimaryContactRef` is present (not null/empty):
    -   Search for a `Contact` record where `Crm_LegacyId` matches the `PrimaryContactRef`.
    -   If found, update the `Account.PrimaryContactId` to point to that Contact's GUID.
    -   Important: Use the `IOrganizationService` to perform the update.

### Versioning & Concurrency in Plugins
-   Just like the sync service, ensure you respect the `VersionNumber` if you fetch and update records.

**Tests to pass:** `Phase3_PluginTests` (4 tests)

---

## Phase 4: Caching + Batch Processing (Stretch Goal, ~8 min)

### InMemoryCacheProvider

Implement `ICacheProvider` with an in-memory dictionary.

### Batch Sync

`SyncCustomerBatchAsync` (and other batch methods) should process all records with error isolation.

**Tests to pass:** `Phase4_CacheTests` (3 tests)

---

## Phase 5: D365 UI Personalization (PCF) (~10 min)

The Dynamics 365 business users want a visual indication of the sync status directly on the form. Contoso uses a **Power Apps Component Framework (PCF)** control for this.

### The Task: Sync Status Logic

Implement the `getSyncStatusMessage` logic in `src/CrmSync/PCF/SyncStatusControl/index.ts`.

**Rules:**
1.  If the input `lastSynced` date is **null**, return `"Not Synced"`.
2.  If the input `lastSynced` date is from **today** (same UTC date), return `"Synced recently"`.
3.  Otherwise, return `"Sync Pending Update"`.

**Evaluation:** This phase is evaluated via manual code review in the rubric.

---

## Key Files

| File | Purpose | Modify? |
|------|---------|---------|
| `src/CrmSync/Models/Dynamics/*.cs` | Target CRM entity schemas | **NO** |
| `src/CrmSync/Models/Legacy/*.cs` | Source legacy schemas | **NO** |
| `src/CrmSync/Models/Sync/*.cs` | Sync result types | **NO** |
| `src/CrmSync/Interfaces/*.cs` | Contracts to implement | **NO** |
| `src/CrmSync/Infrastructure/*.cs` | In-memory repo + message bus | **NO** |
| `src/CrmSync/Services/*.cs` | **YOUR CODE GOES HERE** | **YES** |
| `tests/CrmSync.Tests/*.cs` | Test suite | **NO** |

## Time Limit

**60 minutes.** Phase 4 is a stretch goal -- prioritize getting Phases 1-3 solid before attempting it.
