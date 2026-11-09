namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

public sealed record DeleteByIdStatementTests
{
    private static readonly Guid Id = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");

    [Fact]
    public void MatchesNodesOfQueryAndDetachDeletesThem()
    {
        Assert.Equal(
            "MATCH (n:Node {queryId: $id}) DETACH DELETE n",
            new DeleteByIdStatement(Id).Value.Text
        );
    }

    [Fact]
    public void PassesIdAsOnlyParameter()
    {
        Assert.Equal(
            "id",
            Assert.Single(new DeleteByIdStatement(Id).Value.Parameters.Keys)
        );
    }

    [Fact]
    public void PassesIdInDFormat()
    {
        Assert.Equal(
            "0f8fad5b-d9cb-469f-a165-70867728950e",
            new DeleteByIdStatement(Id).Value.Parameters["id"]
        );
    }

    [Fact]
    public void PassesEmptyId()
    {
        Assert.Equal(
            "00000000-0000-0000-0000-000000000000",
            new DeleteByIdStatement(Guid.Empty).Value.Parameters["id"]
        );
    }

    [Fact]
    public void KeepsId()
    {
        Assert.Equal(Id, new DeleteByIdStatement(Id).Id);
    }

    [Fact]
    public void BuildsEqualQueriesOnEveryRead()
    {
        DeleteByIdStatement statement = new DeleteByIdStatement(Id);
        Assert.Equal(statement.Value.Parameters["id"], statement.Value.Parameters["id"]);
    }

    [Fact]
    public void EqualsWithSameId()
    {
        Assert.Equal(new DeleteByIdStatement(Id), new DeleteByIdStatement(Id));
    }

    [Fact]
    public void DiffersWithOtherId()
    {
        Assert.NotEqual(new DeleteByIdStatement(Id), new DeleteByIdStatement(Guid.Empty));
    }
}
