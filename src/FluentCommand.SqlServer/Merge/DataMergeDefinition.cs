using FluentCommand.Extensions;
using FluentCommand.Reflection;

namespace FluentCommand.Merge;

/// <summary>
/// Represents the definition and configuration for a data merge operation, including target table, columns, and merge behavior.
/// </summary>
public class DataMergeDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMergeDefinition"/> class with default settings.
    /// </summary>
    /// <remarks>
    /// By default, <see cref="Columns"/> is initialized as an empty list, <see cref="TemporaryTable"/> is generated with a unique name,
    /// <see cref="IncludeInsert"/> and <see cref="IncludeUpdate"/> are set to <c>true</c>, and <see cref="Mode"/> is set to <see cref="DataMergeMode.Auto"/>.
    /// </remarks>
    public DataMergeDefinition()
    {
        Columns = new List<DataMergeColumn>();
        TemporaryTable = "#Merge" + DateTime.Now.Ticks;
        IncludeInsert = true;
        IncludeUpdate = true;
        Mode = DataMergeMode.Auto;
    }

    /// <summary>
    /// Gets or sets the name of the target table into which data will be merged.
    /// </summary>
    /// <value>
    /// The name of the target table.
    /// </value>
    public string TargetTable { get; set; }

    /// <summary>
    /// Gets or sets the name of the temporary table used for bulk inserting data before merging.
    /// If not set, a unique name is generated.
    /// </summary>
    /// <value>
    /// The name of the temporary table for bulk insert operations.
    /// </value>
    public string TemporaryTable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to insert data not found in the <see cref="TargetTable"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> to insert new data; otherwise, <c>false</c>. Default is <c>true</c>.
    /// </value>
    public bool IncludeInsert { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to update data found in the <see cref="TargetTable"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> to update existing data; otherwise, <c>false</c>. Default is <c>true</c>.
    /// </value>
    public bool IncludeUpdate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to delete data from the <see cref="TargetTable"/> that is not found in the <see cref="TemporaryTable"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> to delete target data not present in the source data; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeDelete { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to output the inserted, updated, or deleted values from the <see cref="TargetTable"/> after the merge operation.
    /// </summary>
    /// <value>
    /// <c>true</c> to output the affected data; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeOutput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow identity insert on the <see cref="TargetTable"/> during the merge operation.
    /// </summary>
    /// <value>
    /// <c>true</c> to allow identity insert; otherwise, <c>false</c>.
    /// </value>
    public bool IdentityInsert { get; set; }

    /// <summary>
    /// Gets or sets the collection of column mappings used in the merge operation.
    /// </summary>
    /// <value>
    /// A list of <see cref="DataMergeColumn"/> instances representing the mapped columns.
    /// </value>
    public List<DataMergeColumn> Columns { get; set; }

    /// <summary>
    /// Gets or sets the mode that determines how the merge operation will be processed.
    /// </summary>
    /// <value>
    /// The <see cref="DataMergeMode"/> specifying the merge processing mode.
    /// </value>
    public DataMergeMode Mode { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="DataMergeDefinition"/> with properties automatically mapped from the specified entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to auto-map.</typeparam>
    /// <returns>A new <see cref="DataMergeDefinition"/> instance with columns and table name mapped from <typeparamref name="TEntity"/>.</returns>
    public static DataMergeDefinition Create<TEntity>()
    {
        var mergeDefinition = new DataMergeDefinition();
        AutoMap<TEntity>(mergeDefinition);

        return mergeDefinition;
    }

    /// <summary>
    /// Automatically maps the properties of the specified entity type <typeparamref name="TEntity"/> to the given <see cref="DataMergeDefinition"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to map.</typeparam>
    /// <param name="mergeDefinition">The <see cref="DataMergeDefinition"/> to populate with mapped columns and table name.</param>
    /// <remarks>
    /// This method uses reflection to map entity properties to <see cref="DataMergeColumn"/> instances, setting key, insert, update, and ignore flags as appropriate.
    /// </remarks>
    public static void AutoMap<TEntity>(DataMergeDefinition mergeDefinition)
    {
        var typeAccessor = TypeAccessor.GetAccessor<TEntity>();

        // don't overwrite existing
        if (mergeDefinition.TargetTable.IsNullOrEmpty())
            mergeDefinition.TargetTable = typeAccessor.TableSchema.HasValue() ? $"{typeAccessor.TableSchema}.{typeAccessor.TableName}" : typeAccessor.TableName;

        foreach (var property in typeAccessor.GetProperties())
        {
            string sourceColumn = property.Column;
            string targetColumn = property.Column;
            string nativeType = property.ColumnType ?? SqlTypeMapping.NativeType(property.MemberType);

            // find existing map and update
            var mergeColumn = mergeDefinition.Columns.FirstOrAdd(
                m => m.SourceColumn == sourceColumn,
                () => new DataMergeColumn { SourceColumn = sourceColumn });

            mergeColumn.TargetColumn = targetColumn;
            mergeColumn.NativeType = nativeType;
            mergeColumn.IsKey = property.IsKey;
            mergeColumn.CanUpdate = !property.IsKey && !property.IsDatabaseGenerated && !property.IsConcurrencyCheck;
            mergeColumn.CanInsert = !property.IsDatabaseGenerated && !property.IsConcurrencyCheck;
            mergeColumn.IsIgnored = property.IsNotMapped || string.Equals(nativeType, "sql_variant", StringComparison.OrdinalIgnoreCase);
        }
    }

}
