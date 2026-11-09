using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion;

/// <summary>
/// One stored query deleted from a Neo4j database in a write transaction of its own.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
/// <param name="Id">The id of the query to delete.</param>
/// <remarks>
/// Opens a session on <see cref="INeo4jStoredQueries.Driver"/> against
/// <see cref="INeo4jStoredQueries.Database"/> and runs <see cref="DeletedQuery"/> inside
/// <c>ExecuteWriteAsync</c>, which retries transient failures. Deleting a query that is
/// not stored is a no-op. To compose the deletion with other operations in one
/// transaction, use <see cref="DeletedQuery"/> on that transaction instead.
/// </remarks>
public sealed record DeletedQueryOf(INeo4jStoredQueries Store, Guid Id)
{
    /// <summary>Runs the deletion in a write transaction and commits it.</summary>
    /// <param name="ct">
    /// Checked before the session is opened and before the statement is sent.
    /// </param>
    /// <returns>A task completing once the transaction has committed.</returns>
    public async Task ApplyAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await using IAsyncSession session = Store.Driver.AsyncSession(builder =>
            builder.WithDatabase(Store.Database)
        );
        await session.ExecuteWriteAsync(runner =>
            new DeletedQuery(runner, Id).ApplyAsync(ct)
        );
    }
}
