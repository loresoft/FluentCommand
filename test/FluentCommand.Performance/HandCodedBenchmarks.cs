using System;
using System.ComponentModel;
using System.Data;

using BenchmarkDotNet.Attributes;

using FluentCommand.Extensions;

using Microsoft.Data.SqlClient;

namespace FluentCommand.Performance;

[Description("Hand Coded")]
public class HandCodedBenchmarks : BenchmarkBase
{
    private SqlCommand _postCommand;
    private SqlParameter _idParam;
    private DataTable _table;

    [GlobalSetup]
    public void Setup()
    {
        BaseSetup();
        _postCommand = new SqlCommand("select Top 1 * from Posts where Id = @Id", Connection);

        _idParam = _postCommand.Parameters.Add("@Id", SqlDbType.Int);

        _postCommand.Prepare();

        _table = new DataTable
        {
            Columns =
                    {
                        {"Id", typeof (int)},
                        {"Text", typeof (string)},
                        {"CreationDate", typeof (DateTime)},
                        {"LastChangeDate", typeof (DateTime)},
                        {"Counter1", typeof (int)},
                        {"Counter2", typeof (int)},
                        {"Counter3", typeof (int)},
                        {"Counter4", typeof (int)},
                        {"Counter5", typeof (int)},
                        {"Counter6", typeof (int)},
                        {"Counter7", typeof (int)},
                        {"Counter8", typeof (int)},
                        {"Counter9", typeof (int)},
                    }
        };
    }

    [Benchmark(Description = "SqlCommand")]
    public Post SqlCommand()
    {
        _idParam.Value = NextIndex();

        using var reader = _postCommand.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow);

        reader.Read();

        return new Post
        {
            Id = reader.GetInt32(nameof(Post.Id)),
            Text = reader.GetStringNull(nameof(Post.Text)),
            CreationDate = reader.GetDateTime(nameof(Post.CreationDate)),
            LastChangeDate = reader.GetDateTime(nameof(Post.LastChangeDate)),

            Counter1 = reader.GetInt32Null(nameof(Post.Counter1)),
            Counter2 = reader.GetInt32Null(nameof(Post.Counter2)),
            Counter3 = reader.GetInt32Null(nameof(Post.Counter3)),
            Counter4 = reader.GetInt32Null(nameof(Post.Counter4)),
            Counter5 = reader.GetInt32Null(nameof(Post.Counter5)),
            Counter6 = reader.GetInt32Null(nameof(Post.Counter6)),
            Counter7 = reader.GetInt32Null(nameof(Post.Counter7)),
            Counter8 = reader.GetInt32Null(nameof(Post.Counter8)),
            Counter9 = reader.GetInt32Null(nameof(Post.Counter9))
        };
    }
}
