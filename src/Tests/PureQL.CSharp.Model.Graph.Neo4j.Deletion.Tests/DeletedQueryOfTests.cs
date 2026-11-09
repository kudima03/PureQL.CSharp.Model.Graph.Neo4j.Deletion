using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

[Collection(SharedNeo4j.Name)]
public sealed record DeletedQueryOfTests
{
    private readonly INeo4jStoredQueries _store;

    private readonly SeededDatabase _database;

    public DeletedQueryOfTests(Neo4jFixture fixture)
    {
        _store = new Neo4jStoredQueries(fixture.Driver);
        _database = new SeededDatabase(fixture.Driver);
    }

    [Fact]
    public async Task RemovesEveryNodeOfQuery()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await new DeletedQueryOf(_store, graphs[2].Id).ApplyAsync();
        Assert.Equal(0, await _database.NodeCountAsync(graphs[2].Id));
    }

    [Fact]
    public async Task RemovesExactlyNodesOfQuery()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await new DeletedQueryOf(_store, graphs[2].Id).ApplyAsync();
        Assert.Equal(before - graphs[2].NodeCount, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task RemovesExactlyRelationshipsOfQuery()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.RelationshipCountAsync();
        await new DeletedQueryOf(_store, graphs[2].Id).ApplyAsync();
        Assert.Equal(
            before - graphs[2].RelationshipCount,
            await _database.RelationshipCountAsync()
        );
    }

    [Fact]
    public async Task KeepsEveryNodeWithUnknownId()
    {
        _ = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await new DeletedQueryOf(_store, Guid.NewGuid()).ApplyAsync();
        Assert.Equal(before, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task UsesDatabaseOfStore()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        _ = await Assert.ThrowsAsync<FatalDiscoveryException>(() =>
            new DeletedQueryOf(
                new Neo4jStoredQueries(_store.Driver, "missing"),
                graphs[2].Id
            ).ApplyAsync()
        );
        Assert.Equal(graphs[2].NodeCount, await _database.NodeCountAsync(graphs[2].Id));
    }

    [Fact]
    public async Task KeepsQueryWhenCancelled()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DeletedQueryOf(_store, graphs[2].Id).ApplyAsync(
                new CancellationToken(true)
            )
        );
        Assert.Equal(graphs[2].NodeCount, await _database.NodeCountAsync(graphs[2].Id));
    }
}
