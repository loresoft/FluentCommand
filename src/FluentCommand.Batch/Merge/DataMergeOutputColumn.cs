using System;

namespace FluentCommand.Merge
{
    /// <summary>
    /// A data merge output column.
    /// </summary>
    public class DataMergeOutputColumn
    {
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the current value of the column.
        /// </summary>
        /// <value>
        /// The current value of the column.
        /// </value>
        public object Current { get; set; }
        
        /// <summary>
        /// Gets or sets the original value of the column.
        /// </summary>
        /// <value>
        /// The original value of the column.
        /// </value>
        public object Original { get; set; }

        /// <summary>
        /// Gets or sets the .
        /// </summary>
        /// <value>
        /// The column value type.
        /// </value>
        public Type Type { get; set; }
    }
}