using System;
using System.Linq;

using System.Linq.Expressions;
using FluentCommand.Extensions;
using FluentCommand.Reflection;

namespace FluentCommand.Merge
{
    /// <summary>
    /// Fluent class for building strongly typed data merge mapping
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity used in the mapping.</typeparam>
    public class DataMergeMapping<TEntity> : DataMergeMapping, IDataMergeMapping<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataMergeMapping{TEntity}"/> class.
        /// </summary>
        /// <param name="mergeDefinition">The data merge definition.</param>
        public DataMergeMapping(DataMergeDefinition mergeDefinition)
            : base(mergeDefinition)
        {
        }

        /// <summary>
        /// Automatically maps all properties in <typeparamref name="TEntity"/> as columns.
        /// </summary>
        /// <returns></returns>
        public IDataMergeMapping<TEntity> AutoMap()
        {
            DataMergeDefinition.AutoMap<TEntity>(MergeDefinition);

            return this;
        }

        /// <summary>
        /// Start column mapping for the specified source column name.
        /// </summary>
        /// <typeparam name="TValue">The property value type.</typeparam>
        /// <param name="sourceProperty">The source property.</param>
        /// <returns>
        /// a fluent <c>interface</c> for mapping a data merge column.
        /// </returns>
        public IDataColumnMapping Column<TValue>(Expression<Func<TEntity, TValue>> sourceProperty)
        {
            string sourceColumn = ReflectionHelper.ExtractPropertyName(sourceProperty);
            return Column(sourceColumn);
        }
    }

    /// <summary>
    /// Fluent class for building data merge mapping
    /// </summary>
    public class DataMergeMapping : IDataMergeMapping
    {
        private readonly DataMergeDefinition _mergeDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMergeMapping"/> class.
        /// </summary>
        /// <param name="mergeDefinition">The data merge definition.</param>
        public DataMergeMapping(DataMergeDefinition mergeDefinition)
        {
            _mergeDefinition = mergeDefinition;
        }

        /// <summary>
        /// Gets the current <see cref="DataMergeDefinition"/> being updated.
        /// </summary>
        /// <value>
        /// The current data merge definition.
        /// </value>
        public DataMergeDefinition MergeDefinition
        {
            get { return _mergeDefinition; }
        }

        /// <summary>
        /// Start column mapping for the specified source column name.
        /// </summary>
        /// <param name="sourceColumn">The source column name.</param>
        /// <returns>a fluent <c>interface</c> for mapping a data merge column.</returns>
        public IDataColumnMapping Column(string sourceColumn)
        {
            var mergeColumn = MergeDefinition.Columns.FirstOrAdd(
                c => c.SourceColumn == sourceColumn,
                () => new DataMergeColumn
                {
                    SourceColumn = sourceColumn,
                    TargetColumn = sourceColumn
                });

            var columnMapping = new DataMergeColumnMapping(mergeColumn);

            return columnMapping;
        }
    }
}