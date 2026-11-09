using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

[Collection(SharedNeo4j.Name)]
public sealed record DeletedQueriesTests
{
    private readonly IDriver _driver;

    private readonly SeededDatabase _database;

    public DeletedQueriesTests(Neo4jFixture fixture)
    {
        _driver = fixture.Driver;
        _database = new SeededDatabase(fixture.Driver);
    }

    [Fact]
    public async Task RemovesEveryNodeOfEachQuery()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(session, [graphs[0].Id, graphs[2].Id]).ApplyAsync();
        Assert.Equal(0, await _database.NodeCountAsync(graphs[0].Id));
        Assert.Equal(0, await _database.NodeCountAsync(graphs[2].Id));
    }

    [Fact]
    public async Task RemovesExactlyNodesOfQueries()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(session, [graphs[0].Id, graphs[2].Id]).ApplyAsync();
        Assert.Equal(
            before - graphs[0].NodeCount - graphs[2].NodeCount,
            await _database.NodeCountAsync()
        );
    }

    [Fact]
    public async Task RemovesExactlyRelationshipsOfQueries()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.RelationshipCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(session, [graphs[0].Id, graphs[2].Id]).ApplyAsync();
        Assert.Equal(
            before - graphs[0].RelationshipCount - graphs[2].RelationshipCount,
            await _database.RelationshipCountAsync()
        );
    }

    [Fact]
    public async Task KeepsOtherQueries()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(session, [graphs[0].Id, graphs[2].Id]).ApplyAsync();
        foreach (StoredGraph graph in new[] { graphs[1], graphs[3], graphs[4] })
        {
            Assert.Equal(graph.NodeCount, await _database.NodeCountAsync(graph.Id));
            Assert.Equal(
                graph.RelationshipCount,
                await _database.RelationshipCountAsync(graph.Id)
            );
        }
    }

    [Fact]
    public async Task KeepsNodeOutsideQueries()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(
            session,
            [.. graphs.Select(graph => graph.Id)]
        ).ApplyAsync();
        Assert.Equal(1, await _database.MarkerCountAsync());
    }

    [Fact]
    public async Task RemovesEveryQueryWithEveryId()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(
            session,
            [.. graphs.Select(graph => graph.Id)]
        ).ApplyAsync();
        Assert.Equal(1, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task SkipsUnknownIds()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(
            session,
            [Guid.NewGuid(), graphs[3].Id, Guid.NewGuid()]
        ).ApplyAsync();
        Assert.Equal(before - graphs[3].NodeCount, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task KeepsEveryNodeWithUnknownIds()
    {
        _ = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(session, [Guid.NewGuid(), Guid.NewGuid()]).ApplyAsync();
        Assert.Equal(before, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task KeepsEveryNodeWithoutIds()
    {
        _ = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(session, []).ApplyAsync();
        Assert.Equal(before, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task KeepsEveryRelationshipWithoutIds()
    {
        _ = await _database.SeedAsync();
        long before = await _database.RelationshipCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(session, []).ApplyAsync();
        Assert.Equal(before, await _database.RelationshipCountAsync());
    }

    [Fact]
    public async Task DeletesRepeatedIdOnce()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(session, [graphs[4].Id, graphs[4].Id]).ApplyAsync();
        Assert.Equal(before - graphs[4].NodeCount, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task KeepsQueriesWithTransactionRolledBack()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await using IAsyncTransaction transaction = await session.BeginTransactionAsync();
        await new DeletedQueries(transaction, [graphs[0].Id, graphs[2].Id]).ApplyAsync();
        await transaction.RollbackAsync();
        Assert.Equal(before, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task KeepsQueriesWhenCancelled()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DeletedQueries(session, [graphs[0].Id]).ApplyAsync(
                new CancellationToken(true)
            )
        );
        Assert.Equal(before, await _database.NodeCountAsync());
    }
}
