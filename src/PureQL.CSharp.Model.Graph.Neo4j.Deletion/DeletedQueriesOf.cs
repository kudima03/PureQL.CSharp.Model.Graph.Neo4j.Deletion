using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Deletion;

/// <summary>
/// A set of stored queries deleted from a Neo4j database in a write transaction of
/// its own.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
/// <param name="Ids">The ids of the queries to delete.</param>
/// <remarks>
/// Opens a session on <see cref="INeo4jStoredQueries.Driver"/> against
/// <see cref="INeo4jStoredQueries.Database"/> and runs <see cref="DeletedQueries"/>
/// inside <c>ExecuteWriteAsync</c>, which retries transient failures. Ids of queries
/// that are not stored are skipped; an empty set deletes nothing. To compose the
/// deletion with other operations in one transaction, use <see cref="DeletedQueries"/>
/// on that transaction instead.
/// </remarks>
public sealed record DeletedQueriesOf(
    INeo4jStoredQueries Store,
    IReadOnlyCollection<Guid> Ids
)
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
            new DeletedQueries(runner, Ids).ApplyAsync(ct)
        );
    }
}
