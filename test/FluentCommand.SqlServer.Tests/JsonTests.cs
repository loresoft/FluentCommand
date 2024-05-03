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
    public JsonTests(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
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

        blobContainer.CreateIfNotExists();

        var exportFile = $"{nameof(QueryJsonSteamAzurite)}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.json";
        var exportClient = blobContainer.GetBlobClient(exportFile);

        var options = new BlobOpenWriteOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = MediaTypeNames.Application.Json
            }
        };

        using var exportStream = exportClient.OpenWrite(true, options);

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
            .QueryJsonAsync();

        json.Should().NotBeNull();
    }

    [Fact]
    public async Task QueryJsonSteamAzuriteAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var blobContainer = Services.GetRequiredService<BlobContainerClient>();
        blobContainer.Should().NotBeNull();

        await blobContainer.CreateIfNotExistsAsync();

        var exportFile = $"{nameof(QueryJsonSteamAzuriteAsync)}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.json";
        var exportClient = blobContainer.GetBlobClient(exportFile);

        var options = new BlobOpenWriteOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = MediaTypeNames.Application.Json
            }
        };

        await using var exportStream = await exportClient.OpenWriteAsync(true, options);

        string sql = "select TOP 1000 * from [User]";

        await session.Sql(sql)
            .QueryJsonAsync(exportStream);

        await exportStream.FlushAsync();
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
            .QueryJsonAsync();

        json.Should().NotBeNull();
    }
}
