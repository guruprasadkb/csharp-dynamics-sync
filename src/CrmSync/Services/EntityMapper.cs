namespace CrmSync.Services;

using CrmSync.Interfaces;
using CrmSync.Models.Dynamics;
using CrmSync.Models.Legacy;

public sealed class EntityMapper : IEntityMapper
{
    public Contact MapToContact(LegacyCustomer legacy)
    {
        var (firstName, lastName) = SplitName(legacy.FullName);

        var contact = new Contact
        {
            FirstName = firstName,
            LastName = lastName,
            EmailAddress1 = legacy.Email,
            EmailAddress2 = legacy.SecondaryEmail,
            Telephone1 = legacy.Phone,
            MobilePhone = legacy.Mobile,
            Address1_Line1 = legacy.Street,
            Address1_City = legacy.City,
            Address1_StateOrProvince = legacy.State,
            Address1_PostalCode = legacy.Zip,
            Address1_Country = legacy.Country,
            JobTitle = legacy.Title,
            Department = legacy.Dept,
            Crm_LegacyId = legacy.CustomerId,
            ModifiedOn = DateTimeOffset.FromUnixTimeSeconds(legacy.LastUpdatedEpoch).UtcDateTime
        };

        MapContactStatus(legacy.Status, contact);
        return contact;
    }

    public Account MapToAccount(LegacyCompany legacy)
    {
        var account = new Account
        {
            Name = legacy.CompanyName,
            Telephone1 = legacy.MainPhone,
            Fax = legacy.FaxNumber,
            WebSiteUrl = legacy.Website,
            EmailAddress1 = legacy.ContactEmail,
            Address1_Line1 = legacy.Street,
            Address1_City = legacy.City,
            Address1_StateOrProvince = legacy.State,
            Address1_PostalCode = legacy.Zip,
            Address1_Country = legacy.Country,
            NumberOfEmployees = legacy.EmployeeCount,
            Revenue = legacy.AnnualRevenue,
            IndustryCode = legacy.Industry,
            Crm_LegacyId = legacy.CompanyId,
            ModifiedOn = DateTimeOffset.FromUnixTimeSeconds(legacy.LastUpdatedEpoch).UtcDateTime
        };

        MapContactStatus(legacy.Status, account);
        return account;
    }

    public Opportunity MapToOpportunity(LegacyDeal legacy)
    {
        var opportunity = new Opportunity
        {
            Name = legacy.DealName,
            Description = legacy.Notes,
            EstimatedValue = legacy.ExpectedAmount,
            ActualValue = legacy.FinalAmount,
            EstimatedCloseDate = ParseDate(legacy.ExpectedCloseDate),
            ActualCloseDate = ParseDate(legacy.ActualCloseDate),
            SalesStage = MapSalesStage(legacy.Stage),
            CloseProbability = legacy.WinProbability,
            Crm_LegacyId = legacy.DealId,
            ModifiedOn = DateTimeOffset.FromUnixTimeSeconds(legacy.LastUpdatedEpoch).UtcDateTime
        };

        MapOpportunityStatus(legacy.Status, opportunity);
        return opportunity;
    }

    private static (string first, string last) SplitName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (string.Empty, string.Empty);

        var spaceIndex = fullName.IndexOf(' ');
        if (spaceIndex < 0)
            return (fullName, string.Empty);

        return (fullName[..spaceIndex], fullName[(spaceIndex + 1)..]);
    }

    private static void MapContactStatus(string status, EntityBase entity)
    {
        switch (status.ToLowerInvariant())
        {
            case "active":
                entity.StateCode = StateCode.Active;
                entity.StatusCode = StatusCode.Active;
                break;
            case "inactive":
            case "deleted":
                entity.StateCode = StateCode.Inactive;
                entity.StatusCode = StatusCode.Inactive;
                break;
        }
    }

    private static void MapOpportunityStatus(string status, Opportunity opp)
    {
        switch (status.ToLowerInvariant())
        {
            case "open":
                opp.StateCode = StateCode.Active;
                opp.StatusCode = StatusCode.Active;
                break;
            case "won":
                opp.StateCode = StateCode.Inactive;
                opp.StatusCode = StatusCode.Won;
                break;
            case "lost":
                opp.StateCode = StateCode.Inactive;
                opp.StatusCode = StatusCode.Lost;
                break;
            case "canceled":
                opp.StateCode = StateCode.Inactive;
                opp.StatusCode = StatusCode.Canceled;
                break;
        }
    }

    private static int MapSalesStage(string stage) => stage.ToLowerInvariant() switch
    {
        "qualifying" => 0,
        "developing" => 1,
        "proposing" => 2,
        "closing" => 3,
        _ => 0
    };

    private static DateTime? ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return null;
        return DateTime.Parse(dateStr);
    }
}
