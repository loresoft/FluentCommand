using FluentCommand.Import;

namespace FluentCommand.SqlServer.Tests;

public class ImportDefinitionTests
{
    [Fact]
    public void Build_Should_Throw_On_Null_Builder()
    {
        Action act = () => ImportDefinition.Build(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Build_Should_Configure_Properties()
    {
        var definition = ImportDefinition.Build(b => b
            .Name("TestImport")
            .TargetTable("TestTable")
            .CanInsert(false)
            .CanUpdate(true)
            .MaxErrors(5)
            .Field(f => f
                .FieldName("Field1")
                .DataType<string>()
                .CanMap()
            )
        );

        definition.Name.Should().Be("TestImport");
        definition.TargetTable.Should().Be("TestTable");
        definition.CanInsert.Should().BeFalse();
        definition.CanUpdate.Should().BeTrue();
        definition.MaxErrors.Should().Be(5);
        definition.Fields.Should().ContainSingle(f => f.Name == "Field1");
    }
    
    [Fact]
    public void BuildMapping_Should_Map_Fields_By_Regex()
    {
        var definition = new ImportDefinition
        {
            Fields =
            [
                new FieldDefinition
                {
                    Name = "Email",
                    CanMap = true,
                    Expressions = ["^email$", "e-mail"]
                },
                new FieldDefinition
                {
                    Name = "Name",
                    CanMap = true,
                    Expressions = ["^name$"]
                },
                new FieldDefinition
                {
                    Name = "Ignored",
                    CanMap = false
                }
            ]
        };

        var headers = new List<FieldMap>
        {
            new() { Name = "email", Index = 0 },
            new() { Name = "name", Index = 1 }
        };

        var result = definition.BuildMapping(headers);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Email");
        result[0].Index.Should().Be(0);
        result[1].Name.Should().Be("Name");
        result[1].Index.Should().Be(1);
    }

    [Fact]
    public void BuildMapping_Should_Handle_Null_Or_Empty_Headers()
    {
        var definition = new ImportDefinition
        {
            Fields =
            [
                new FieldDefinition
                {
                    Name = "Field1",
                    CanMap = true,
                    Expressions = [".*"]
                }
            ]
        };

        var result1 = definition.BuildMapping(null);
        var result2 = definition.BuildMapping([]);

        result1.Should().ContainSingle(f => f.Name == "Field1" && f.Index == null);
        result2.Should().ContainSingle(f => f.Name == "Field1" && f.Index == null);
    }
}
