namespace FluentCommand.Entities.Experiments;

using global::FluentCommand.Extensions;

// Counter: 72

/// <summary>
/// Extension methods for FluentCommand
/// </summary>
public static partial class AuditDataReaderFactoryExtensions
{
    /// <summary>
    /// Executes the command against the connection and converts the results to <see cref="T:FluentCommand.Entities.Audit"/> objects.
    /// </summary>
    /// <param name="dataQuery">The <see cref="T:FluentCommand.IDataQuery"/> for this extension method.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:FluentCommand.Entities.Audit"/> objects.
    /// </returns>
    public static global::System.Collections.Generic.IEnumerable<FluentCommand.Entities.Audit> QueryAudit(
        this global::FluentCommand.IDataQuery dataQuery)
        => dataQuery.Query<FluentCommand.Entities.Audit>(
            factory: AuditDataReaderFactoryExtensions.AuditFactory,
            commandBehavior: global::System.Data.CommandBehavior.SequentialAccess |
                             global::System.Data.CommandBehavior.SingleResult);

    /// <summary>
    /// Executes the query and returns the first row in the result as a <see cref="T:FluentCommand.Entities.Audit"/> object.
    /// </summary>
    /// <param name="dataQuery">The <see cref="T:FluentCommand.IDataQuery"/> for this extension method.</param>
    /// <returns>
    /// A instance of <see cref="T:FluentCommand.Entities.Audit"/>  if row exists; otherwise null.
    /// </returns>
    public static FluentCommand.Entities.Audit QuerySingleAudit(
        this global::FluentCommand.IDataQuery dataQuery)
        => dataQuery.QuerySingle<FluentCommand.Entities.Audit>(
            factory: AuditDataReaderFactoryExtensions.AuditFactory,
            commandBehavior: global::System.Data.CommandBehavior.SequentialAccess |
                             global::System.Data.CommandBehavior.SingleResult |
                             global::System.Data.CommandBehavior.SingleRow);

    /// <summary>
    /// Executes the command against the connection and converts the results to <see cref="T:FluentCommand.Entities.Audit"/> objects.
    /// </summary>
    /// <param name="dataQuery">The <see cref="T:FluentCommand.IDataQueryAsync"/> for this extension method.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:FluentCommand.Entities.Audit"/> objects.
    /// </returns>
    public static
        global::System.Threading.Tasks.Task<
            global::System.Collections.Generic.IEnumerable<FluentCommand.Entities.Audit>> QueryAuditAsync(
            this global::FluentCommand.IDataQueryAsync dataQuery,
            global::System.Threading.CancellationToken cancellationToken = default)
        => dataQuery.QueryAsync<FluentCommand.Entities.Audit>(
            factory: AuditDataReaderFactoryExtensions.AuditFactory,
            commandBehavior: global::System.Data.CommandBehavior.SequentialAccess |
                             global::System.Data.CommandBehavior.SingleResult,
            cancellationToken: cancellationToken);

    /// <summary>
    /// Executes the query and returns the first row in the result as a <see cref="T:FluentCommand.Entities.Audit"/> object.
    /// </summary>
    /// <param name="dataQuery">The <see cref="T:FluentCommand.IDataQueryAsync"/> for this extension method.</param>
    /// <returns>
    /// A instance of <see cref="T:FluentCommand.Entities.Audit"/>  if row exists; otherwise null.
    /// </returns>
    public static global::System.Threading.Tasks.Task<FluentCommand.Entities.Audit> QuerySingleAuditAsync(
        this global::FluentCommand.IDataQueryAsync dataQuery,
        global::System.Threading.CancellationToken cancellationToken = default)
        => dataQuery.QuerySingleAsync<FluentCommand.Entities.Audit>(
            factory: AuditDataReaderFactoryExtensions.AuditFactory,
            commandBehavior: global::System.Data.CommandBehavior.SequentialAccess |
                             global::System.Data.CommandBehavior.SingleResult |
                             global::System.Data.CommandBehavior.SingleRow,
            cancellationToken: cancellationToken);

    /// <summary>
    /// A factory for creating <see cref="T:FluentCommand.Entities.Audit"/> objects from the current row in the specified <paramref name="dataRecord"/>.
    /// </summary>
    /// <param name="dataRecord">The open <see cref="T:System.Data.IDataReader"/> to get the object from.</param>
    /// <returns>
    /// A instance of <see cref="FluentCommand.Entities.Audit"/>  having property names set that match the field names in the <paramref name="dataRecord"/>.
    /// </returns>
    public static FluentCommand.Entities.Audit AuditFactory(this global::System.Data.IDataReader dataRecord)
    {
        if (dataRecord == null)
            throw new global::System.ArgumentNullException(nameof(dataRecord));

        int v_id = default;
        System.DateTime v_date = default;
        int? v_userId = default;
        int? v_taskId = default;
        string v_content = default;
        string v_username = default;
        System.DateTimeOffset v_created = default;
        string v_createdBy = default;
        System.DateTimeOffset v_updated = default;
        string v_updatedBy = default;
        FluentCommand.ConcurrencyToken v_rowVersion = default;

        for (var __index = 0; __index < dataRecord.FieldCount; __index++)
        {
            if (dataRecord.IsDBNull(__index))
                continue;

            var __name = dataRecord.GetName(__index);
            switch (__name)
            {
                case nameof(FluentCommand.Entities.Audit.Id):
                    v_id = dataRecord.GetInt32(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.Date):
                    v_date = dataRecord.GetDateTime(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.UserId):
                    v_userId = dataRecord.GetInt32(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.TaskId):
                    v_taskId = dataRecord.GetInt32(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.Content):
                    v_content = dataRecord.GetString(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.Username):
                    v_username = dataRecord.GetString(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.Created):
                    v_created = dataRecord.GetDateTimeOffset(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.CreatedBy):
                    v_createdBy = dataRecord.GetString(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.Updated):
                    v_updated = dataRecord.GetDateTimeOffset(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.UpdatedBy):
                    v_updatedBy = dataRecord.GetString(__index);
                    break;
                case nameof(FluentCommand.Entities.Audit.RowVersion):
                    var c_rowVersion = global::FluentCommand.Internal
                            .Singleton<FluentCommand.Handlers.ConcurrencyTokenHandler>.Current
                        as global::FluentCommand.IDataFieldConverter<FluentCommand.ConcurrencyToken>;

                    v_rowVersion = c_rowVersion.ReadValue(dataRecord, __index);
                    break;
            }
        }

        return new FluentCommand.Entities.Audit
        {
            Id = v_id,
            Date = v_date,
            UserId = v_userId,
            TaskId = v_taskId,
            Content = v_content,
            Username = v_username,
            Created = v_created,
            CreatedBy = v_createdBy,
            Updated = v_updated,
            UpdatedBy = v_updatedBy,
            RowVersion = v_rowVersion
        };
    }
}
