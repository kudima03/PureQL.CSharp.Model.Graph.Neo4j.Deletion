using Neo4jQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion;

/// <summary>
/// The statement deleting a set of stored queries: every node of their graphs and
/// every relationship attached to them.
/// </summary>
/// <param name="Ids">The ids of the queries to delete.</param>
/// <remarks>
/// The statement is <c>MATCH (n:Node) WHERE n.queryId IN $ids DETACH DELETE n</c>, with
/// the ids passed as the list parameter <c>ids</c> in the Guid "D" format, as the
/// <c>PureQL.CSharp.Model.Graph</c> package writes <c>queryId</c>. Ids of queries that
/// are not stored match nothing; an empty set deletes nothing.
/// </remarks>
public sealed record DeleteByIdsStatement(IReadOnlyCollection<Guid> Ids)
{
    private const string Text =
        $"MATCH (n:{GraphLabels.Node}) "
        + $"WHERE n.{GraphProperties.QueryId} IN $ids "
        + "DETACH DELETE n";

    /// <summary>The Cypher text of the statement and its <c>ids</c> parameter.</summary>
    public Neo4jQuery Value =>
        new Neo4jQuery(
            Text,
            new Dictionary<string, object>
            {
                ["ids"] = Ids.Select(id => id.ToString("D")).ToArray(),
            }
        );
}
