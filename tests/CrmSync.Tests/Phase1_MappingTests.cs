using CrmSync.Models.Dynamics;
using CrmSync.Models.Legacy;
using CrmSync.Tests.Helpers;

namespace CrmSync.Tests;

public class Phase1_MappingTests
{
    private readonly CrmSync.Interfaces.IEntityMapper _mapper;

    public Phase1_MappingTests()
    {
        _mapper = TestFixture.CreateMapper();
    }

    [Fact]
    public void MapToContact_SplitsFullName_IntoFirstAndLast()
    {
        var legacy = new LegacyCustomer
        {
            CustomerId = "CUST-001",
            FullName = "Jane Doe",
            Email = "jane@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };

        var contact = _mapper.MapToContact(legacy);

        Assert.Equal("Jane", contact.FirstName);
        Assert.Equal("Doe", contact.LastName);
    }

    [Fact]
    public void MapToContact_SingleName_PutsEntireNameInFirstName()
    {
        var legacy = new LegacyCustomer
        {
            CustomerId = "CUST-002",
            FullName = "Cher",
            Email = "cher@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };

        var contact = _mapper.MapToContact(legacy);

        Assert.Equal("Cher", contact.FirstName);
        Assert.Equal(string.Empty, contact.LastName);
    }

    [Fact]
    public void MapToContact_MapsAllFields_IncludingAddressAndLegacyId()
    {
        var legacy = new LegacyCustomer
        {
            CustomerId = "CUST-003",
            FullName = "John Smith",
            Email = "john@contoso.com",
            SecondaryEmail = "john.personal@email.com",
            Phone = "+1-555-0100",
            Mobile = "+1-555-0101",
            Street = "123 Main St",
            City = "Seattle",
            State = "WA",
            Zip = "98101",
            Country = "US",
            Title = "Senior Developer",
            Dept = "Engineering",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };

        var contact = _mapper.MapToContact(legacy);

        Assert.Equal("john@contoso.com", contact.EmailAddress1);
        Assert.Equal("john.personal@email.com", contact.EmailAddress2);
        Assert.Equal("+1-555-0100", contact.Telephone1);
        Assert.Equal("+1-555-0101", contact.MobilePhone);
        Assert.Equal("123 Main St", contact.Address1_Line1);
        Assert.Equal("Seattle", contact.Address1_City);
        Assert.Equal("WA", contact.Address1_StateOrProvince);
        Assert.Equal("98101", contact.Address1_PostalCode);
        Assert.Equal("US", contact.Address1_Country);
        Assert.Equal("Senior Developer", contact.JobTitle);
        Assert.Equal("Engineering", contact.Department);
        Assert.Equal("CUST-003", contact.Crm_LegacyId);
    }

    [Theory]
    [InlineData("active", StateCode.Active, StatusCode.Active)]
    [InlineData("inactive", StateCode.Inactive, StatusCode.Inactive)]
    [InlineData("deleted", StateCode.Inactive, StatusCode.Inactive)]
    public void MapToContact_MapsStatus_Correctly(
        string legacyStatus, StateCode expectedState, StatusCode expectedStatus)
    {
        var legacy = new LegacyCustomer
        {
            CustomerId = "CUST-004",
            FullName = "Test User",
            Email = "test@contoso.com",
            Status = legacyStatus,
            LastUpdatedEpoch = 1700000000
        };

        var contact = _mapper.MapToContact(legacy);

        Assert.Equal(expectedState, contact.StateCode);
        Assert.Equal(expectedStatus, contact.StatusCode);
    }

    [Fact]
    public void MapToContact_ConvertsEpoch_ToUtcDateTime()
    {
        var legacy = new LegacyCustomer
        {
            CustomerId = "CUST-005",
            FullName = "Epoch Test",
            Email = "epoch@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000  // 2023-11-14T22:13:20Z
        };

        var contact = _mapper.MapToContact(legacy);

        var expected = DateTimeOffset.FromUnixTimeSeconds(1700000000).UtcDateTime;
        Assert.Equal(expected, contact.ModifiedOn);
    }

    [Fact]
    public void MapToOpportunity_MapsStageAndStatus_Correctly()
    {
        var legacy = new LegacyDeal
        {
            DealId = "DEAL-001",
            DealName = "Contoso ERP Upgrade",
            Stage = "proposing",
            Status = "open",
            ExpectedAmount = 150000m,
            ExpectedCloseDate = "2025-06-15",
            WinProbability = 60,
            LastUpdatedEpoch = 1700000000
        };

        var opportunity = _mapper.MapToOpportunity(legacy);

        Assert.Equal("Contoso ERP Upgrade", opportunity.Name);
        Assert.Equal(2, opportunity.SalesStage);    // "proposing" -> 2
        Assert.Equal(StateCode.Active, opportunity.StateCode);
        Assert.Equal(StatusCode.Active, opportunity.StatusCode);
        Assert.Equal(150000m, opportunity.EstimatedValue);
        Assert.Equal(new DateTime(2025, 6, 15), opportunity.EstimatedCloseDate);
        Assert.Equal(60, opportunity.CloseProbability);
        Assert.Equal("DEAL-001", opportunity.Crm_LegacyId);
    }
}
