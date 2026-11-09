namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests;

public sealed record DeleteByIdsStatementTests
{
    private static readonly Guid First = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");

    private static readonly Guid Second = new Guid(
        "7c9e6679-7425-40de-944b-e07fc1f90ae7"
    );

    [Fact]
    public void MatchesNodesOfQueriesAndDetachDeletesThem()
    {
        Assert.Equal(
            "MATCH (n:Node) WHERE n.queryId IN $ids DETACH DELETE n",
            new DeleteByIdsStatement([First, Second]).Value.Text
        );
    }

    [Fact]
    public void PassesIdsAsOnlyParameter()
    {
        Assert.Equal(
            "ids",
            Assert.Single(new DeleteByIdsStatement([First]).Value.Parameters.Keys)
        );
    }

    [Fact]
    public void PassesIdsInDFormatAndOrder()
    {
        string[] expected =
        [
            "7c9e6679-7425-40de-944b-e07fc1f90ae7",
            "0f8fad5b-d9cb-469f-a165-70867728950e",
        ];
        Assert.Equal(
            expected,
            new DeleteByIdsStatement([Second, First]).Value.Parameters["ids"]
        );
    }

    [Fact]
    public void PassesIdsAsStringArray()
    {
        _ = Assert.IsType<string[]>(
            new DeleteByIdsStatement([First, Second]).Value.Parameters["ids"]
        );
    }

    [Fact]
    public void PassesEmptyListWithoutIds()
    {
        Assert.Equal(
            Array.Empty<string>(),
            new DeleteByIdsStatement([]).Value.Parameters["ids"]
        );
    }

    [Fact]
    public void KeepsTextWithoutIds()
    {
        Assert.Equal(
            "MATCH (n:Node) WHERE n.queryId IN $ids DETACH DELETE n",
            new DeleteByIdsStatement([]).Value.Text
        );
    }

    [Fact]
    public void ReadsIdsOnEveryRead()
    {
        List<Guid> ids = [First];
        DeleteByIdsStatement statement = new DeleteByIdsStatement(ids);
        _ = statement.Value;
        ids.Add(Second);
        string[] expected =
        [
            "0f8fad5b-d9cb-469f-a165-70867728950e",
            "7c9e6679-7425-40de-944b-e07fc1f90ae7",
        ];
        Assert.Equal(expected, statement.Value.Parameters["ids"]);
    }

    [Fact]
    public void KeepsIds()
    {
        IReadOnlyCollection<Guid> ids = [First, Second];
        Assert.Same(ids, new DeleteByIdsStatement(ids).Ids);
    }

    [Fact]
    public void EqualsWithSameIds()
    {
        IReadOnlyCollection<Guid> ids = [First, Second];
        Assert.Equal(new DeleteByIdsStatement(ids), new DeleteByIdsStatement(ids));
    }
}
