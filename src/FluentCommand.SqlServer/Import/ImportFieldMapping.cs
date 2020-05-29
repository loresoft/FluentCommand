using System;

namespace FluentCommand.Import
{
    /// <summary>
    /// Import field mapping
    /// </summary>
    public class ImportFieldMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportFieldMapping"/> class.
        /// </summary>
        /// <param name="fieldDefinition">The definition.</param>
        public ImportFieldMapping(FieldDefinition fieldDefinition) : this(fieldDefinition, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportFieldMapping"/> class.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <param name="fieldMap">The field field map.</param>
        /// <exception cref="ArgumentNullException">definition is null</exception>
        public ImportFieldMapping(FieldDefinition fieldDefinition, FieldMap fieldMap)
        {
            if (fieldDefinition == null)
                throw new ArgumentNullException(nameof(fieldDefinition));

            Definition = fieldDefinition;
            FieldMap = fieldMap;
        }

        /// <summary>
        /// Gets the field definition.
        /// </summary>
        /// <value>
        /// The field definition.
        /// </value>
        public FieldDefinition Definition { get; }

        /// <summary>
        /// Gets the field mapping.
        /// </summary>
        /// <value>
        /// The field mapping.
        /// </value>
        public FieldMap FieldMap { get; }
    }
}