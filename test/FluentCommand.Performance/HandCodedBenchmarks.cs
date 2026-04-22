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
    private SqlCommand _postCommand = null!;
    private SqlParameter _idParam = null!;

    [GlobalSetup]
    public void Setup()
    {
        BaseSetup();
        _postCommand = new SqlCommand("select Top 1 * from Post where Id = @Id", Connection);

        _idParam = _postCommand.Parameters.Add("@Id", SqlDbType.Int);

        _postCommand.Prepare();
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
