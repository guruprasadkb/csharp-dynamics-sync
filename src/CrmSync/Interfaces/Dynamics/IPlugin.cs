namespace CrmSync.Interfaces.Dynamics;

/// <summary>
/// Mock of Microsoft.Xrm.Sdk.IPlugin
/// </summary>
public interface IPlugin
{
    void Execute(IServiceProvider serviceProvider);
}
