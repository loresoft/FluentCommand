using System;

using FluentAssertions;

using FluentCommand.Batch.Fluent;
using FluentCommand.Batch.Validation;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace FluentCommand.Batch.Tests;

public class BatchProcessorTests
{
    [Fact]
    public void CreateTable()
    {
        var batchJob = new BatchJob();
        var builder = new BatchBuilder(batchJob)
            .Description("User")
            .TargetTable("dbo.User")
            .CanInsert()
            .CanUpdate()
            .Field(f => f
               .Name("EmailAddress")
               .DisplayName("Email Address")
               .DataType<string>()
               .IsKey()
            )
            .Field(f => f
                .Name("FirstName")
                .DisplayName("First Name")
                .DataType<string>()
            )
            .Field(f => f
                .Name("LastName")
                .DisplayName("Last Name")
                .DataType<string>()
            )
            .Field(f => f
                .Name("IsValidated")
                .DisplayName("Validated")
                .DataType<bool>()
            )
            .Field(f => f
                .Name("LockoutCount")
                .DisplayName("Lockout Count")
                .DataType<int?>()
                .CanBeNull()
            )
            .Field(f => f
                .Name("Updated")
                .DisplayName("Updated")
                .DataType<DateTimeOffset>()
                .Default(FieldDefault.CurrentDate)
            );

        batchJob.Should().NotBeNull();
        batchJob.Fields.Count.Should().Be(6);

        batchJob.FileName = "Testing.csv";

        builder.Field("EmailAddress", f => f.Index(0));
        builder.Field("IsValidated", f => f.Index(1));
        builder.Field("LastName", f => f.Index(2));
        builder.Field("FirstName", f => f.Index(3));
        builder.Field("LockoutCount", f => f.Index(4));

        batchJob.Data = new[]
        {
            new[] {"email", "validated", "last", "first", "lockout"},
            new[] {"user1@email.com", "true", "last1", "first1", ""},
            new[] {"user2@email.com", "false", "", "first2", ""},
            new[] {"user3@email.com", "", "last3", "first3", "2"},
        };

        var batchFactory = new BatchFactory(new[] { new BatchValidator() }, null);
        var batchProcessor = new BatchProcessor(NullLogger<BatchProcessor>.Instance, batchFactory, null);
        batchProcessor.Should().NotBeNull();

        var dataTable = batchProcessor.CreateTable(batchJob);
        dataTable.Should().NotBeNull();

        dataTable.Columns.Count.Should().Be(6);

        dataTable.Columns[0].ColumnName.Should().Be("EmailAddress");
        dataTable.Columns[0].DataType.Should().Be<string>();

        dataTable.Columns[1].ColumnName.Should().Be("FirstName");
        dataTable.Columns[1].DataType.Should().Be<string>();

        dataTable.Columns[2].ColumnName.Should().Be("LastName");
        dataTable.Columns[2].DataType.Should().Be<string>();

        dataTable.Columns[3].ColumnName.Should().Be("IsValidated");
        dataTable.Columns[3].DataType.Should().Be<bool>();

        dataTable.Columns[4].ColumnName.Should().Be("LockoutCount");
        dataTable.Columns[4].DataType.Should().Be<int>();

        dataTable.Rows.Count.Should().Be(3);

        dataTable.Rows[0][0].Should().Be("user1@email.com");
        dataTable.Rows[0][1].Should().Be("first1");
        dataTable.Rows[0][2].Should().Be("last1");
        dataTable.Rows[0][3].Should().Be(true);
        dataTable.Rows[0][4].Should().Be(DBNull.Value);

        dataTable.Rows[1][0].Should().Be("user2@email.com");
        dataTable.Rows[1][1].Should().Be("first2");
        dataTable.Rows[1][2].Should().Be("");
        dataTable.Rows[1][3].Should().Be(false);
        dataTable.Rows[1][4].Should().Be(DBNull.Value);

        dataTable.Rows[2][0].Should().Be("user3@email.com");
        dataTable.Rows[2][1].Should().Be("first3");
        dataTable.Rows[2][2].Should().Be("last3");
        dataTable.Rows[2][3].Should().Be(false);
        dataTable.Rows[2][4].Should().Be(2);

    }

}
