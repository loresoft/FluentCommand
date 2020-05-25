using System;
using System.Collections.Generic;

namespace FluentCommand.Import
{
    /// <summary>
    /// Field definition
    /// </summary>
    public class FieldDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDefinition"/> class.
        /// </summary>
        public FieldDefinition()
        {
            MatchExpressions = new List<string>();
        }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public Type DataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FieldDefinition"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the list match expressions.
        /// </summary>
        /// <value>
        /// The list match expressions.
        /// </value>
        public List<string> MatchExpressions { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Display: {DisplayName}, Name: {FieldName}, DataType: {DataType?.Name}";
        }

    }
}