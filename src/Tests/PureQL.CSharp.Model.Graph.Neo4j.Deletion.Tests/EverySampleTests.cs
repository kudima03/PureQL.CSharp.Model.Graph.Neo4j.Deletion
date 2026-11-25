using Neo4j.Driver;
using PureQL.CSharp.Model.Samples.Queries.Subqueries;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

/// <summary>
/// Every sample query written next to another one and deleted again: its graph is gone,
/// <see cref="GraphLabels.List"/> nodes included, and the other query is untouched.
/// </summary>
[Collection(SharedNeo4j.Name)]
public sealed record EverySampleTests
{
    private readonly IDriver _driver;

    private readonly SeededDatabase _database;

    public EverySampleTests(Neo4jFixture fixture)
    {
        _driver = fixture.Driver;
        _database = new SeededDatabase(fixture.Driver);
    }

    [Fact]
    public void EverySampleIsLoaded()
    {
        Assert.Equal(151, QuerySamples.All.Count);
    }

    [Theory]
    [MemberData(nameof(Samples))]
    public async Task DeletesEveryNodeAndRelationshipOfTheSample(string name)
    {
        StoredGraph sample = Graph(QuerySamples.All[name]);
        StoredGraph other = Graph(new InlineStatusRowsQuery().Value);
        await WriteAsync(sample, other);
        Assert.Equal(sample.NodeCount, await _database.NodeCountAsync(sample.Id));
        Assert.Equal(
            sample.RelationshipCount,
            await _database.RelationshipCountAsync(sample.Id)
        );

        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQuery(session, sample.Id).ApplyAsync();

        Assert.Equal(0, await _database.NodeCountAsync(sample.Id));
        Assert.Equal(0, await _database.RelationshipCountAsync(sample.Id));
        Assert.Equal(other.NodeCount, await _database.NodeCountAsync(other.Id));
        Assert.Equal(
            other.RelationshipCount,
            await _database.RelationshipCountAsync(other.Id)
        );
    }

    [Fact]
    public async Task DeletesTheListNodesOfInlineRows()
    {
        StoredGraph rows = Graph(new InlineStatusRowsQuery().Value);
        StoredGraph other = Graph(new InlineStatusRowsQuery().Value);
        await WriteAsync(rows, other);
        long lists = rows.Graph.Nodes.Count(node =>
            node.Labels.Contains(GraphLabels.List)
        );
        Assert.True(lists > 0);
        Assert.Equal(lists, await ListCountAsync(rows.Id));

        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQuery(session, rows.Id).ApplyAsync();

        Assert.Equal(0, await ListCountAsync(rows.Id));
        Assert.Equal(lists, await ListCountAsync(other.Id));
    }

    [Fact]
    public async Task DeletesEverySampleAtOnce()
    {
        IReadOnlyList<StoredGraph> samples = [.. QuerySamples.All.Values.Select(Graph)];
        _ = await _database.SeedAsync();
        long before = await _database.NodeCountAsync();
        long relationshipsBefore = await _database.RelationshipCountAsync();
        await WriteAsync([.. samples]);

        await using IAsyncSession session = _driver.AsyncSession();
        await new DeletedQueries(session, [.. samples.Select(s => s.Id)]).ApplyAsync();

        Assert.Equal(before, await _database.NodeCountAsync());
        Assert.Equal(relationshipsBefore, await _database.RelationshipCountAsync());
    }

    public static TheoryData<string> Samples()
    {
        return QuerySamples.Names();
    }

    private static StoredGraph Graph(PureQLQuery query)
    {
        Guid id = Guid.NewGuid();
        return new StoredGraph(id, QueryGraphs.Of(query, id));
    }

    private async Task WriteAsync(params StoredGraph[] graphs)
    {
        await using IAsyncSession session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async runner =>
        {
            foreach (StoredGraph graph in graphs)
            {
                await new InsertedGraph(runner, graph.Graph).ApplyAsync();
            }
        });
    }

    private async Task<long> ListCountAsync(Guid id)
    {
        await using IAsyncSession session = _driver.AsyncSession();
        IResultCursor cursor = await session.RunAsync(
            $"MATCH (n:{GraphLabels.List}:{GraphLabels.Node} {{queryId: $id}}) "
                + "RETURN count(n)",
            new Dictionary<string, object> { ["id"] = id.ToString("D") }
        );
        IRecord record = await cursor.SingleAsync();
        return record[0].As<long>();
    }
}
