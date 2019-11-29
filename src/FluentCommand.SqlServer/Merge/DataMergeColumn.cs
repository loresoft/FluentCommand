using System;

namespace FluentCommand.Merge
{
    /// <summary>
    /// Class representing data merge column mapping.
    /// </summary>
    public class DataMergeColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataMergeColumn"/> class.
        /// </summary>
        public DataMergeColumn()
        {
            CanInsert = true;
            CanUpdate = true;
            CanBulkCopy = true;
        }

        /// <summary>
        /// Gets or sets the source column name used to read from source data. The column name is also used in the temporary table.
        /// </summary>
        /// <value>
        /// The source column name.
        /// </value>
        public string SourceColumn { get; set; }

        /// <summary>
        /// Gets or sets the target column name.
        /// </summary>
        /// <value>
        /// The target column name.
        /// </value>
        public string TargetColumn { get; set; }

        /// <summary>
        /// Gets or sets the SQL Server native type for the <see cref="SourceColumn"/>.
        /// </summary>
        /// <value>
        /// The  SQL Server native type for the <see cref="SourceColumn"/>.
        /// </value>
        public string NativeType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is include in the bulk copy operation. Default value is <c>true</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if the column is included in bulk copy; otherwise, <c>false</c>.
        /// </value>
        public bool CanBulkCopy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column can be inserted. Default value is <c>true</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if the column can be inserted; otherwise, <c>false</c>.
        /// </value>
        public bool CanInsert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column can be updated. Default value is <c>true</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if the column can be updated; otherwise, <c>false</c>.
        /// </value>
        public bool CanUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is used as part of the key to merge on.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the column is part of the key; otherwise, <c>false</c>.
        /// </value>
        public bool IsKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is ignored, not used by merge.  
        /// </summary>
        /// <value>
        /// <c>true</c> if the column is ignored; otherwise, <c>false</c>.
        /// </value>
        public bool IsIgnored { get; set; }
    }
}