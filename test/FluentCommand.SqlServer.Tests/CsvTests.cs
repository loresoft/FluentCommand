using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using FluentCommand.Entities;

using Microsoft.Extensions.DependencyInjection;

using Task = System.Threading.Tasks.Task;

namespace FluentCommand.SqlServer.Tests;

public class CsvTests : DatabaseTestBase
{
    public CsvTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {

    }

    [Fact]
    public void QueryCsv()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string sql = "select TOP 1000 * from [User]";

        var data = session.Sql(sql)
            .QueryCsv();

        data.Should().NotBeNull();
    }

    [Fact]
    public void QueryCsvSteamAzurite()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var blobContainer = Services.GetRequiredService<BlobContainerClient>();
        blobContainer.Should().NotBeNull();

        blobContainer.CreateIfNotExists(cancellationToken: TestCancellation);

        var exportFile = $"{nameof(QueryCsvSteamAzurite)}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";
        var exportClient = blobContainer.GetBlobClient(exportFile);

        var options = new BlobOpenWriteOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "text/csv"
            }
        };

        using var exportStream = exportClient.OpenWrite(true, options, TestCancellation);

        string sql = "select TOP 1000 * from [User]";

        session.Sql(sql).QueryCsv(exportStream);

        exportStream.Flush();
    }

    [Fact]
    public async Task QueryCsvAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string sql = "select TOP 1000 * from [User]";

        var data = await session.Sql(sql)
            .QueryCsvAsync(cancellationToken: TestCancellation);

        data.Should().NotBeNull();
    }

    [Fact]
    public async Task QueryCsvSteamAzuriteAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var blobContainer = Services.GetRequiredService<BlobContainerClient>();
        blobContainer.Should().NotBeNull();

        await blobContainer.CreateIfNotExistsAsync(cancellationToken: TestCancellation);

        var exportFile = $"{nameof(QueryCsvSteamAzuriteAsync)}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";
        var exportClient = blobContainer.GetBlobClient(exportFile);

        var options = new BlobOpenWriteOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "text/csv"
            }
        };

        await using var exportStream = await exportClient.OpenWriteAsync(true, options, TestCancellation);

        string sql = "select TOP 1000 * from [User]";

        await session.Sql(sql)
            .QueryCsvAsync(exportStream, cancellationToken: TestCancellation);

        await exportStream.FlushAsync(TestCancellation);
    }

    [Fact]
    public async Task QuerySelectAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var data = await session
            .Sql(builder => builder
                .Select<Status>()
                .OrderBy(p => p.DisplayOrder)
                .Limit(0, 1000)
            )
            .QueryCsvAsync(cancellationToken: TestCancellation);

        data.Should().NotBeNull();
    }
}
