# FluentCommand

Fluent wrapper for ADO.NET `DbCommand` with automatic object mapping, caching, query building, and source-generated data readers.

[![Build status](https://github.com/loresoft/FluentCommand/workflows/Build/badge.svg)](https://github.com/loresoft/FluentCommand/actions)

[![Coverage Status](https://coveralls.io/repos/github/loresoft/FluentCommand/badge.svg?branch=master)](https://coveralls.io/github/loresoft/FluentCommand?branch=master)

| Package | Version |
| :--- | :--- |
| [FluentCommand](https://www.nuget.org/packages/FluentCommand/) | [![FluentCommand](https://img.shields.io/nuget/v/FluentCommand.svg)](https://www.nuget.org/packages/FluentCommand/) |
| [FluentCommand.SqlServer](https://www.nuget.org/packages/FluentCommand.SqlServer/) | [![FluentCommand.SqlServer](https://img.shields.io/nuget/v/FluentCommand.SqlServer.svg)](https://www.nuget.org/packages/FluentCommand.SqlServer/) |
| [FluentCommand.Caching](https://www.nuget.org/packages/FluentCommand.Caching/) | [![FluentCommand.Caching](https://img.shields.io/nuget/v/FluentCommand.Caching.svg)](https://www.nuget.org/packages/FluentCommand.Caching/) |

## Installation

```shell
dotnet add package FluentCommand
```

For SQL Server bulk copy, merge, and import features:

```shell
dotnet add package FluentCommand.SqlServer
```

For distributed caching:

```shell
dotnet add package FluentCommand.Caching
```

## Features

- Fluent wrapper over `DbConnection` and `DbCommand`
- Automatic connection state management
- Source-generated `IDataReader` mapping (no reflection)
- SQL query builder with Select, Insert, Update, Delete, and Upsert support
- JSON column support with `[JsonColumn]` attribute for source-generated readers
- JSON and CSV export directly from query results
- JSON parameter serialization with `ParameterJson`
- Parameterized queries with output, input-output, and return value callbacks
- Conditional parameters and query builder filters (`ParameterIf`, `WhereIf`, `ValueIf`)
- Result caching with sliding or absolute expiration
- Distributed cache integration via `FluentCommand.Caching`
- Query logging with elapsed time and parameter details
- Connection and command interceptors
- Multiple result set handling
- Multiple database configuration with discriminated registrations
- SQL Server bulk copy and merge data operations
- Tabular data import with field mapping, validation, and merge
- Multi-target: `netstandard2.0`, `net8.0`, `net9.0`, `net10.0`
- Supports SQL Server, PostgreSQL, and SQLite
