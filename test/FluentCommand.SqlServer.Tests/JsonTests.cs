using System.Net.Mime;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using FluentCommand.Entities;
using FluentCommand.Query;

using Microsoft.Extensions.DependencyInjection;

using Task = System.Threading.Tasks.Task;

namespace FluentCommand.SqlServer.Tests;

public class JsonTests : DatabaseTestBase
{
    public JsonTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {

    }

    [Fact]
    public void QueryJson()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string sql = "select TOP 1000 * from [User]";

        var json = session.Sql(sql)
            .QueryJson();

        json.Should().NotBeNull();
    }

    [Fact]
    public void QueryJsonSteamAzurite()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var blobContainer = Services.GetRequiredService<BlobContainerClient>();
        blobContainer.Should().NotBeNull();

        blobContainer.CreateIfNotExists(cancellationToken: TestCancellation);

        var exportFile = $"{nameof(QueryJsonSteamAzurite)}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.json";
        var exportClient = blobContainer.GetBlobClient(exportFile);

        var options = new BlobOpenWriteOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = MediaTypeNames.Application.Json
            }
        };

        using var exportStream = exportClient.OpenWrite(true, options, TestCancellation);

        string sql = "select TOP 1000 * from [User]";

        session.Sql(sql)
            .QueryJson(exportStream);

        exportStream.Flush();
    }

    [Fact]
    public async Task QueryJsonAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string sql = "select TOP 1000 * from [User]";

        var json = await session.Sql(sql)
            .QueryJsonAsync(cancellationToken: TestCancellation);

        json.Should().NotBeNull();
    }

    [Fact]
    public async Task QueryJsonSteamAzuriteAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var blobContainer = Services.GetRequiredService<BlobContainerClient>();
        blobContainer.Should().NotBeNull();

        await blobContainer.CreateIfNotExistsAsync(cancellationToken: TestCancellation);

        var exportFile = $"{nameof(QueryJsonSteamAzuriteAsync)}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.json";
        var exportClient = blobContainer.GetBlobClient(exportFile);

        var options = new BlobOpenWriteOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = MediaTypeNames.Application.Json
            }
        };

        await using var exportStream = await exportClient.OpenWriteAsync(true, options, TestCancellation);

        string sql = "select TOP 1000 * from [User]";

        await session.Sql(sql)
            .QueryJsonAsync(exportStream, cancellationToken: TestCancellation);

        await exportStream.FlushAsync(TestCancellation);
    }

    [Fact]
    public async Task QuerySelectAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var json = await session
            .Sql(builder => builder
                .Select<Status>()
                .OrderBy(p => p.DisplayOrder)
                .Limit(0, 1000)
            )
            .QueryJsonAsync(cancellationToken: TestCancellation);

        json.Should().NotBeNull();
    }

    [Fact]
    public async Task QueryDataTypeSelectAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var item = new DataType()
        {
            Id = DateTime.Now.Ticks,
            Name = "Test1",
            Boolean = false,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
            DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            Guid = Guid.Empty,
            TimeSpan = TimeSpan.FromHours(1),
            DateOnly = new DateOnly(2022, 12, 1),
            TimeOnly = new TimeOnly(1, 30, 0),
            BooleanNull = false,
            ShortNull = 2,
            LongNull = 200,
            FloatNull = 200.20F,
            DoubleNull = 300.35,
            DecimalNull = 456.12M,
            DateTimeNull = new DateTime(2024, 4, 1, 8, 0, 0),
            DateTimeOffsetNull = new DateTimeOffset(2024, 4, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            GuidNull = Guid.Empty,
            TimeSpanNull = TimeSpan.FromHours(1),
            DateOnlyNull = new DateOnly(2022, 12, 1),
            TimeOnlyNull = new TimeOnly(1, 30, 0),
        };

        await session
            .Sql(builder => builder
                .Insert<DataType>()
                .Values(item)
            )
            .ExecuteAsync(cancellationToken: TestCancellation);

        var json = await session
            .Sql(builder => builder
                .Select<DataType>()
                .OrderBy(p => p.Id)
                .Limit(0, 1000)
            )
            .QueryJsonAsync(cancellationToken: TestCancellation);

        json.Should().NotBeNull();
    }
}
