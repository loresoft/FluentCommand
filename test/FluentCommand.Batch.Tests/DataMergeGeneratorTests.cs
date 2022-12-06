using System;
using System.Collections.Generic;

using FluentAssertions;

using FluentCommand.Entities;
using FluentCommand.Merge;

using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.Batch.Tests;

public class DataMergeGeneratorTests
{
    public DataMergeGeneratorTests(ITestOutputHelper output)
    {
        Output = output;
    }

    public ITestOutputHelper Output { get; }

    [Fact]
    public void ParseIdentifier()
    {
        string result = DataMergeGenerator.ParseIdentifier("[Name]");
        result.Should().Be("Name");

        result = DataMergeGenerator.ParseIdentifier("[Name");
        result.Should().Be("[Name");

        result = DataMergeGenerator.ParseIdentifier("Name]");
        result.Should().Be("Name]");

        result = DataMergeGenerator.ParseIdentifier("[Nam]e]");
        result.Should().Be("Nam]e");

        result = DataMergeGenerator.ParseIdentifier("[]");
        result.Should().Be("");

        result = DataMergeGenerator.ParseIdentifier("B");
        result.Should().Be("B");
    }

    [Fact]
    public void QuoteIdentifier()
    {
        string result = DataMergeGenerator.QuoteIdentifier("[Name]");
        result.Should().Be("[Name]");

        result = DataMergeGenerator.QuoteIdentifier("Name");
        result.Should().Be("[Name]");

        result = DataMergeGenerator.QuoteIdentifier("[Name");
        result.Should().Be("[[Name]");

        result = DataMergeGenerator.QuoteIdentifier("Name]");
        result.Should().Be("[Name]]]");

        result = DataMergeGenerator.QuoteIdentifier("Nam]e");
        result.Should().Be("[Nam]]e]");

        result = DataMergeGenerator.QuoteIdentifier("");
        result.Should().Be("[]");

        result = DataMergeGenerator.QuoteIdentifier("B");
        result.Should().Be("[B]");

    }

    [Fact]
    public void BuildMergeTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<UserImport>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();

        definition.TargetTable = "dbo.User";

        var column = definition.Columns.Find(c => c.SourceColumn == "EmailAddress");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var sql = DataMergeGenerator.BuildMerge(definition);
        sql.Should().NotBeNullOrEmpty();

        Output.WriteLine("MergeStatement:");
        Output.WriteLine(sql);
    }


    [Fact]
    public void BuildMergeDataTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<UserImport>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();

        definition.TargetTable = "dbo.User";

        var column = definition.Columns.Find(c => c.SourceColumn == "EmailAddress");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var users = new List<UserImport>
        {
            new UserImport
            {
                EmailAddress = "test@email.com",
                DisplayName = "Test User",
                FirstName = "Test",
                LastName = "User"
            },
            new UserImport
            {
                EmailAddress = "blah@email.com",
                DisplayName = "Blah User",
                FirstName = "Blah",
                LastName = "User"
            }
        };

        var dataTable = new ListDataReader<UserImport>(users);

        var sql = DataMergeGenerator.BuildMerge(definition, dataTable);
        sql.Should().NotBeNullOrEmpty();

        Output.WriteLine("MergeStatement:");
        Output.WriteLine(sql);
    }

    [Fact]
    public void BuildMergeDataOutputTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<UserImport>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();

        definition.IncludeOutput = true;
        definition.TargetTable = "dbo.User";

        var column = definition.Columns.Find(c => c.SourceColumn == "EmailAddress");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var users = new List<UserImport>
        {
            new UserImport
            {
                EmailAddress = "test@email.com",
                DisplayName = "Test User",
                FirstName = "Test",
                LastName = "User"
            },
            new UserImport
            {
                EmailAddress = "blah@email.com",
                DisplayName = "Blah User",
                FirstName = "Blah",
                LastName = "User"
            },
            new UserImport
            {
                EmailAddress = $"random.{DateTime.Now.Ticks}@email.com",
                DisplayName = "Random User",
                FirstName = "Random",
                LastName = "User"
            }
        };

        var dataTable = new ListDataReader<UserImport>(users);

        var sql = DataMergeGenerator.BuildMerge(definition, dataTable);
        sql.Should().NotBeNullOrEmpty();

        Output.WriteLine("MergeStatement:");
        Output.WriteLine(sql);
    }

}
