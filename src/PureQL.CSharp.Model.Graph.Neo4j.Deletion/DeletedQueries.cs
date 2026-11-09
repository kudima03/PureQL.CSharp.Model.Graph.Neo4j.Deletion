using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion;

/// <summary>
/// A set of stored queries deleted on a query runner: every node of their graphs and
/// every relationship attached to them.
/// </summary>
/// <param name="Runner">
/// The runner executing the statement: a session, or a transaction to compose the
/// deletion with other operations.
/// </param>
/// <param name="Ids">The ids of the queries to delete.</param>
/// <remarks>
/// Runs <see cref="DeleteByIdsStatement"/> once for the whole set. Ids of queries that
/// are not stored are skipped; an empty set deletes nothing.
/// </remarks>
public sealed record DeletedQueries(
    IAsyncQueryRunner Runner,
    IReadOnlyCollection<Guid> Ids
)
{
    /// <summary>Runs the deletion on <see cref="Runner"/>.</summary>
    /// <param name="ct">
    /// Checked before the statement is sent; the driver's <c>RunAsync</c> takes no
    /// cancellation token.
    /// </param>
    /// <returns>A task completing once the server has applied the deletion.</returns>
    public async Task ApplyAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IResultCursor cursor = await Runner.RunAsync(new DeleteByIdsStatement(Ids).Value);
        _ = await cursor.ConsumeAsync();
    }
}
