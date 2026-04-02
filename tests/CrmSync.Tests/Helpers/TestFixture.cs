using CrmSync.Infrastructure;
using CrmSync.Interfaces;
using CrmSync.Models.Sync;

namespace CrmSync.Tests.Helpers;

/// <summary>
/// Creates service instances for tests. The candidate must create classes
/// with these exact names in the CrmSync.Services namespace:
///
///   - EntityMapper        : IEntityMapper
///   - SyncService         : ISyncService  (constructor: ICrmRepository, IEntityMapper, IConflictResolver, IMessageBus, ConflictStrategy)
///   - ConflictResolver    : IConflictResolver
///   - InMemoryCacheProvider : ICacheProvider
///
/// DO NOT MODIFY this file.
/// </summary>
public static class TestFixture
{
    public static IEntityMapper CreateMapper()
    {
        var type = Type.GetType("CrmSync.Services.EntityMapper, CrmSync")
            ?? throw new InvalidOperationException(
                "Could not find CrmSync.Services.EntityMapper. " +
                "Create a class named 'EntityMapper' in the Services folder that implements IEntityMapper.");

        return (IEntityMapper)Activator.CreateInstance(type)!;
    }

    public static IConflictResolver CreateConflictResolver()
    {
        var type = Type.GetType("CrmSync.Services.ConflictResolver, CrmSync")
            ?? throw new InvalidOperationException(
                "Could not find CrmSync.Services.ConflictResolver. " +
                "Create a class named 'ConflictResolver' in the Services folder that implements IConflictResolver.");

        return (IConflictResolver)Activator.CreateInstance(type)!;
    }

    public static ISyncService CreateSyncService(
        InMemoryCrmRepository repo,
        InMemoryMessageBus bus,
        ConflictStrategy strategy = ConflictStrategy.LastWriterWins)
    {
        var mapper = CreateMapper();
        var resolver = CreateConflictResolver();

        var type = Type.GetType("CrmSync.Services.SyncService, CrmSync")
            ?? throw new InvalidOperationException(
                "Could not find CrmSync.Services.SyncService. " +
                "Create a class named 'SyncService' in the Services folder that implements ISyncService.");

        // Try constructor: (ICrmRepository, IEntityMapper, IConflictResolver, IMessageBus, ConflictStrategy)
        var instance = Activator.CreateInstance(type, repo, mapper, resolver, (IMessageBus)bus, strategy)
            ?? throw new InvalidOperationException("Failed to create SyncService instance.");

        return (ISyncService)instance;
    }

    public static ICacheProvider CreateCacheProvider()
    {
        var type = Type.GetType("CrmSync.Services.InMemoryCacheProvider, CrmSync")
            ?? throw new InvalidOperationException(
                "Could not find CrmSync.Services.InMemoryCacheProvider. " +
                "Create a class named 'InMemoryCacheProvider' in the Services folder that implements ICacheProvider.");

        return (ICacheProvider)Activator.CreateInstance(type)!;
    }
}
