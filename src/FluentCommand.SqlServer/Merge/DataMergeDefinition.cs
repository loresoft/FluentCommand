using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FluentCommand.Extensions;

namespace FluentCommand.Merge
{
    /// <summary>
    /// Class representing a data merge definition.
    /// </summary>
    public class DataMergeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataMergeDefinition"/> class.
        /// </summary>
        public DataMergeDefinition()
        {
            Columns = new List<DataMergeColumn>();
            TemporaryTable = "#Merge" + DateTime.Now.Ticks;
            IncludeInsert = true;
            IncludeUpdate = true;
            Mode = DataMergeMode.Auto;
        }

        /// <summary>
        /// Gets or sets the name of target table.
        /// </summary>
        /// <value>
        /// The name of target table.
        /// </value>
        public string TargetTable { get; set; }

        /// <summary>
        /// Gets or sets the name of temporary table the data will be bulk inserted into. The value will be generated if empty.
        /// </summary>
        /// <value>
        /// The name of the temporary table the data will be bulk inserted into.
        /// </value>
        public string TemporaryTable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to insert data not found in <see cref="TargetTable"/>. Default value is <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> to insert data not found; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInsert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to update data found in <see cref="TargetTable"/>. Default value is <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> to update data found; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to delete data from <see cref="TargetTable"/> not found in <see cref="TemporaryTable"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> to delete target data not in source data; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeDelete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to output the inserted or deleted values from <see cref="TargetTable"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> to output updated data from the target table; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow identity insert on the <see cref="TargetTable"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> to allow identity insert; otherwise, <c>false</c>.
        /// </value>
        public bool IdentityInsert { get; set; }

        /// <summary>
        /// Gets or sets the collection of mapped columns.
        /// </summary>
        /// <value>
        /// The mapped columns collection.
        /// </value>
        public List<DataMergeColumn> Columns { get; set; }

        /// <summary>
        /// Gets or sets the mode for how the merge will be processed.
        /// </summary>
        /// <value>
        /// The mode for how the merge will be processed.
        /// </value>
        /// <seealso cref="DataMergeMode"/>
        public DataMergeMode Mode { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="DataMergeDefinition"/> with properties from type <typeparamref name="TEntity"/> auto mapped.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>A new instance of <see cref="DataMergeDefinition"/>.</returns>
        public static DataMergeDefinition Create<TEntity>()
        {
            var mergeDefinition = new DataMergeDefinition();
            AutoMap<TEntity>(mergeDefinition);

            return mergeDefinition;
        }

        /// <summary>
        /// Automatics the map the properties of type <typeparamref name="TEntity"/> to the specified <see cref="DataMergeDefinition"/> .
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="mergeDefinition">The merge definition up auto map to.</param>
        public static void AutoMap<TEntity>(DataMergeDefinition mergeDefinition)
        {
            var entityType = typeof(TEntity);
            var properties = TypeDescriptor.GetProperties(entityType);

            
            var tableAttribute = Attribute.GetCustomAttribute(entityType, typeof(TableAttribute)) as TableAttribute;
            if (tableAttribute != null)
            {
                string targetTable = tableAttribute.Name;
                if (!string.IsNullOrEmpty(tableAttribute.Schema))
                    targetTable = tableAttribute.Schema + "." + targetTable;

                mergeDefinition.TargetTable = targetTable;
            }

            if (string.IsNullOrEmpty(mergeDefinition.TargetTable))
                mergeDefinition.TargetTable = entityType.Name;

            foreach (PropertyDescriptor p in properties)
            {
                string sourceColumn = p.Name;
                string targetColumn = sourceColumn;
                string nativeType = null;

                var columnAttribute = p.Attributes
                    .OfType<ColumnAttribute>()
                    .FirstOrDefault();

                if (columnAttribute != null)
                {
                    if (columnAttribute.Name.HasValue())
                        targetColumn = columnAttribute.Name;
                    if (columnAttribute.TypeName.HasValue())
                        nativeType = columnAttribute.TypeName;
                }

                var mergeColumn = mergeDefinition.Columns.FirstOrAdd(
                    m => m.SourceColumn == sourceColumn,
                    () => new DataMergeColumn { SourceColumn = sourceColumn });

                mergeColumn.TargetColumn = targetColumn;
                mergeColumn.NativeType = nativeType;

                var keyAttribute = p.Attributes
                    .OfType<KeyAttribute>()
                    .FirstOrDefault();

                if (keyAttribute != null)
                {
                    mergeColumn.IsKey = true;
                    mergeColumn.CanUpdate = false;
                }

                var ignoreAttribute = p.Attributes
                    .OfType<NotMappedAttribute>()
                    .FirstOrDefault();

                if (ignoreAttribute != null)
                    mergeColumn.IsIgnored = true;
            }
        }

    }
}
