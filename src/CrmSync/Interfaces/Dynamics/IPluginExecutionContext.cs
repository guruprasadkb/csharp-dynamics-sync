namespace CrmSync.Interfaces.Dynamics;

/// <summary>
/// Mock of Microsoft.Xrm.Sdk.IPluginExecutionContext
/// </summary>
public interface IPluginExecutionContext
{
    Guid BusinessUnitId { get; }
    Guid CorrelationId { get; }
    int Depth { get; }
    Guid InitiatingUserId { get; }
    bool IsInTransaction { get; }
    int IsolationMode { get; }
    string MessageName { get; }
    int Mode { get; }
    DateTime OperationCreatedOn { get; }
    Guid OperationId { get; }
    Guid OrganizationId { get; }
    string OrganizationName { get; }
    Guid PrimaryEntityId { get; }
    string PrimaryEntityName { get; }
    Guid? RequestId { get; }
    string SecondaryEntityName { get; }
    int Stage { get; }
    Guid UserId { get; }
    
    // Using object to simplify, normally ParameterCollection
    IDictionary<string, object> InputParameters { get; }
    IDictionary<string, object> OutputParameters { get; }
    IDictionary<string, object> PreEntityImages { get; }
    IDictionary<string, object> PostEntityImages { get; }
}
