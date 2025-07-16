using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Import;

namespace FluentCommand.SqlServer.Tests;

public class ImportDefinitionBuilderTests
{
    [Table("TestModel", Schema = "UT")]
    private class TestModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }

        [Column("Created", TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string NotMappedProperty { get; set; } = "";
    }

    [Fact]
    public void AutoMap_Should_Map_All_Eligible_Properties()
    {
        var definition = ImportDefinitionBuilder<TestModel>.Build(b => b.AutoMap());

        definition.Should().NotBeNull();
        definition.Name.Should().Be("TestModel");
        definition.TargetTable.Should().Be("UT.TestModel");
        definition.Fields.Should().Contain(f => f.Name == nameof(TestModel.Id));
        definition.Fields.Should().Contain(f => f.Name == nameof(TestModel.Name));
        definition.Fields.Should().Contain(f => f.Name == nameof(TestModel.IsActive));
        definition.Fields.Should().Contain(f => f.Name == "Created");
        definition.Fields.Should().NotContain(f => f.Name == nameof(TestModel.NotMappedProperty));

        definition.Fields.First(f => f.Name == nameof(TestModel.Id)).IsKey.Should().BeTrue();
    }

    [Fact]
    public void Field_Should_Allow_Explicit_Field_Configuration()
    {
        var definition = ImportDefinitionBuilder<TestModel>.Build(b => b
            .Field(m => m.Name)
                .DisplayName("Custom Name")
                .Required()
        );

        var field = definition.Fields.FirstOrDefault(f => f.Name == nameof(TestModel.Name));
        field.Should().NotBeNull();
        field.DisplayName.Should().Be("Custom Name");
        field.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void Name_And_TargetTable_Should_Set_Properties()
    {
        var definition = ImportDefinitionBuilder<TestModel>.Build(b => b
            .Name("MyImport")
            .TargetTable("dbo.MyTable")
        );

        definition.Name.Should().Be("MyImport");
        definition.TargetTable.Should().Be("dbo.MyTable");
    }

    [Fact]
    public void CanInsert_And_CanUpdate_Should_Set_Properties()
    {
        var definition = ImportDefinitionBuilder<TestModel>.Build(b => b
            .CanInsert(false)
            .CanUpdate(false)
        );

        definition.CanInsert.Should().BeFalse();
        definition.CanUpdate.Should().BeFalse();
    }
}
