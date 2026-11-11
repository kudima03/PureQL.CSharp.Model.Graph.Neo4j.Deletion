# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All `dotnet` commands must be run from the `./src` directory. The projects need the .NET 11 SDK (for the model's C# 15 unions); where the system SDK is older, use `~/.dotnet/dotnet` or put it first on `PATH` with `DOTNET_ROOT=$HOME/.dotnet`.

```bash
dotnet restore
dotnet build --no-restore -warnaserror
dotnet format --verify-no-changes          # check code style (CI enforces this)
dotnet format                              # auto-fix code style
dotnet test --no-build --verbosity normal  # run tests
dotnet csharpier check .                   # check formatting (CI enforces this)
dotnet pack --configuration Release -p:Version=<version> --output .
```

CI additionally passes `-p:AssemblyVersion` (pinned to the major) and `-p:FileVersion`; see `.github/workflows/publish-nuget.yml`.

## Architecture

This is the **deletion package** of the `PureQL.CSharp.Model.Graph` family: it deletes query graphs stored in Neo4j by the query id. It depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.0.0` (the label and property names), `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.0.0` and `Neo4j.Driver` `6.5.1`, never on another operation package. Namespace `PureQL.CSharp.Model.Graph.Neo4j.Deletion`, one public type per file:

- `DeleteByIdStatement` — `sealed record (Guid Id)`; `Value` is the `Neo4j.Driver.Query` `MATCH (n:Node {queryId: $id}) DETACH DELETE n`.
- `DeleteByIdsStatement` — `sealed record (IReadOnlyCollection<Guid> Ids)`; `Value` is `MATCH (n:Node) WHERE n.queryId IN $ids DETACH DELETE n`.
- `DeletedQuery` / `DeletedQueries` — `sealed record (IAsyncQueryRunner Runner, Guid Id)` / `(IAsyncQueryRunner Runner, IReadOnlyCollection<Guid> Ids)`; `ApplyAsync(CancellationToken)` runs the statement on the runner and consumes the cursor. The runner may be a session or a transaction, so the Updating package composes `DeletedQuery` with an insertion in one transaction.
- `DeletedQueryOf` / `DeletedQueriesOf` — the same over `INeo4jStoredQueries Store`: open a session on `Store.Driver` with `WithDatabase(Store.Database)` and run the record above inside `ExecuteWriteAsync`.

Deleting what is not stored is a no-op (an unknown id matches nothing, an empty set deletes nothing), and the statements need no index to be correct (Creation's `pureql_node_query_id` index on `Node(queryId)` only speeds them up). The driver's `RunAsync` takes no cancellation token, so `ApplyAsync` only checks its token before opening the session and before sending the statement.

The Cypher text is a constant built from `GraphLabels` and `GraphProperties`; ids are passed as parameters in the Guid "D" format, as the Graph package writes `queryId`. Inside the package's namespace `Query` resolves to `PureQL.CSharp.Model.Query`, so the driver's type is imported as the alias `Neo4jQuery`. Every public member has XML documentation (`GenerateDocumentationFile`; a missing comment fails the build under `-warnaserror`).

**Test project:** `PureQL.CSharp.Model.Graph.Neo4j.Deletion.Tests` targets `net11.0` and uses xunit with coverlet. The statement tests check the exact Cypher text and parameters. The other tests run against a real server: `Neo4jFixture` starts `neo4j:5-community` with `Testcontainers.Neo4j` (no fixed host ports; Docker required) once for the `SharedNeo4j` collection, so those test classes run one after another. `SeededDatabase.SeedAsync` empties the database, creates a marker node outside any query, and writes five samples of `PureQL.CSharp.Model.Samples`, each under a fresh id, through `QueryGraphs.Of` and `InsertedGraph`: a test-only copy of the Insertion package's `UNWIND` statement (no operation package may be referenced). Assertions count nodes and relationships per `queryId` and in total, so a deletion is checked to remove exactly its graphs.

**Target framework:** net11.0 only, as the model. `IsAotCompatible` is `true`.

**Package validation:** `EnablePackageValidation` is `true` against `PackageValidationBaselineVersion` `0.1.0-preview.0.2.0`, the first release; the `nuget-publish` skill moves it after each release, and intentional breaks go to `CompatibilitySuppressions.xml` next to the csproj.

**Mutation testing:** the Stryker step of CI is commented out.

**Publishing:** triggered by pushing a semver tag (pattern `*.*.*`). The tag becomes the `PackageVersion`. The workflow publishes to both GitHub Packages and NuGet.org.

## Code Style

Enforced via `.editorconfig` and `dotnet format --verify-no-changes` in CI:

- No `var` — always use explicit types
- No expression-bodied methods or constructors — use block bodies
- Expression-bodied properties and accessors are required
- File-scoped namespace declarations (`namespace Foo.Bar;`)
- Private fields prefixed with `_` in camelCase (e.g., `_options`)
- Braces always required, even for single-statement blocks
- Max line length: 90 characters
- Allman brace style — opening brace on its own line for all blocks
- `System.*` using directives sorted first, no blank line between using groups
- Parentheses required in arithmetic and relational binary operators for clarity

## Commit Messages

Do not mention Claude or AI assistance in commit messages.
