namespace CrmSync.Interfaces;

/// <summary>
/// Simulates Azure Service Bus for event-driven processing.
/// An in-memory implementation is provided -- do not modify it.
/// </summary>
public interface IMessageBus
{
    /// <summary>Publish a message to a topic.</summary>
    Task PublishAsync<T>(string topic, T message) where T : class;

    /// <summary>Subscribe a handler to a topic.</summary>
    void Subscribe<T>(string topic, Func<T, Task> handler) where T : class;

    /// <summary>Get count of messages published to a topic (for testing).</summary>
    int GetMessageCount(string topic);

    /// <summary>Get all messages published to a topic (for testing).</summary>
    IReadOnlyList<object> GetMessages(string topic);

    /// <summary>Process all pending messages (triggers subscribed handlers).</summary>
    Task ProcessPendingAsync();
}
