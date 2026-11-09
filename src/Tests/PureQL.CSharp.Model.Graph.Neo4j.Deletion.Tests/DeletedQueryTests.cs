using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

[Collection(SharedNeo4j.Name)]
public sealed record DeletedQueryTests
{
    private readonly IDriver _driver;

    private readonly SeededDatabase _database;

    public DeletedQueryTests(Neo4jFixture fixture)
    {
        _driver = fixture.Driver;
        _database = new SeededDatabase(fixture.Driver);
    }

    [Fact]
    public async Task RemovesEveryNodeOfQuery()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQuery(session, graphs[1].Id).ApplyAsync();
        Assert.Equal(0, await _database.NodeCountAsync(graphs[1].Id));
    }

    [Fact]
    public async Task RemovesEveryRelationshipOfQuery()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.RelationshipCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQuery(session, graphs[1].Id).ApplyAsync();
        Assert.Equal(
            before - graphs[1].RelationshipCount,
            await _database.RelationshipCountAsync()
        );
    }

    [Fact]
    public async Task RemovesNoOtherNode()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQuery(session, graphs[1].Id).ApplyAsync();
        Assert.Equal(before - graphs[1].NodeCount, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task KeepsOtherQueries()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQuery(session, graphs[1].Id).ApplyAsync();
        foreach (StoredGraph graph in graphs.Where(graph => graph.Id != graphs[1].Id))
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
        await new DeletedQuery(session, graphs[1].Id).ApplyAsync();
        Assert.Equal(1, await _database.MarkerCountAsync());
    }

    [Fact]
    public async Task KeepsEveryNodeWithUnknownId()
    {
        _ = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQuery(session, Guid.NewGuid()).ApplyAsync();
        Assert.Equal(before, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task KeepsEveryRelationshipWithUnknownId()
    {
        _ = await _database.SeedAsync();
        long before = await _database.RelationshipCountAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQuery(session, Guid.NewGuid()).ApplyAsync();
        Assert.Equal(before, await _database.RelationshipCountAsync());
    }

    [Fact]
    public async Task DeletesAgainWithoutChange()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQuery(session, graphs[1].Id).ApplyAsync();
        long before = await _database.NodeCountAsync();
        await new DeletedQuery(session, graphs[1].Id).ApplyAsync();
        Assert.Equal(before, await _database.NodeCountAsync());
    }

    [Fact]
    public async Task DeletesWithTransactionCommitted()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await using IAsyncTransaction transaction = await session.BeginTransactionAsync();
        await new DeletedQuery(transaction, graphs[1].Id).ApplyAsync();
        await transaction.CommitAsync();
        Assert.Equal(0, await _database.NodeCountAsync(graphs[1].Id));
    }

    [Fact]
    public async Task KeepsQueryWithTransactionRolledBack()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        await using IAsyncTransaction transaction = await session.BeginTransactionAsync();
        await new DeletedQuery(transaction, graphs[1].Id).ApplyAsync();
        await transaction.RollbackAsync();
        Assert.Equal(graphs[1].NodeCount, await _database.NodeCountAsync(graphs[1].Id));
    }

    [Fact]
    public async Task ThrowsWhenCancelled()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DeletedQuery(session, graphs[1].Id).ApplyAsync(
                new CancellationToken(true)
            )
        );
    }

    [Fact]
    public async Task KeepsQueryWhenCancelled()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        await using IAsyncSession session = _driver.AsyncSession();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DeletedQuery(session, graphs[1].Id).ApplyAsync(
                new CancellationToken(true)
            )
        );
        Assert.Equal(graphs[1].NodeCount, await _database.NodeCountAsync(graphs[1].Id));
    }
}
