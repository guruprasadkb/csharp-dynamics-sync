namespace CrmSync.Services;

using CrmSync.Interfaces;
using CrmSync.Models.Dynamics;
using CrmSync.Models.Legacy;
using CrmSync.Models.Sync;
using System.Diagnostics;

public sealed class SyncService : ISyncService
{
    private readonly ICrmRepository _repo;
    private readonly IEntityMapper _mapper;
    private readonly IConflictResolver _conflictResolver;
    private readonly IMessageBus _bus;
    private readonly ConflictStrategy _strategy;

    public SyncService(
        ICrmRepository repo,
        IEntityMapper mapper,
        IConflictResolver conflictResolver,
        IMessageBus bus,
        ConflictStrategy strategy)
    {
        _repo = repo;
        _mapper = mapper;
        _conflictResolver = conflictResolver;
        _bus = bus;
        _strategy = strategy;

        SubscribeAutoLinkHandler();
    }

    public async Task<SyncResult> SyncCustomerAsync(LegacyCustomer customer)
    {
        try
        {
            var mapped = _mapper.MapToContact(customer);
            var existing = await _repo.GetContactByLegacyIdAsync(customer.CustomerId);

            var legacyModified = DateTimeOffset.FromUnixTimeSeconds(customer.LastUpdatedEpoch).UtcDateTime;

            if (existing is null)
            {
                mapped.Crm_LastSyncedOn = legacyModified;
                var created = await _repo.CreateContactAsync(mapped);
                await _bus.PublishAsync("contact.synced", new SyncEvent(created.Id, customer.CustomerId, SyncOperation.Created));
                return new SyncResult { Success = true, EntityId = created.Id, Operation = SyncOperation.Created, LegacyId = customer.CustomerId };
            }

            // Idempotency: skip if legacy timestamp <= last synced
            if (existing.Crm_LastSyncedOn.HasValue && legacyModified <= existing.Crm_LastSyncedOn.Value)
            {
                return new SyncResult { Success = true, EntityId = existing.Id, Operation = SyncOperation.Skipped, LegacyId = customer.CustomerId };
            }

            // Conflict detection: CRM modified after legacy record
            if (existing.ModifiedOn > legacyModified)
            {
                var conflict = new ConflictInfo
                {
                    EntityId = existing.Id,
                    LegacyId = customer.CustomerId,
                    EntityType = "Contact",
                    LegacyModifiedOn = legacyModified,
                    CrmModifiedOn = existing.Crm_LastSyncedOn ?? existing.ModifiedOn,
                    ExpectedVersion = existing.VersionNumber,
                    ActualVersion = existing.VersionNumber
                };

                if (!_conflictResolver.ShouldApplyChange(conflict, _strategy))
                {
                    return new SyncResult { Success = true, EntityId = existing.Id, Operation = SyncOperation.Skipped, LegacyId = customer.CustomerId };
                }
            }

            // Apply update
            mapped.Id = existing.Id;
            mapped.VersionNumber = existing.VersionNumber;
            mapped.Crm_LastSyncedOn = legacyModified;
            var updated = await _repo.UpdateContactAsync(mapped);
            await _bus.PublishAsync("contact.synced", new SyncEvent(updated.Id, customer.CustomerId, SyncOperation.Updated));
            return new SyncResult { Success = true, EntityId = updated.Id, Operation = SyncOperation.Updated, LegacyId = customer.CustomerId };
        }
        catch (Exception ex)
        {
            return new SyncResult { Success = false, Operation = SyncOperation.Failed, ErrorMessage = ex.Message, LegacyId = customer.CustomerId };
        }
    }

    public async Task<SyncResult> SyncCompanyAsync(LegacyCompany company)
    {
        try
        {
            var mapped = _mapper.MapToAccount(company);
            var existing = await _repo.GetAccountByLegacyIdAsync(company.CompanyId);

            var legacyModified = DateTimeOffset.FromUnixTimeSeconds(company.LastUpdatedEpoch).UtcDateTime;

            if (existing is null)
            {
                mapped.Crm_LastSyncedOn = legacyModified;
                var created = await _repo.CreateAccountAsync(mapped);
                await _bus.PublishAsync("account.synced", new AccountSyncEvent(created.Id, company.CompanyId, SyncOperation.Created, company.PrimaryContactRef));
                return new SyncResult { Success = true, EntityId = created.Id, Operation = SyncOperation.Created, LegacyId = company.CompanyId };
            }

            if (existing.Crm_LastSyncedOn.HasValue && legacyModified <= existing.Crm_LastSyncedOn.Value)
            {
                return new SyncResult { Success = true, EntityId = existing.Id, Operation = SyncOperation.Skipped, LegacyId = company.CompanyId };
            }

            if (existing.ModifiedOn > legacyModified)
            {
                var conflict = new ConflictInfo
                {
                    EntityId = existing.Id,
                    LegacyId = company.CompanyId,
                    EntityType = "Account",
                    LegacyModifiedOn = legacyModified,
                    CrmModifiedOn = existing.Crm_LastSyncedOn ?? existing.ModifiedOn,
                    ExpectedVersion = existing.VersionNumber,
                    ActualVersion = existing.VersionNumber
                };

                if (!_conflictResolver.ShouldApplyChange(conflict, _strategy))
                {
                    return new SyncResult { Success = true, EntityId = existing.Id, Operation = SyncOperation.Skipped, LegacyId = company.CompanyId };
                }
            }

            mapped.Id = existing.Id;
            mapped.VersionNumber = existing.VersionNumber;
            mapped.Crm_LastSyncedOn = legacyModified;
            var updated = await _repo.UpdateAccountAsync(mapped);
            await _bus.PublishAsync("account.synced", new AccountSyncEvent(updated.Id, company.CompanyId, SyncOperation.Updated, company.PrimaryContactRef));
            return new SyncResult { Success = true, EntityId = updated.Id, Operation = SyncOperation.Updated, LegacyId = company.CompanyId };
        }
        catch (Exception ex)
        {
            return new SyncResult { Success = false, Operation = SyncOperation.Failed, ErrorMessage = ex.Message, LegacyId = company.CompanyId };
        }
    }

