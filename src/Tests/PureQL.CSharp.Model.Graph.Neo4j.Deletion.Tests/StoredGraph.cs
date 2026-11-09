namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

/// <summary>A query graph seeded into the test database under its id.</summary>
internal sealed record StoredGraph(Guid Id, QueryGraph Graph)
{
    public long NodeCount => Graph.Nodes.Count;

    public long RelationshipCount => Graph.Relationships.Count;
}
