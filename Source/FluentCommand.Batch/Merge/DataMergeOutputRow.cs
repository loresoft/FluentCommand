using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentCommand.Merge
{
    /// <summary>
    /// A data merge output row.
    /// </summary>
    public class DataMergeOutputRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataMergeOutputRow"/> class.
        /// </summary>
        public DataMergeOutputRow()
        {
            Columns = new List<DataMergeOutputColumn>();
        }

        /// <summary>
        /// Gets or sets the merge action.
        /// </summary>
        /// <value>
        /// The merge action for this row.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the columns that changed.
        /// </summary>
        /// <value>
        /// The list of columns.
        /// </value>
        public List<DataMergeOutputColumn> Columns { get; set; }

        /// <summary>
        /// Gets the <see cref="DataMergeOutputColumn"/> with the specified column name.
        /// </summary>
        /// <value>
        /// The <see cref="DataMergeOutputColumn"/>.
        /// </value>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public DataMergeOutputColumn this[string columnName]
        {
            get
            {
                return Columns.FirstOrDefault(c => c.Name == columnName);
            }
        }
    }
}