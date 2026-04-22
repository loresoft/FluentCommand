using FluentCommand.Entities;
using FluentCommand.Reflection;

namespace FluentCommand.Tests.Reflection;

public class TypeAccessorTests
{
    [Fact]
    public void WhenTypeHasTableAttributeThenGetAccessorReturnsGeneratedTypeAccessor()
    {
        // Act
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Assert
        accessor.Should().NotBeNull();
        accessor.GetType().Name.Should().Be("StatusGeneratedTypeAccessor");
    }

    [Fact]
    public void WhenGeneratedAccessorThenTableNameMatchesAttribute()
    {
        // Act
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Assert
        accessor.TableName.Should().Be("Status");
        accessor.TableSchema.Should().Be("dbo");
    }

    [Fact]
    public void WhenGeneratedAccessorThenCreateReturnsInstance()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var instance = accessor.Create();

        // Assert
        instance.Should().BeOfType<Status>();
    }

    [Fact]
    public void WhenGeneratedAccessorThenFindReturnsGeneratedMemberAccessor()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.Find("Name");

        // Assert
        member.Should().NotBeNull();
        member.Name.Should().Be("Name");
        member.MemberType.Should().Be(typeof(string));
        member.GetType().Name.Should().NotBe(nameof(PropertyAccessor));
    }

    [Fact]
    public void WhenGeneratedAccessorThenGetSetValueWorks()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();
        var status = new Status { Name = "Active" };
        var member = accessor.Find("Name")!;

        // Act
        var value = member.GetValue(status);
        member.SetValue(status, "Inactive");

        // Assert
        value.Should().Be("Active");
        status.Name.Should().Be("Inactive");
    }

    [Fact]
    public void WhenGeneratedAccessorThenGetPropertiesReturnsGeneratedMembers()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var properties = accessor.GetProperties().ToList();

        // Assert
        properties.Should().NotBeEmpty();
        properties.Should().Contain(p => p.Name == "Id");
        properties.Should().Contain(p => p.Name == "Name");
        properties.Should().Contain(p => p.Name == "IsActive");
    }

    [Fact]
    public void WhenGeneratedAccessorThenFindColumnReturnsGeneratedMember()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.FindColumn("Name");

        // Assert
        member.Should().NotBeNull();
        member.Name.Should().Be("Name");
    }

    [Fact]
    public void WhenGeneratedAccessorThenFindPropertyReturnsGeneratedMember()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.FindProperty("Id");

        // Assert
        member.Should().NotBeNull();
        member.Name.Should().Be("Id");
        member.MemberType.Should().Be(typeof(int));
    }

    #region Table Attribute

    [Fact]
    public void WhenTableHasSchemaAttributeThenSchemaIsRead()
    {
        // Act
        var accessor = TypeAccessor.GetAccessor<Member>();

        // Assert
        accessor.TableName.Should().Be("member_user");
        accessor.TableSchema.Should().Be("dbo");
    }

    [Fact]
    public void WhenTableHasNoSchemaThenSchemaIsNull()
    {
        // Act
        var accessor = TypeAccessor.GetAccessor<UserImport>();

        // Assert
        accessor.TableName.Should().Be("User");
        accessor.TableSchema.Should().BeNull();
    }

    [Fact]
    public void WhenTableNameHasSpecialCharactersThenNameIsPreserved()
    {
        // Act
        var accessor = TypeAccessor.GetAccessor<TableTest>();

        // Assert
        accessor.TableName.Should().Be("Table1 $ Test");
        accessor.TableSchema.Should().Be("dbo");
    }

    #endregion

    #region Key Attribute

    [Fact]
    public void WhenPropertyHasKeyAttributeThenIsKeyIsTrue()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<DataType>();

        // Act
        var member = accessor.Find("Id");

        // Assert
        member.Should().NotBeNull();
        member.IsKey.Should().BeTrue();
    }

    [Fact]
    public void WhenPropertyHasNoKeyAttributeThenIsKeyIsFalse()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<DataType>();

        // Act
        var member = accessor.Find("Name");

        // Assert
        member.Should().NotBeNull();
        member.IsKey.Should().BeFalse();
    }

    [Fact]
    public void WhenPropertyHasKeyAndColumnAttributeThenBothAreRead()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<TableTest>();

        // Act
        var member = accessor.Find("Test");

        // Assert
        member.Should().NotBeNull();
        member.IsKey.Should().BeTrue();
        member.Column.Should().Be("Test$");
    }

    #endregion

    #region Column Attribute

    [Fact]
    public void WhenPropertyHasColumnAttributeThenColumnNameIsMapped()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Member>();

        // Act
        var emailMember = accessor.Find("EmailAddress");

        // Assert
        emailMember.Should().NotBeNull();
        emailMember.Name.Should().Be("EmailAddress");
        emailMember.Column.Should().Be("email_address");
    }

    [Fact]
    public void WhenPropertyHasNoColumnAttributeThenColumnMatchesPropertyName()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.Find("Name");

        // Assert
        member.Should().NotBeNull();
        member.Column.Should().Be("Name");
    }

    [Fact]
    public void WhenColumnNameHasSpecialCharactersThenNameIsPreserved()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<TableTest>();

        // Act
        var member = accessor.Find("TableExampleID");

        // Assert
        member.Should().NotBeNull();
        member.Column.Should().Be("Table Example ID");
    }

    [Fact]
    public void WhenColumnNameStartsWithNumberThenNameIsPreserved()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<TableTest>();

        // Act
        var member = accessor.Find("FirstNumber");

        // Assert
        member.Should().NotBeNull();
        member.Column.Should().Be("1stNumber");
    }

    [Fact]
    public void WhenFindColumnByMappedNameThenMemberIsReturned()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Member>();

        // Act
        var member = accessor.FindColumn("email_address");

        // Assert
        member.Should().NotBeNull();
        member.Name.Should().Be("EmailAddress");
        member.Column.Should().Be("email_address");
    }

    [Fact]
    public void WhenFindByPropertyNameAndFindColumnByMappedNameThenSameAccessorIsReturned()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Member>();

        // Act
        var byProperty = accessor.Find("FirstName");
        var byColumn = accessor.FindColumn("first_name");

        // Assert
        byProperty.Should().NotBeNull();
        byColumn.Should().NotBeNull();
        byProperty.Should().BeSameAs(byColumn);
    }

    [Fact]
    public void WhenMultipleColumnsAreMappedThenAllAreAccessible()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Member>();

        // Act & Assert
        var firstName = accessor.FindColumn("first_name");
        firstName.Should().NotBeNull();
        firstName.Name.Should().Be("FirstName");

        var lastName = accessor.FindColumn("last_name");
        lastName.Should().NotBeNull();
        lastName.Name.Should().Be("LastName");

        var displayName = accessor.FindColumn("display_name");
        displayName.Should().NotBeNull();
        displayName.Name.Should().Be("DisplayName");
    }

    #endregion

    #region ConcurrencyCheck and DatabaseGenerated Attributes

    [Fact]
    public void WhenPropertyHasConcurrencyCheckAttributeThenIsConcurrencyCheckIsTrue()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.Find("RowVersion");

        // Assert
        member.Should().NotBeNull();
        member.IsConcurrencyCheck.Should().BeTrue();
    }

    [Fact]
    public void WhenPropertyHasDatabaseGeneratedAttributeThenIsDatabaseGeneratedIsTrue()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.Find("RowVersion");

        // Assert
        member.Should().NotBeNull();
        member.IsDatabaseGenerated.Should().BeTrue();
    }

    [Fact]
    public void WhenPropertyHasNoConcurrencyCheckAttributeThenIsConcurrencyCheckIsFalse()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.Find("Name");

        // Assert
        member.Should().NotBeNull();
        member.IsConcurrencyCheck.Should().BeFalse();
        member.IsDatabaseGenerated.Should().BeFalse();
    }

    #endregion

    #region NotMapped Attribute

    [Fact]
    public void WhenPropertyHasNotMappedAttributeThenIsNotMappedIsTrue()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.Find("Tasks");

        // Assert
        member.Should().NotBeNull();
        member.IsNotMapped.Should().BeTrue();
    }

    [Fact]
    public void WhenPropertyHasNotMappedAttributeThenPropertyIsIncludedInGetProperties()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var properties = accessor.GetProperties().ToList();

        // Assert — [NotMapped] properties are included in the TypeAccessor
        properties.Should().Contain(p => p.Name == "Tasks");
    }

    [Fact]
    public void WhenPropertyHasNoNotMappedAttributeThenIsNotMappedIsFalse()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.Find("Name");

        // Assert
        member.Should().NotBeNull();
        member.IsNotMapped.Should().BeFalse();
    }

    [Fact]
    public void WhenMultiplePropertiesAreNotMappedThenAllHaveIsNotMappedTrue()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<User>();

        // Act
        var properties = accessor.GetProperties().ToList();

        // Assert
        properties.Should().Contain(p => p.Name == "Audits" && p.IsNotMapped);
        properties.Should().Contain(p => p.Name == "AssignedTasks" && p.IsNotMapped);
        properties.Should().Contain(p => p.Name == "CreatedTasks" && p.IsNotMapped);
        properties.Should().Contain(p => p.Name == "Roles" && p.IsNotMapped);
        properties.Should().Contain(p => p.Name == "EmailAddress" && !p.IsNotMapped);
        properties.Should().Contain(p => p.Name == "DisplayName" && !p.IsNotMapped);
    }

    #endregion

    #region HasGetter and HasSetter

    [Fact]
    public void WhenPropertyHasGetterAndSetterThenBothAreTrue()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<Status>();

        // Act
        var member = accessor.Find("Name");

        // Assert
        member.Should().NotBeNull();
        member.HasGetter.Should().BeTrue();
        member.HasSetter.Should().BeTrue();
    }

    [Fact]
    public void WhenPropertyHasInitOnlySetterThenHasSetterIsFalse()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<StatusReadOnly>();

        // Act
        var member = accessor.Find("Name");

        // Assert
        member.Should().NotBeNull();
        member.HasGetter.Should().BeTrue();
        member.HasSetter.Should().BeFalse();
    }

    [Fact]
    public void WhenConstructorInitializedTypeHasGetterOnlyThenHasSetterIsFalse()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<StatusConstructor>();

        // Act
        var member = accessor.Find("Name");

        // Assert
        member.Should().NotBeNull();
        member.HasGetter.Should().BeTrue();
        member.HasSetter.Should().BeFalse();
    }

    #endregion

    #region MemberType

    [Fact]
    public void WhenPropertyIsIntThenMemberTypeIsInt()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<DataType>();

        // Act
        var member = accessor.Find("Short");

        // Assert
        member.Should().NotBeNull();
        member.MemberType.Should().Be(typeof(short));
    }

    [Fact]
    public void WhenPropertyIsNullableThenMemberTypeIsNullable()
    {
        // Arrange
        var accessor = TypeAccessor.GetAccessor<DataType>();

        // Act
        var member = accessor.Find("GuidNull");

        // Assert
        member.Should().NotBeNull();
        member.MemberType.Should().Be(typeof(Guid?));
    }

    #endregion
}
