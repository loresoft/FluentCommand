using System.Text.Json;

using FluentCommand.Query;

namespace FluentCommand.Tests.Query;

public class QueryJsonTests
{
    [Fact]
    public void QueryRequestRoundTripPreservesFilterValueDataTypes()
    {
        var request = new QueryRequest
        {
            Filter = new QueryFilter
            {
                Logic = LogicalOperators.And,
                Filters = new List<QueryFilter>
                {
                    new()
                    {
                        Name = "Name",
                        Value = "Test",
                        Operator = FilterOperators.Contains,
                    },
                    new()
                    {
                        Name = "IsActive",
                        Value = true,
                    },
                    new()
                    {
                        Name = "DisplayOrder",
                        Value = 42,
                        Operator = FilterOperators.GreaterThanOrEqual,
                    },
                    new()
                    {
                        Name = "Id",
                        Value = new[] { 1, 2 },
                        Operator = FilterOperators.In,
                    },
                },
            },
            Sort = new List<QuerySort>
            {
                new()
                {
                    Name = "DisplayOrder",
                    Direction = SortDirections.Descending,
                },
            },
            Page = 2,
            PageSize = 10,
            ContinuationToken = "next-page",
        };

        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<QueryRequest>(json);

        result.Should().NotBeNull();

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);

        result.ContinuationToken.Should().Be("next-page");

        result.Sort.Should().NotBeNull();
        result.Sort.Should().ContainSingle();
        result.Sort[0].Name.Should().Be("DisplayOrder");
        result.Sort[0].Direction.Should().Be(SortDirections.Descending);

        result.Filter.Should().NotBeNull();
        result.Filter.Logic.Should().Be(LogicalOperators.And);

        result.Filter.Filters.Should().NotBeNull();
        result.Filter.Filters.Should().HaveCount(4);
        result.Filter.Filters[0].Value.Should().BeOfType<string>().Which.Should().Be("Test");
        result.Filter.Filters[1].Value.Should().BeOfType<bool>().Which.Should().BeTrue();
        result.Filter.Filters[2].Value.Should().BeOfType<int>().Which.Should().Be(42);
        result.Filter.Filters[3].Value.Should().BeAssignableTo<IEnumerable<object?>>().Which.Should().Equal(1, 2);
    }

    [Fact]
    public void QueryRequestRoundTripPreservesLongAndDecimalFilterValueDataTypes()
    {
        var request = new QueryRequest
        {
            Filter = new QueryFilter
            {
                Logic = LogicalOperators.And,
                Filters = new List<QueryFilter>
                {
                    new()
                    {
                        Name = "BigId",
                        Value = 3000000000L,
                    },
                    new()
                    {
                        Name = "Amount",
                        Value = 123.45m,
                    },
                },
            },
        };

        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<QueryRequest>(json);

        result.Should().NotBeNull();

        result!.Filter!.Filters.Should().HaveCount(2);
        result.Filter.Filters![0].Value.Should().BeOfType<long>().Which.Should().Be(3000000000L);
        result.Filter.Filters[1].Value.Should().BeOfType<decimal>().Which.Should().Be(123.45m);
    }

    [Fact]
    public void QueryRequestRoundTripPreservesObjectFilterValueAsJsonElement()
    {
        var request = new QueryRequest
        {
            Filter = new QueryFilter
            {
                Name = "Data",
                Value = new QueryResultItem(1, "First"),
            },
        };

        var json = JsonSerializer.Serialize(request);
        var result = JsonSerializer.Deserialize<QueryRequest>(json);

        result.Should().NotBeNull();

        var value = result!.Filter!.Value.Should().BeOfType<JsonElement>().Subject;

        value.ValueKind.Should().Be(JsonValueKind.Object);

        value.GetProperty("Id").GetInt32().Should().Be(1);
        value.GetProperty("Name").GetString().Should().Be("First");
    }

    [Fact]
    public void QueryResultRoundTripPreservesDataAndPagingMetadata()
    {
        var result = new QueryResult<QueryResultItem>
        {
            Data = new List<QueryResultItem>
            {
                new(1, "First"),
                new(2, "Second"),
            },
            Total = 12,
            ContinuationToken = "next-page",
        };

        var json = JsonSerializer.Serialize(result);
        var roundTrip = JsonSerializer.Deserialize<QueryResult<QueryResultItem>>(json);

        json.Should().Contain("\"data\"");
        json.Should().Contain("\"total\":12");
        json.Should().Contain("\"continuationToken\":\"next-page\"");

        roundTrip.Should().NotBeNull();
        roundTrip!.Total.Should().Be(12);
        roundTrip.ContinuationToken.Should().Be("next-page");
        roundTrip.Data.Should().Equal(new QueryResultItem(1, "First"), new QueryResultItem(2, "Second"));
    }

    private sealed record QueryResultItem(int Id, string Name);
}
