# Changelog

All notable changes to `PureQL.CSharp.Model.Graph.Neo4j.Deletion` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0-preview.0.1.0] - 2026-11-09

First release: deletes PureQL query graphs stored in Neo4j by the query id.

### Added

- **`DeleteByIdStatement`** — `sealed record DeleteByIdStatement(Guid Id)`: the
  Cypher that detaches and deletes every node of one stored query.
- **`DeleteByIdsStatement`** — `sealed record
  DeleteByIdsStatement(IReadOnlyCollection<Guid> Ids)`: the same for several
  stored queries.
- **`DeletedQuery`** and **`DeletedQueries`** — run those statements on any
  `IAsyncQueryRunner` (a session or a transaction), so a deletion composes
  inside a caller's transaction.
- **`DeletedQueryOf`** and **`DeletedQueriesOf`** — delete from an
  `INeo4jStoredQueries` store, in a write transaction against its database.
- Deleting an unknown id or an empty set is a no-op.
- Depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.0.0`,
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.0.0` and
  `Neo4j.Driver` `6.5.1`; targets .NET 11 and is AOT-compatible.
