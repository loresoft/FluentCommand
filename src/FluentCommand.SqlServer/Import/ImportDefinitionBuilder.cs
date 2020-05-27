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
            if (importDefinition == null)
                throw new ArgumentNullException(nameof(importDefinition));

            _importDefinition = importDefinition;
        }

        /// <summary>
        /// Sets the import name for the <see cref="ImportDefinition"/>
        /// </summary>
        /// <param name="name">The import name value.</param>
        /// <returns>Fluent builder for <see cref="ImportDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when value is null</exception>
        public ImportDefinitionBuilder Name(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            _importDefinition.Name = name;
            return this;
        }
        
        /// <summary>
        /// Sets the target table for the <see cref="ImportDefinition"/>
        /// </summary>
        /// <param name="name">The target table name.</param>
        /// <returns>Fluent builder for <see cref="ImportDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when value is null</exception>
        public ImportDefinitionBuilder TargetTable(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            
            _importDefinition.TargetTable = name;
            return this;
        }

        /// <summary>
        /// Sets weather data can be inserted into target table for the <see cref="ImportDefinition" />
        /// </summary>
        /// <param name="value">if set to <c>true</c> allow inserts.</param>
        /// <returns>
        /// Fluent builder for <see cref="ImportDefinition" />
        /// </returns>
        public ImportDefinitionBuilder CanInsert(bool value = true)
        {
            _importDefinition.CanInsert = value;
            return this;
        }

        /// <summary>
        /// Sets weather data can be updated in target table for the <see cref="ImportDefinition" />
        /// </summary>
        /// <param name="value">if set to <c>true</c> allow updates.</param>
        /// <returns>
        /// Fluent builder for <see cref="ImportDefinition" />
        /// </returns>
        public ImportDefinitionBuilder CanUpdate(bool value = true)
        {
            _importDefinition.CanUpdate = value;
            return this;
        }


        /// <summary>
        /// Maps import fields with specified fluent <paramref name="builder"/> action.
        /// </summary>
        /// <param name="builder">The fluent builder action.</param>
        /// <returns>Fluent builder for <see cref="ImportDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when the builder action in null</exception>
        public ImportDefinitionBuilder Field(Action<FieldDefinitionBuilder> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return Field(null, builder);
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
        public ImportDefinitionBuilder Field(string fieldName, Action<FieldDefinitionBuilder> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var fieldMapping = _importDefinition.Fields.FirstOrDefault(m => m.Name == fieldName);
            if (fieldMapping == null)
            {
                fieldMapping = new FieldDefinition { Name = fieldName };
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
        public FieldDefinitionBuilder Field(string fieldName)
        {
            if (fieldName == null) 
                throw new ArgumentNullException(nameof(fieldName));

            var fieldMapping = _importDefinition.Fields.FirstOrDefault(m => m.Name == fieldName);
            if (fieldMapping == null)
            {
                fieldMapping = new FieldDefinition { Name = fieldName };
                _importDefinition.Fields.Add(fieldMapping);
            }

            var fieldBuilder = new FieldDefinitionBuilder(fieldMapping);
            return fieldBuilder;
        }
    }
}