using CrmSync.Infrastructure;
using CrmSync.Interfaces.Dynamics;
using CrmSync.Models.Dynamics;
using CrmSync.Models.Legacy;
using System.Reflection;

namespace CrmSync.Tests;

public class Phase3_PluginTests
{
    private class MockPluginProvider : IServiceProvider
    {
        private readonly ICrmRepository _repo;
        private readonly Account _target;

        public MockPluginProvider(ICrmRepository repo, Account target)
        {
            _repo = repo;
            _target = target;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType.Name.Contains("IPluginExecutionContext"))
            {
                return new MockExecutionContext(_target);
            }
            if (serviceType.Name.Contains("IOrganizationService"))
            {
                return new MockOrgService(_repo);
            }
            return null!;
        }
    }

    private class MockExecutionContext : IPluginExecutionContext
    {
        public MockExecutionContext(Account target)
        {
            InputParameters = new Dictionary<string, object> { { "Target", target } };
        }

        public Guid BusinessUnitId => Guid.Empty;
        public Guid CorrelationId => Guid.NewGuid();
        public int Depth => 1;
        public Guid InitiatingUserId => Guid.Empty;
        public bool IsInTransaction => false;
        public int IsolationMode => 1;
        public string MessageName => "Update";
        public int Mode => 0;
        public DateTime OperationCreatedOn => DateTime.UtcNow;
        public Guid OperationId => Guid.NewGuid();
        public Guid OrganizationId => Guid.Empty;
        public string OrganizationName => "Contoso";
        public Guid PrimaryEntityId => Guid.Empty;
        public string PrimaryEntityName => "account";
        public Guid? RequestId => null;
        public string SecondaryEntityName => "";
        public int Stage => 40; // Post-operation
        public Guid UserId => Guid.Empty;
        public IDictionary<string, object> InputParameters { get; }
        public IDictionary<string, object> OutputParameters => new Dictionary<string, object>();
        public IDictionary<string, object> PreEntityImages => new Dictionary<string, object>();
        public IDictionary<string, object> PostEntityImages => new Dictionary<string, object>();
    }

    private class MockOrgService : IOrganizationService
    {
        private readonly ICrmRepository _repo;
        public MockOrgService(ICrmRepository repo) => _repo = repo;

        public Task<Guid> CreateAsync(object entity) => throw new NotImplementedException();
        public async Task UpdateAsync(object entity)
        {
            if (entity is Account account) await _repo.UpdateAccountAsync(account);
            else if (entity is Contact contact) await _repo.UpdateContactAsync(contact);
        }
        public Task DeleteAsync(string entityName, Guid id) => throw new NotImplementedException();
        public async Task<T?> RetrieveAsync<T>(string entityName, Guid id) where T : class
        {
            if (typeof(T) == typeof(Account)) return await _repo.GetAccountByIdAsync(id) as T;
            if (typeof(T) == typeof(Contact)) return await _repo.GetContactByIdAsync(id) as T;
            return null;
        }
    }

    [Fact]
    public void Plugin_ImplementsIPlugin()
    {
        var pluginType = Type.GetType("CrmSync.Services.Plugins.AccountLinkPlugin, CrmSync");
        Assert.NotNull(pluginType);
        Assert.True(typeof(IPlugin).IsAssignableFrom(pluginType));
    }

    [Fact]
    public async Task Plugin_LinksContact_WhenPrimaryContactRefExists()
    {
        var repo = new InMemoryCrmRepository();
        
        // 1. Setup Data: Contact exists in CRM
        var contact = new Contact { Id = Guid.NewGuid(), Crm_LegacyId = "CUST-999", FullName = "Plugin Target" };
        await repo.CreateContactAsync(contact);

        // 2. Setup Data: Account being synced
        var account = new Account { Id = Guid.NewGuid(), Crm_LegacyId = "COMP-999", Name = "Linked Corp", PrimaryContactRef = "CUST-999" };
        await repo.CreateAccountAsync(account);

        // 3. Instantiate Plugin
        var pluginType = Type.GetType("CrmSync.Services.Plugins.AccountLinkPlugin, CrmSync");
        var plugin = (IPlugin)Activator.CreateInstance(pluginType!)!;

        // 4. Execute Plugin
        var provider = new MockPluginProvider(repo, account);
        plugin.Execute(provider);

        // 5. Verify Results
        var updatedAccount = await repo.GetAccountByIdAsync(account.Id);
        Assert.NotNull(updatedAccount!.PrimaryContactId);
        Assert.Equal(contact.Id, updatedAccount.PrimaryContactId);
    }

    // Additional tests for concurrency, null handling, etc. can be added here
}
