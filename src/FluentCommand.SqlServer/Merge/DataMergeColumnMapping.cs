using System;

namespace FluentCommand.Merge
{
    /// <summary>
    /// Fluent class for building data merge column mapping
    /// </summary>
    public class DataMergeColumnMapping : IDataColumnMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataMergeColumnMapping"/> class.
        /// </summary>
        /// <param name="mergeColumn">The merge column definition to be updated.</param>
        public DataMergeColumnMapping(DataMergeColumn mergeColumn)
        {
            MergeColumn = mergeColumn;
        }

        /// <summary>
        /// Gets the current <see cref="DataMergeColumn"/> being updated.
        /// </summary>
        /// <value>
        /// The data merge column being updated.
        /// </value>
        public DataMergeColumn MergeColumn { get; }

        /// <summary>
        /// Sets the source column name used to read from source data.
        /// </summary>
        /// <param name="value">The source column name.</param>
        /// <returns></returns>
        public IDataColumnMapping SourceColumn(string value)
        {
            MergeColumn.SourceColumn = value;
            return this;
        }

        /// <summary>
        /// Sets the target column name.
        /// </summary>
        /// <param name="value">The target column name.</param>
        /// <returns></returns>
        public IDataColumnMapping TargetColumn(string value)
        {
            MergeColumn.TargetColumn = value;
            return this;
        }

        /// <summary>
        /// Sets the SQL Server native type for the <see cref="SourceColumn"/>.
        /// </summary>
        /// <param name="value">The  SQL Server native type.</param>
        /// <returns></returns>
        public IDataColumnMapping NativeType(string value)
        {
            MergeColumn.NativeType = value;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether the column is include in the bulk copy operation.
        /// </summary>
        /// <param name="value"><c>true</c> if column is included in bulk copy; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> for a data column mapping.
        /// </returns>
        public IDataColumnMapping BulkCopy(bool value = true)
        {
            MergeColumn.CanBulkCopy = value;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether the column can be inserted.
        /// </summary>
        /// <param name="value"><c>true</c> if the column can be inserted; otherwise, <c>false</c>.</param>
        /// <returns></returns>
        public IDataColumnMapping Insert(bool value = true)
        {
            MergeColumn.CanInsert = value;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether the column can be updated.
        /// </summary>
        /// <param name="value"><c>true</c> if the column can be updated; otherwise, <c>false</c>.</param>
        /// <returns></returns>
        public IDataColumnMapping Update(bool value = true)
        {
            MergeColumn.CanUpdate = value;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether the column is used as part of the key to merge on.
        /// </summary>
        /// <param name="value"><c>true</c> if the column is part of the key; otherwise, <c>false</c>.</param>
        /// <returns></returns>
        public IDataColumnMapping Key(bool value = true)
        {
            MergeColumn.IsKey = value;

            // only change if true
            if (value)
                MergeColumn.CanUpdate = false;

            return this;
        }

        /// <summary>
        /// sets a value indicating whether the column is ignored, not used in anyway.
        /// </summary>
        /// <param name="value"><c>true</c> if the column is ignored; otherwise, <c>false</c>.</param>
        /// <returns></returns>
        public IDataColumnMapping Ignore(bool value = true)
        {
            MergeColumn.IsIgnored = value;
            return this;
        }
    }
}