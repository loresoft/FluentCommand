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
            CanInsert = true;
            CanUpdate = true;
            CanMap = true;
            Expressions = new List<string>();
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
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public Type DataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is a key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this field is a key; otherwise, <c>false</c>.
        /// </value>
        public bool IsKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field can insert.
        /// </summary>
        /// <value>
        /// <c>true</c> if this field can insert; otherwise, <c>false</c>.
        /// </value>
        public bool CanInsert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field can update.
        /// </summary>
        /// <value>
        /// <c>true</c> if this field can update; otherwise, <c>false</c>.
        /// </value>
        public bool CanUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field can be mapped by the users.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this field can be mapped; otherwise, <c>false</c>.
        /// </value>
        public bool CanMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FieldDefinition"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }


        /// <summary>
        /// Gets or sets the default value generation.
        /// </summary>
        /// <value>
        /// The default value generation.
        /// </value>
        public FieldDefault? Default { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the field translator <see cref="Type"/>.
        /// </summary>
        /// <value>
        /// The field translator <see cref="Type"/>.
        /// </value>
        public Type Translator { get; set; }

        /// <summary>
        /// Gets or sets the list match expressions.
        /// </summary>
        /// <value>
        /// The list match expressions.
        /// </value>
        public List<string> Expressions { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Display: {DisplayName}, Name: {Name}, DataType: {DataType?.Name}";
        }

    }
}