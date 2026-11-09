using Neo4j.Driver;
using PureQL.CSharp.Model.Samples.Queries.GroupBy;
using PureQL.CSharp.Model.Samples.Queries.Joins;
using PureQL.CSharp.Model.Samples.Queries.Pagination;
using PureQL.CSharp.Model.Samples.Queries.Select;
using PureQL.CSharp.Model.Samples.Queries.Subqueries;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

/// <summary>
/// The test database emptied and filled with sample query graphs, one id each, next
/// to a node that belongs to no query.
/// </summary>
internal sealed record SeededDatabase(IDriver Driver)
{
    public const string MarkerLabel = "PureQLModelVersion";

    private static IReadOnlyList<PureQLQuery> Queries =>
        [
            new InnerJoinQuery().Value,
            new FilterOrderAndPageQuery().Value,
            new SubqueryPipelineQuery().Value,
            new LiteralOfEveryTypeNextToFieldQuery().Value,
            new GroupByCountQuery().Value,
        ];

    public async Task<IReadOnlyList<StoredGraph>> SeedAsync()
    {
        IReadOnlyList<StoredGraph> graphs =
        [
            .. Queries.Select(query =>
            {
                Guid id = Guid.NewGuid();
                return new StoredGraph(id, QueryGraphs.Of(query, id));
            }),
        ];
        await using IAsyncSession session = Driver.AsyncSession();
        await session.ExecuteWriteAsync(async runner =>
        {
            IResultCursor cleared = await runner.RunAsync("MATCH (n) DETACH DELETE n");
            _ = await cleared.ConsumeAsync();
            IResultCursor marked = await runner.RunAsync(
                $"CREATE (:{MarkerLabel} {{value: 'marker'}})"
            );
            _ = await marked.ConsumeAsync();
            foreach (StoredGraph graph in graphs)
            {
                await new InsertedGraph(runner, graph.Graph).ApplyAsync();
            }
        });
        return graphs;
    }

    public Task<long> NodeCountAsync()
    {
        return ScalarAsync("MATCH (n) RETURN count(n)", null);
    }

    public Task<long> RelationshipCountAsync()
    {
        return ScalarAsync("MATCH ()-[r]->() RETURN count(r)", null);
    }

    public Task<long> NodeCountAsync(Guid id)
    {
        return ScalarAsync(
            "MATCH (n {queryId: $id}) RETURN count(n)",
            new Dictionary<string, object> { ["id"] = id.ToString("D") }
        );
    }

    public Task<long> RelationshipCountAsync(Guid id)
    {
        return ScalarAsync(
            "MATCH (n {queryId: $id})-[r]-() RETURN count(DISTINCT r)",
            new Dictionary<string, object> { ["id"] = id.ToString("D") }
        );
    }

    public Task<long> MarkerCountAsync()
    {
        return ScalarAsync($"MATCH (n:{MarkerLabel}) RETURN count(n)", null);
    }

    private async Task<long> ScalarAsync(
        string text,
        IDictionary<string, object>? parameters
    )
    {
        await using IAsyncSession session = Driver.AsyncSession();
        IResultCursor cursor = await session.RunAsync(
            text,
            parameters ?? new Dictionary<string, object>()
        );
        IRecord record = await cursor.SingleAsync();
        return record[0].As<long>();
    }
}
