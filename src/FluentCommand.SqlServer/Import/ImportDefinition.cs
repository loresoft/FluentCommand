using System;
using System.Collections.Generic;

namespace FluentCommand.Import
{
    /// <summary>
    /// Import definition
    /// </summary>
    public class ImportDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDefinition"/> class.
        /// </summary>
        public ImportDefinition()
        {
            Fields = new List<FieldDefinition>();
        }

        /// <summary>
        /// Gets or sets the name of the import.
        /// </summary>
        /// <value>
        /// The name of the import.
        /// </value>
        public string ImportName { get; set; }

        /// <summary>
        /// Gets or sets the list of field definitions.
        /// </summary>
        /// <value>
        /// The list of field definitions.
        /// </value>
        public List<FieldDefinition> Fields { get; set; }


        /// <summary>
        /// Builds an <see cref="ImportDefinition"/> using the specified builder action.
        /// </summary>
        /// <param name="builder">The builder action delegate.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">builder is null</exception>
        public static ImportDefinition Build(Action<ImportDefinitionBuilder> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var importDefinition = new ImportDefinition();

            var importBuilder = new ImportDefinitionBuilder(importDefinition);
            builder(importBuilder);

            return importDefinition;
        }

    }
}