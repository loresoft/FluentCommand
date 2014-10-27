using System;
using FluentAssertions;
using FluentCommand.Merge;
using Xunit;

namespace FluentCommand.Batch.Tests
{
    
    public class DataMergeGeneratorTests
    {
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
    }
}
