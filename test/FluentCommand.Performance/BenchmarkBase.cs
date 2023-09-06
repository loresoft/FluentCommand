using BenchmarkDotNet.Attributes;

using Microsoft.Data.SqlClient;


namespace FluentCommand.Performance;

[BenchmarkCategory("ORM")]
public abstract class BenchmarkBase
{
    private SqlConnection _connection;
    protected SqlConnection Connection => _connection;

    public static string ConnectionString { get; } = "Data Source=(local);Initial Catalog=tempdb;Integrated Security=True;TrustServerCertificate=True";

    private int _index;
    protected int Index => _index;

    protected void BaseSetup()
    {
        _index = 0;

        _connection = new SqlConnection(ConnectionString);
        _connection.Open();
    }

    protected int NextIndex()
    {
        _index++;
        if (_index >= 5000)
            _index = 1;

        return _index;
    }
}
