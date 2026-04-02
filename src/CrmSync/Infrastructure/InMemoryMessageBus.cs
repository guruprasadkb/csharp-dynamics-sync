namespace CrmSync.Infrastructure;

using CrmSync.Interfaces;
using System.Collections.Concurrent;

/// <summary>
/// In-memory message bus simulating Azure Service Bus topics/subscriptions.
/// DO NOT MODIFY.
/// </summary>
public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly ConcurrentDictionary<string, List<object>> _messages = new();
    private readonly ConcurrentDictionary<string, List<Delegate>> _handlers = new();

    public Task PublishAsync<T>(string topic, T message) where T : class
    {
        ArgumentNullException.ThrowIfNull(message);
        var messages = _messages.GetOrAdd(topic, _ => []);
        lock (messages)
        {
            messages.Add(message);
        }
        return Task.CompletedTask;
    }

    public void Subscribe<T>(string topic, Func<T, Task> handler) where T : class
    {
        var handlers = _handlers.GetOrAdd(topic, _ => []);
        lock (handlers)
        {
            handlers.Add(handler);
        }
    }

    public int GetMessageCount(string topic)
    {
        if (_messages.TryGetValue(topic, out var messages))
        {
            lock (messages) { return messages.Count; }
        }
        return 0;
    }

    public IReadOnlyList<object> GetMessages(string topic)
    {
        if (_messages.TryGetValue(topic, out var messages))
        {
            lock (messages) { return messages.ToList(); }
        }
        return [];
    }

    public async Task ProcessPendingAsync()
    {
        foreach (var (topic, messages) in _messages)
        {
            if (!_handlers.TryGetValue(topic, out var handlers))
                continue;

            List<object> toProcess;
            lock (messages)
            {
                toProcess = [.. messages];
                messages.Clear();
            }

            foreach (var message in toProcess)
            {
                List<Delegate> currentHandlers;
                lock (handlers) { currentHandlers = [.. handlers]; }

                foreach (var handler in currentHandlers)
                {
                    try
                    {
                        var task = (Task)handler.DynamicInvoke(message)!;
                        await task;
                    }
                    catch
                    {
                        // In production: dead-letter. For this challenge: swallow.
                    }
                }
            }
        }
    }
}
