using System;
using System.Collections.Generic;

namespace FluentCommand.Import
{
    /// <summary>
    /// Fluent builder for <see cref="FieldDefinition"/>
    /// </summary>
    public class FieldDefinitionBuilder
    {
        private readonly FieldDefinition _fieldDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDefinition"/> class.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <exception cref="ArgumentNullException">fieldDefinition</exception>
        public FieldDefinitionBuilder(FieldDefinition fieldDefinition)
        {
            if (fieldDefinition == null)
                throw new ArgumentNullException(nameof(fieldDefinition));

            _fieldDefinition = fieldDefinition;
        }

        /// <summary>
        /// Sets the field name for the <see cref="FieldDefinition"/>
        /// </summary>
        /// <param name="value">The field name value.</param>
        /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when value is null</exception>
        public FieldDefinitionBuilder FieldName(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _fieldDefinition.FieldName = value;

            return this;
        }

        /// <summary>
        /// Sets the display name for the <see cref="FieldDefinition"/>
        /// </summary>
        /// <param name="value">The display name value.</param>
        /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when value is null</exception>
        public FieldDefinitionBuilder DisplayName(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _fieldDefinition.DisplayName = value;

            return this;
        }

        /// <summary>
        /// Sets the data type for the <see cref="FieldDefinition"/>
        /// </summary>
        /// <param name="value">The data type value.</param>
        /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when value is null</exception>
        public FieldDefinitionBuilder DataType(Type value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _fieldDefinition.DataType = value;

            return this;
        }

        /// <summary>
        /// Sets the data type for the <see cref="FieldDefinition"/>
        /// </summary>
        /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
        public FieldDefinitionBuilder DataType<T>()
        {
            _fieldDefinition.DataType = typeof(T);
            return this;
        }

        /// <summary>
        /// Sets the field as required for the <see cref="FieldDefinition"/>
        /// </summary>
        /// <param name="value">The required field value.</param>
        /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
        public FieldDefinitionBuilder Required(bool value = true)
        {
            _fieldDefinition.Required = value;
            return this;
        }

        /// <summary>
        /// Sets the match regular expression for the <see cref="FieldDefinition"/>
        /// </summary>
        /// <param name="value">The regular expression value.</param>
        /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when value is null</exception>
        public FieldDefinitionBuilder Expression(string value)
        {
            if (value == null) 
                throw new ArgumentNullException(nameof(value));

            _fieldDefinition.MatchExpressions.Add(value);

            return this;
        }

        /// <summary>
        /// Sets the match regular expression for the <see cref="FieldDefinition"/>
        /// </summary>
        /// <param name="values">The regular expression values.</param>
        /// <returns>Fluent builder for <see cref="FieldDefinition"/></returns>
        /// <exception cref="ArgumentNullException">when values is null</exception>
        public FieldDefinitionBuilder Expressions(IEnumerable<string> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            _fieldDefinition.MatchExpressions.AddRange(values);

            return this;
        }
    }
}