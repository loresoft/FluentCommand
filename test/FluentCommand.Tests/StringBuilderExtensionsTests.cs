using System.Text;

using FluentCommand.Extensions;

namespace FluentCommand.Tests;

public class StringBuilderExtensionsTests
{
    [Fact]
    public void AppendTruncatedWithLongTextShouldAppendEllipsis()
    {
        // Arrange
        var builder = new StringBuilder();

        // Act
        builder.AppendTruncated("abcdef", 5);

        // Assert
        builder.ToString().Should().Be("ab...");
    }

    [Fact]
    public void AppendTruncatedWithCustomEllipsisShouldAppendCustomEllipsis()
    {
        // Arrange
        var builder = new StringBuilder();

        // Act
        builder.AppendTruncated("abcdef", 5, "~");

        // Assert
        builder.ToString().Should().Be("abcd~");
    }

    [Fact]
    public void AppendTruncatedWithEmptyEllipsisShouldAppendMaximumLength()
    {
        // Arrange
        var builder = new StringBuilder();

        // Act
        builder.AppendTruncated("abcdef", 5, string.Empty);

        // Assert
        builder.ToString().Should().Be("abcde");
    }

    [Fact]
    public void AppendTruncatedWithShortMaxLengthShouldTruncateEllipsis()
    {
        // Arrange
        var builder = new StringBuilder();

        // Act
        builder.AppendTruncated("abcdef", 2);

        // Assert
        builder.ToString().Should().Be("..");
    }

    [Fact]
    public void AppendTruncatedWithObjectShouldAppendEllipsis()
    {
        // Arrange
        var builder = new StringBuilder();

        // Act
        builder.AppendTruncated(123456, 4);

        // Assert
        builder.ToString().Should().Be("1...");
    }
}
