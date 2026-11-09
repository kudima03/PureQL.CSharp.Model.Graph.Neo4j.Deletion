# PureQL.CSharp.Model.Graph.Neo4j.Deletion

Deletes **PureQL** query graphs stored in Neo4j.

[![.NET build & test](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Deletion/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Deletion/actions/workflows/build-and-test.yml)
[![Build and Deploy](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Deletion/actions/workflows/publish-nuget.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Deletion/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/PureQL.CSharp.Model.Graph.Neo4j.Deletion)](https://www.nuget.org/packages/PureQL.CSharp.Model.Graph.Neo4j.Deletion)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

`PureQL.CSharp.Model.Graph.Neo4j.Deletion` is the deletion package of the `PureQL.CSharp.Model.Graph` family. A query stored by [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) is a graph of `Node`s that all carry the query's id as `queryId`; deleting the query detaches and deletes every one of them.

## Public API

Namespace `PureQL.CSharp.Model.Graph.Neo4j.Deletion`:

| Name | Description |
|------|-------------|
| `DeleteByIdStatement` | `sealed record DeleteByIdStatement(Guid Id)`; `Value` is the `Neo4j.Driver.Query` `MATCH (n:Node {queryId: $id}) DETACH DELETE n` |
| `DeleteByIdsStatement` | `sealed record DeleteByIdsStatement(IReadOnlyCollection<Guid> Ids)`; `Value` is the `Neo4j.Driver.Query` `MATCH (n:Node) WHERE n.queryId IN $ids DETACH DELETE n` |
| `DeletedQuery` | `sealed record DeletedQuery(IAsyncQueryRunner Runner, Guid Id)`; `ApplyAsync` runs `DeleteByIdStatement` on the runner |
| `DeletedQueries` | `sealed record DeletedQueries(IAsyncQueryRunner Runner, IReadOnlyCollection<Guid> Ids)`; `ApplyAsync` runs `DeleteByIdsStatement` on the runner |
| `DeletedQueryOf` | `sealed record DeletedQueryOf(INeo4jStoredQueries Store, Guid Id)`; `ApplyAsync` opens a session on `Store.Driver` against `Store.Database` and runs `DeletedQuery` in `ExecuteWriteAsync` |
| `DeletedQueriesOf` | `sealed record DeletedQueriesOf(INeo4jStoredQueries Store, IReadOnlyCollection<Guid> Ids)`; the same for `DeletedQueries` |

- Ids are passed in the Guid "D" format, as the Graph package writes `queryId`.
- Deleting what is not stored is a no-op: an unknown id matches nothing, and an empty set deletes nothing.
- `DeletedQuery` and `DeletedQueries` run on any `IAsyncQueryRunner`, so a deletion composes with other operations inside one transaction; the `...Of` records run in a write transaction of their own, which the driver retries on transient failures.
- The driver's `RunAsync` takes no cancellation token: `ApplyAsync` checks its token before opening the session and before sending the statement.

## Usage

```csharp
using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;
using PureQL.CSharp.Model.Graph.Neo4j.Deletion;

await using IDriver driver = GraphDatabase.Driver(
    "neo4j://localhost:7687",
    AuthTokens.Basic("neo4j", "password")
);
INeo4jStoredQueries store = new Neo4jStoredQueries(driver);

// One query, in a transaction of its own:
await new DeletedQueryOf(store, queryId).ApplyAsync();

// The queries of a dashboard, in one statement:
await new DeletedQueriesOf(store, [firstQueryId, secondQueryId]).ApplyAsync();

// Or as part of a larger transaction:
await using IAsyncSession session = driver.AsyncSession(
    builder => builder.WithDatabase(store.Database)
);
await session.ExecuteWriteAsync(
    async transaction =>
    {
        await new DeletedQuery(transaction, queryId).ApplyAsync();
        // ...other operations on the same transaction
    }
);
```

## Design Principles

- **Statements, then execution** — the Cypher is a value you can inspect or run yourself; the executing records only send it
- **AOT-compatible** — `IsAotCompatible=true`

## Dependencies

- [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) `0.1.0-preview.1.0.0` — `GraphLabels`, `GraphProperties`
- [`PureQL.CSharp.Model.Graph.Neo4j.Abstractions`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Abstractions) `0.1.0-preview.1.0.0` — `INeo4jStoredQueries`
- [`Neo4j.Driver`](https://www.nuget.org/packages/Neo4j.Driver) `6.5.1`

## Target Frameworks

- .NET 11

## Installation

```
dotnet add package PureQL.CSharp.Model.Graph.Neo4j.Deletion
```

## Related packages

- [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) — maps a `PureQLQuery` to a property graph and back, without a driver
- [`PureQL.CSharp.Model.Graph.Neo4j.Abstractions`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Abstractions) — the stored-queries contract the Neo4j packages share
- [`PureQL.CSharp.Model.Graph.Neo4j.Creation`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Creation) — constraints and indexes
- [`PureQL.CSharp.Model.Graph.Neo4j.Insertion`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Insertion) — writes a query
- [`PureQL.CSharp.Model.Graph.Neo4j.Querying`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying) — reads queries back
- [`PureQL.CSharp.Model.Graph.Neo4j.Updating`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Updating) — replaces a query in one transaction
- [`PureQL.CSharp.Model.Graph.Neo4j.Existence`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Existence) — checks the schema and queries exist
- [`PureQL.CSharp.Model.Graph.Neo4j.Destruction`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction) — drops the schema and every stored query
- [`PureQL.CSharp.Model.Graph.Neo4j.Migration`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Migration) — migrates stored graphs between model versions
