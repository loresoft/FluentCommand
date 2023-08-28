# FluentCommand

Fluent Wrapper for DbCommand.

[![Build status](https://github.com/loresoft/FluentCommand/workflows/Build/badge.svg)](https://github.com/loresoft/FluentCommand/actions)

[![Coverage Status](https://coveralls.io/repos/github/loresoft/FluentCommand/badge.svg?branch=master)](https://coveralls.io/github/loresoft/FluentCommand?branch=master)

| Package | Version |
| :--- | :--- |
| [FluentCommand](https://www.nuget.org/packages/FluentCommand/) |  [![FluentCommand](https://img.shields.io/nuget/v/FluentCommand.svg)](https://www.nuget.org/packages/FluentCommand/) |
| [FluentCommand.SqlServer](https://www.nuget.org/packages/FluentCommand.SqlServer/) |  [![FluentCommand.SqlServer](https://img.shields.io/nuget/v/FluentCommand.SqlServer.svg)](https://www.nuget.org/packages/FluentCommand.SqlServer/) |
| [FluentCommand.Json](https://www.nuget.org/packages/FluentCommand.Json/) |  [![FluentCommand.Json](https://img.shields.io/nuget/v/FluentCommand.Json.svg)](https://www.nuget.org/packages/FluentCommand.Json/) |

## Download

The FluentCommand library is available on nuget.org via package name `FluentCommand`.

To install FluentCommand, run the following command in the Package Manager Console

    PM> Install-Package FluentCommand

More information about NuGet package available at
<https://nuget.org/packages/FluentCommand>

## Features

- Fluent wrapper over DbConnection and DbCommand
- Callback for parameter return values
- Automatic handling of connection state
- Caching of results
- Automatic creating of entity from DataReader via Dapper
- Create Dynamic objects from DataReader via Dapper
- Handles multiple result sets
- Basic SQL query builder
- Source Generate DataReader
