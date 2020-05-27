using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentCommand.Import;
using Moq;
using Xunit;

namespace FluentCommand.SqlServer.Tests
{
    public class ImportProcessorTests
    {
        [Fact]
        public void CreateTable()
        {
            var userDefinition = ImportDefinition.Build(b => b
                .Name("User")
                .Field(f => f
                    .DisplayName("Email Address")
                    .FieldName("EmailAddress")
                    .DataType<string>()
                    .Required()
                )
                .Field(f => f
                    .DisplayName("First Name")
                    .FieldName("FirstName")
                    .DataType<string>()
                )
                .Field(f => f
                    .DisplayName("Last Name")
                    .FieldName("LastName")
                    .DataType<string>()
                )
                .Field(f => f
                    .DisplayName("Validated")
                    .FieldName("IsValidated")
                    .DataType<bool>()
                )
            );

            userDefinition.Should().NotBeNull();
            userDefinition.Name.Should().Be("User");
            userDefinition.Fields.Count.Should().Be(4);

            var dataSessionMock = new Mock<IDataSession>();
            var importProcessor = new ImportProcessor(dataSessionMock.Object);
            importProcessor.Should().NotBeNull();

            var dataTable = importProcessor.CreateTable(userDefinition);
            dataTable.Should().NotBeNull();
            dataTable.Columns.Count.Should().Be(4);
            dataTable.Columns[0].ColumnName.Should().Be("EmailAddress");
            dataTable.Columns[0].DataType.Should().Be<string>();
            dataTable.Columns[1].ColumnName.Should().Be("FirstName");
            dataTable.Columns[1].DataType.Should().Be<string>();
            dataTable.Columns[2].ColumnName.Should().Be("LastName");
            dataTable.Columns[2].DataType.Should().Be<string>();
            dataTable.Columns[3].ColumnName.Should().Be("IsValidated");
            dataTable.Columns[3].DataType.Should().Be<bool>();
        }

        [Fact]
        public void CreateAndPopulateTable()
        {
            var userDefinition = ImportDefinition.Build(b => b
                .Name("User")
                .Field(f => f
                    .DisplayName("Email Address")
                    .FieldName("EmailAddress")
                    .DataType<string>()
                    .Required()
                )
                .Field(f => f
                    .DisplayName("First Name")
                    .FieldName("FirstName")
                    .DataType<string>()
                )
                .Field(f => f
                    .DisplayName("Last Name")
                    .FieldName("LastName")
                    .DataType<string>()
                )
                .Field(f => f
                    .DisplayName("Validated")
                    .FieldName("IsValidated")
                    .DataType<bool>()
                )
                .Field(f => f
                    .DisplayName("Lockout Count")
                    .FieldName("LockoutCount")
                    .DataType<int?>()
                )
            );

            userDefinition.Should().NotBeNull();
            userDefinition.Name.Should().Be("User");
            userDefinition.Fields.Count.Should().Be(5);

            var importData = new ImportData();
            importData.FileName = "Testing.csv";
            importData.Mappings = new List<FieldMap>
            {
                new FieldMap {Name = "EmailAddress", Index = 0},
                new FieldMap {Name = "IsValidated", Index = 1},
                new FieldMap {Name = "LastName", Index = 2},
                new FieldMap {Name = "FirstName", Index = 3},
                new FieldMap {Name = "LockoutCount", Index = 4},
            };
            importData.Data = new[]
            {
                new[] {"EmailAddress", "IsValidated", "LastName", "FirstName", "LockoutCount"},
                new[] {"user1@email.com", "true", "last1", "first1", ""},
                new[] {"user2@email.com", "false", "", "first2", ""},
                new[] {"user3@email.com", "", "last3", "first3", "2"},
            };

            var dataSessionMock = new Mock<IDataSession>();
            var importProcessor = new ImportProcessor(dataSessionMock.Object);
            importProcessor.Should().NotBeNull();

            var dataTable = importProcessor.CreateTable(userDefinition, importData);
            dataTable.Should().NotBeNull();
            
            dataTable.Columns.Count.Should().Be(5);

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
}
