using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion;

/// <summary>
/// One stored query deleted on a query runner: every node of its graph and every
/// relationship attached to them.
/// </summary>
/// <param name="Runner">
/// The runner executing the statement: a session, or a transaction to compose the
/// deletion with other operations.
/// </param>
/// <param name="Id">The id of the query to delete.</param>
/// <remarks>
/// Runs <see cref="DeleteByIdStatement"/>. Deleting a query that is not stored is a
/// no-op.
/// </remarks>
public sealed record DeletedQuery(IAsyncQueryRunner Runner, Guid Id)
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
        IResultCursor cursor = await Runner.RunAsync(new DeleteByIdStatement(Id).Value);
        _ = await cursor.ConsumeAsync();
    }
}
