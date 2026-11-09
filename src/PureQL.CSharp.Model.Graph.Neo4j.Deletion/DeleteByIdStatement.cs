using Neo4jQuery = Neo4j.Driver.Query;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion;

/// <summary>
/// The statement deleting one stored query: every node of its graph and every
/// relationship attached to them.
/// </summary>
/// <param name="Id">The id of the query to delete.</param>
/// <remarks>
/// The statement is <c>MATCH (n:Node {queryId: $id}) DETACH DELETE n</c>, with the id
/// passed as the parameter <c>id</c> in the Guid "D" format, as the
/// <c>PureQL.CSharp.Model.Graph</c> package writes <c>queryId</c>. It matches nothing,
/// and so deletes nothing, when no query with that id is stored.
/// </remarks>
public sealed record DeleteByIdStatement(Guid Id)
{
    private const string Text =
        $"MATCH (n:{GraphLabels.Node} {{{GraphProperties.QueryId}: $id}}) "
        + "DETACH DELETE n";

    /// <summary>The Cypher text of the statement and its <c>id</c> parameter.</summary>
    public Neo4jQuery Value =>
        new Neo4jQuery(
            Text,
            new Dictionary<string, object> { ["id"] = Id.ToString("D") }
        );
}
