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

Ids are passed in the Guid "D" format, as the Graph package writes `queryId`.

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
