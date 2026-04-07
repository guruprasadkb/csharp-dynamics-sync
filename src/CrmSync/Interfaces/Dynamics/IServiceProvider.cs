namespace CrmSync.Interfaces.Dynamics;

/// <summary>
/// Mock of System.IServiceProvider specialized for Dynamics Plugin execution.
/// </summary>
public interface IServiceProvider
{
    object GetService(Type serviceType);
}
