using System;
using System.Linq;

namespace FluentCommand.Import
{
    /// <summary>
    /// Fluent builder for <see cref="ImportDefinition"/>
    /// </summary>
    public class ImportDefinitionBuilder
    {
        private readonly ImportDefinition _importDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDefinitionBuilder"/> class.
        /// </summary>
        /// <param name="importDefinition">The import definition.</param>
        /// <exception cref="ArgumentNullException">importDefinition</exception>
        public ImportDefinitionBuilder(ImportDefinition importDefinition)
        {
            _importDefinition = importDefinition 
                                ?? throw new ArgumentNullException(nameof(importDefinition));
        }

        /// <summary>
        /// Sets the import name for the <see cref="ImportDefinition"/>
        /// </summary>
        /// <param name="name">The import name value.</param>
        /// <returns>Fluent builder for <see cref="ImportDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when value is null</exception>
        public ImportDefinitionBuilder ImportName(string name)
        {
            _importDefinition.ImportName = name;
            return this;
        }

        /// <summary>
        /// Maps import fields with specified fluent <paramref name="builder"/> action.
        /// </summary>
        /// <param name="builder">The fluent builder action.</param>
        /// <returns>Fluent builder for <see cref="ImportDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when the builder action in null</exception>
        public ImportDefinitionBuilder Map(Action<FieldDefinitionBuilder> builder)
        {
            if (builder == null) 
                throw new ArgumentNullException(nameof(builder));

            return Map(null, builder);
        }

        /// <summary>
        /// Maps import fields with specified fluent <paramref name="builder" /> action.
        /// </summary>
        /// <param name="fieldName">Name of the field to map.</param>
        /// <param name="builder">The fluent builder action.</param>
        /// <returns>
        /// Fluent builder for <see cref="ImportDefinition" />
        /// </returns>
        /// <exception cref="ArgumentNullException">when the builder action in null</exception>
        public ImportDefinitionBuilder Map(string fieldName, Action<FieldDefinitionBuilder> builder)
        {
            if (builder == null) 
                throw new ArgumentNullException(nameof(builder));

            var fieldMapping = _importDefinition.Fields.FirstOrDefault(m => m.FieldName == fieldName);
            if (fieldMapping == null)
            {
                fieldMapping = new FieldDefinition { FieldName = fieldName };
                _importDefinition.Fields.Add(fieldMapping);
            }

            var fieldBuilder = new FieldDefinitionBuilder(fieldMapping);
            builder(fieldBuilder);

            // default type
            if (fieldMapping.DataType == null)
                fieldMapping.DataType = typeof(string);

            return this;
        }

        /// <summary>
        /// Maps import fields with specified <paramref name="fieldName"/>.
        /// </summary>
        /// <param name="fieldName">Name of the field to map.</param>
        /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when the field name in null</exception>
        public FieldDefinitionBuilder Map(string fieldName)
        {
            var fieldMapping = _importDefinition.Fields.FirstOrDefault(m => m.FieldName == fieldName);
            if (fieldMapping == null)
            {
                fieldMapping = new FieldDefinition { FieldName = fieldName };
                _importDefinition.Fields.Add(fieldMapping);
            }

            var fieldBuilder = new FieldDefinitionBuilder(fieldMapping);
            return fieldBuilder;
        }
    }
}