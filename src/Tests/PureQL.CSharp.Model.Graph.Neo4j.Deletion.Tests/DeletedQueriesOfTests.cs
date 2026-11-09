using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

[Collection(SharedNeo4j.Name)]
public sealed record DeletedQueriesOfTests
{
    private readonly INeo4jStoredQueries _store;

    private readonly SeededDatabase _database;

    public DeletedQueriesOfTests(Neo4jFixture fixture)
    {
        _store = new Neo4jStoredQueries(fixture.Driver);
        _database = new SeededDatabase(fixture.Driver);
    }

    [Fact]
    public async Task RemovesEveryNodeOfEachQuery()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await new DeletedQueriesOf(_store, [graphs[1].Id, graphs[4].Id]).ApplyAsync();
        Assert.Equal(0, await _database.NodeCountAsync(graphs[1].Id));
        Assert.Equal(0, await _database.NodeCountAsync(graphs[4].Id));
    }

    [Fact]
    public async Task RemovesExactlyNodesOfQueries()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await new DeletedQueriesOf(_store, [graphs[1].Id, graphs[4].Id]).ApplyAsync();
        Assert.Equal(
            before - graphs[1].NodeCount - graphs[4].NodeCount,
            await _database.NodeCountAsync()
        );
    }

    [Fact]
    public async Task RemovesExactlyRelationshipsOfQueries()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.RelationshipCountAsync();
        await new DeletedQueriesOf(_store, [graphs[1].Id, graphs[4].Id]).ApplyAsync();
        Assert.Equal(
            before - graphs[1].RelationshipCount - graphs[4].RelationshipCount,
            await _database.RelationshipCountAsync()
        );
    }

    [Fact]
    public async Task KeepsEveryNodeWithoutIds()
    {
        _ = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await new DeletedQueriesOf(_store, []).ApplyAsync();
        Assert.Equal(before, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task KeepsEveryNodeWithUnknownIds()
    {
        _ = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await new DeletedQueriesOf(_store, [Guid.NewGuid()]).ApplyAsync();
        Assert.Equal(before, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task UsesDatabaseOfStore()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        _ = await Assert.ThrowsAsync<FatalDiscoveryException>(() =>
            new DeletedQueriesOf(
                new Neo4jStoredQueries(_store.Driver, "missing"),
                [graphs[1].Id]
            ).ApplyAsync()
        );
        Assert.Equal(before, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task KeepsQueriesWhenCancelled()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DeletedQueriesOf(_store, [graphs[1].Id]).ApplyAsync(
                new CancellationToken(true)
            )
        );
        Assert.Equal(before, await _database.NodeCountAsync());
    }
}
