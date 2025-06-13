namespace FluentCommand.Tests;

public class ListDataReaderTests
{
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
    }

    [Fact]
    public void Read_ShouldIterateOverAllItems()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Alpha", Created = new DateTime(2024, 1, 1) },
            new() { Id = 2, Name = "Beta", Created = new DateTime(2024, 2, 2) }
        };

        using var reader = new ListDataReader<TestItem>(items);

        // Act & Assert
        int count = 0;
        while (reader.Read())
        {
            reader.GetInt32(reader.GetOrdinal("Id")).Should().Be(items[count].Id);
            reader.GetString(reader.GetOrdinal("Name")).Should().Be(items[count].Name);
            reader.GetDateTime(reader.GetOrdinal("Created")).Should().Be(items[count].Created);
            count++;
        }

        count.Should().Be(items.Count);
        reader.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void Read_ShouldIterateOverAllItems_IgnoringCreated()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Alpha", Created = new DateTime(2024, 1, 1) },
            new() { Id = 2, Name = "Beta", Created = new DateTime(2024, 2, 2) }
        };

        using var reader = new ListDataReader<TestItem>(items, ["Created"]);

        // Act & Assert
        int count = 0;
        while (reader.Read())
        {
            reader.GetInt32(reader.GetOrdinal("Id")).Should().Be(items[count].Id);
            reader.GetString(reader.GetOrdinal("Name")).Should().Be(items[count].Name);

            // "Created" should not be present
            reader.GetOrdinal("Created").Should().Be(-1);

            count++;
        }

        count.Should().Be(items.Count);
        reader.FieldCount.Should().Be(2);
        reader.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void GetSchemaTable_ShouldReturnCorrectColumns()
    {
        // Arrange
        var items = new List<TestItem>();
        using var reader = new ListDataReader<TestItem>(items);

        // Act
        var schema = reader.GetSchemaTable();

        // Assert
        schema.Rows.Count.Should().Be(3);
        schema.Rows[0]["ColumnName"].Should().Be("Id");
        schema.Rows[1]["ColumnName"].Should().Be("Name");
        schema.Rows[2]["ColumnName"].Should().Be("Created");
    }

    [Fact]
    public void GetValue_ByNameAndIndex_ShouldReturnSameResult()
    {
        // Arrange
        var item = new TestItem { Id = 42, Name = "Test", Created = DateTime.UtcNow };
        using var reader = new ListDataReader<TestItem>([item]);

        // Act
        reader.Read();
        var idByIndex = reader.GetValue(0);
        var idByName = reader["Id"];

        // Assert
        idByIndex.Should().Be(idByName);
        idByIndex.Should().Be(item.Id);
    }

    [Fact]
    public void IsDBNull_ShouldReturnTrueForNullValues()
    {
        // Arrange
        var item = new TestItem { Id = 1, Name = null, Created = DateTime.UtcNow };
        using var reader = new ListDataReader<TestItem>([item]);

        // Act
        reader.Read();
        var isNull = reader.IsDBNull(reader.GetOrdinal("Name"));

        // Assert
        isNull.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenListIsNull()
    {
        // Act
        Action act = () => new ListDataReader<TestItem>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetOrdinal_ShouldReturnMinusOneForUnknownColumn()
    {
        // Arrange
        var item = new TestItem { Id = 1, Name = "Test", Created = DateTime.UtcNow };
        using var reader = new ListDataReader<TestItem>([item]);

        // Act
        int ordinal = reader.GetOrdinal("UnknownColumn");

        // Assert
        ordinal.Should().Be(-1);
    }

    [Fact]
    public void Close_ShouldDisposeReaderAndSetIsClosed()
    {
        // Arrange
        var item = new TestItem { Id = 1, Name = "Test", Created = DateTime.UtcNow };
        var reader = new ListDataReader<TestItem>([item]);

        // Act
        reader.Close();

        // Assert
        reader.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void NextResult_ShouldAlwaysReturnFalse()
    {
        // Arrange
        var item = new TestItem { Id = 1, Name = "Test", Created = DateTime.UtcNow };
        using var reader = new ListDataReader<TestItem>([item]);

        // Act
        var result = reader.NextResult();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FieldCount_ShouldReturnNumberOfProperties()
    {
        // Arrange
        var item = new TestItem { Id = 1, Name = "Test", Created = DateTime.UtcNow };
        using var reader = new ListDataReader<TestItem>([item]);

        // Act
        int fieldCount = reader.FieldCount;

        // Assert
        fieldCount.Should().Be(3);
    }

    [Fact]
    public void Indexer_ByString_ShouldReturnCorrectValue()
    {
        // Arrange
        var item = new TestItem { Id = 99, Name = "IndexTest", Created = DateTime.UtcNow };
        using var reader = new ListDataReader<TestItem>([item]);

        // Act
        reader.Read();
        var value = reader["Name"];

        // Assert
        value.Should().Be(item.Name);
    }

    [Fact]
    public void GetData_ShouldThrowNotImplementedException()
    {
        // Arrange
        var item = new TestItem { Id = 1, Name = "Test", Created = DateTime.UtcNow };
        using var reader = new ListDataReader<TestItem>([item]);

        // Act
        Action act = () => reader.GetData(0);

        // Assert
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void IgnoreNames_ShouldExcludeSpecifiedProperties()
    {
        // Arrange
        var item = new TestItem { Id = 1, Name = "Test", Created = DateTime.UtcNow };
        using var reader = new ListDataReader<TestItem>([item], ["Name"]);

        // Act
        var schema = reader.GetSchemaTable();

        // Assert
        schema.Rows.Count.Should().Be(2);
        schema.Rows[0]["ColumnName"].Should().Be("Id");
        schema.Rows[1]["ColumnName"].Should().Be("Created");
    }

    [Fact]
    public void GetValues_ShouldReturnValues_ExcludingIgnoredCreated()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Alpha", Created = new DateTime(2024, 1, 1) },
            new() { Id = 2, Name = "Beta", Created = new DateTime(2024, 2, 2) }
        };

        using var reader = new ListDataReader<TestItem>(items, ["Created"]);

        int row = 0;
        while (reader.Read())
        {
            // Prepare a buffer larger than needed to ensure only active columns are filled
            object[] values = new object[5];
            int count = reader.GetValues(values);

            // Only Id and Name should be present
            count.Should().Be(2);
            values[0].Should().Be(items[row].Id);
            values[1].Should().Be(items[row].Name);
            // The rest should remain null
            values[2].Should().BeNull();
            values[3].Should().BeNull();
            values[4].Should().BeNull();

            row++;
        }
        row.Should().Be(items.Count);
    }
}
