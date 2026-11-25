# Changelog

All notable changes to `PureQL.CSharp.Model.Graph.Neo4j.Deletion` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- Tested by writing and deleting every query of `PureQL.CSharp.Model.Samples`
  `0.1.0-preview.4.1.0` (386, was 151: every sample and test of the specification,
  the deeply nested ones included). The package itself is unchanged.

## [0.1.0-preview.1.0.0] - 2026-11-25

Follows `PureQL.CSharp.Model` `0.1.0-preview.15.0.0` (PureQL specification
`0.1.0-preview.3.0.0`). The public API and the statements are unchanged.

### Changed

- **Breaking:** depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.2.0.0` and
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.2.0.0`, both on
  `PureQL.CSharp.Model` `0.1.0-preview.15.0.0`. Deleting by `queryId` also removes the
  `List` nodes that hold the rows of inline rows (`SubqueryRows.Rows`), which carry
  `Node` and `queryId` like every other node of a graph.
- Tested by writing and deleting every query of `PureQL.CSharp.Model.Samples`
  `0.1.0-preview.4.0.0` (151, combined, window, inline-rows and recursive queries
  included).

## [0.1.0-preview.0.2.0] - 2026-11-11

### Changed

- Depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.1.0` and
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.1.0`, the
  .NET 11 releases.
- CI installs the .NET 11 SDK by version instead of pinning it in `global.json`.

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
