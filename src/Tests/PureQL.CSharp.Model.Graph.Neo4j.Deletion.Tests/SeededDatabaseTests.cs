namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

[Collection(SharedNeo4j.Name)]
public sealed record SeededDatabaseTests
{
    private readonly SeededDatabase _database;

    public SeededDatabaseTests(Neo4jFixture fixture)
    {
        _database = new SeededDatabase(fixture.Driver);
    }

    [Fact]
    public async Task StoresEveryNodeOfEachQuery()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        foreach (StoredGraph graph in graphs)
        {
            Assert.Equal(graph.NodeCount, await _database.NodeCountAsync(graph.Id));
        }
    }

    [Fact]
    public async Task StoresEveryRelationshipOfEachQuery()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        foreach (StoredGraph graph in graphs)
        {
            Assert.Equal(
                graph.RelationshipCount,
                await _database.RelationshipCountAsync(graph.Id)
            );
        }
    }

    [Fact]
    public async Task StoresQueriesNextToMarker()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        Assert.Equal(
            graphs.Sum(graph => graph.NodeCount) + 1,
            await _database.NodeCountAsync()
        );
    }

    [Fact]
    public async Task StoresOnlyQueryRelationships()
    {
        IReadOnlyList<StoredGraph> graphs = await _database.SeedAsync();
        Assert.Equal(
            graphs.Sum(graph => graph.RelationshipCount),
            await _database.RelationshipCountAsync()
        );
    }
}