    public async Task<SyncResult> SyncDealAsync(LegacyDeal deal)
    {
        try
        {
            var mapped = _mapper.MapToOpportunity(deal);
            var existing = await _repo.GetOpportunityByLegacyIdAsync(deal.DealId);

            var legacyModified = DateTimeOffset.FromUnixTimeSeconds(deal.LastUpdatedEpoch).UtcDateTime;

            if (existing is null)
            {
                mapped.Crm_LastSyncedOn = legacyModified;
                var created = await _repo.CreateOpportunityAsync(mapped);
                await _bus.PublishAsync("opportunity.synced", new SyncEvent(created.Id, deal.DealId, SyncOperation.Created));
                return new SyncResult { Success = true, EntityId = created.Id, Operation = SyncOperation.Created, LegacyId = deal.DealId };
            }

            if (existing.Crm_LastSyncedOn.HasValue && legacyModified <= existing.Crm_LastSyncedOn.Value)
            {
                return new SyncResult { Success = true, EntityId = existing.Id, Operation = SyncOperation.Skipped, LegacyId = deal.DealId };
            }

            if (existing.ModifiedOn > legacyModified)
            {
                var conflict = new ConflictInfo
                {
                    EntityId = existing.Id,
                    LegacyId = deal.DealId,
                    EntityType = "Opportunity",
                    LegacyModifiedOn = legacyModified,
                    CrmModifiedOn = existing.Crm_LastSyncedOn ?? existing.ModifiedOn,
                    ExpectedVersion = existing.VersionNumber,
                    ActualVersion = existing.VersionNumber
                };

                if (!_conflictResolver.ShouldApplyChange(conflict, _strategy))
                {
                    return new SyncResult { Success = true, EntityId = existing.Id, Operation = SyncOperation.Skipped, LegacyId = deal.DealId };
                }
            }

            mapped.Id = existing.Id;
            mapped.VersionNumber = existing.VersionNumber;
            mapped.Crm_LastSyncedOn = legacyModified;
            var updated = await _repo.UpdateOpportunityAsync(mapped);
            await _bus.PublishAsync("opportunity.synced", new SyncEvent(updated.Id, deal.DealId, SyncOperation.Updated));
            return new SyncResult { Success = true, EntityId = updated.Id, Operation = SyncOperation.Updated, LegacyId = deal.DealId };
        }
        catch (Exception ex)
        {
            return new SyncResult { Success = false, Operation = SyncOperation.Failed, ErrorMessage = ex.Message, LegacyId = deal.DealId };
        }
    }

    public async Task<BatchResult> SyncCustomerBatchAsync(IReadOnlyList<LegacyCustomer> customers)
    {
        return await RunBatchAsync(customers, SyncCustomerAsync);
    }

    public async Task<BatchResult> SyncCompanyBatchAsync(IReadOnlyList<LegacyCompany> companies)
    {
        return await RunBatchAsync(companies, SyncCompanyAsync);
    }

    public async Task<BatchResult> SyncDealBatchAsync(IReadOnlyList<LegacyDeal> deals)
    {
        return await RunBatchAsync(deals, SyncDealAsync);
    }

    private static async Task<BatchResult> RunBatchAsync<T>(IReadOnlyList<T> records, Func<T, Task<SyncResult>> syncFunc)
    {
        var sw = Stopwatch.StartNew();
        var results = new List<SyncResult>(records.Count);

        foreach (var record in records)
        {
            try
            {
                var result = await syncFunc(record);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(new SyncResult { Success = false, Operation = SyncOperation.Failed, ErrorMessage = ex.Message });
            }
        }

        sw.Stop();

        return new BatchResult
        {
            TotalRecords = records.Count,
            Created = results.Count(r => r.Operation == SyncOperation.Created),
            Updated = results.Count(r => r.Operation == SyncOperation.Updated),
            Skipped = results.Count(r => r.Operation == SyncOperation.Skipped),
            Failed = results.Count(r => r.Operation == SyncOperation.Failed),
            Conflicts = results.Count(r => r.Operation == SyncOperation.Conflict),
            Results = results,
            Duration = sw.Elapsed
        };
    }

    private void SubscribeAutoLinkHandler()
    {
        _bus.Subscribe<AccountSyncEvent>("account.synced", async evt =>
        {
            if (string.IsNullOrEmpty(evt.PrimaryContactRef))
                return;

            var account = await _repo.GetAccountByIdAsync(evt.EntityId);
            if (account is null) return;

            var contact = await _repo.GetContactByLegacyIdAsync(evt.PrimaryContactRef);
            if (contact is null) return;

            account.PrimaryContactId = contact.Id;
            await _repo.UpdateAccountAsync(account);
        });
    }

    public sealed record SyncEvent(Guid EntityId, string LegacyId, SyncOperation Operation);
    public sealed record AccountSyncEvent(Guid EntityId, string LegacyId, SyncOperation Operation, string? PrimaryContactRef);
}
