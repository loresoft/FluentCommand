using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentCommand.Import
{
    /// <summary>
    /// The shared data for the current import process
    /// </summary>
    public class ImportProcessContext
    {
        private readonly Lazy<IReadOnlyList<ImportFieldMapping>> _mappedFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportProcessContext"/> class.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="importData">The import data.</param>
        /// <param name="userName">Name of the user.</param>
        public ImportProcessContext(ImportDefinition definition, ImportData importData, string userName)
        {
            Definition = definition;
            ImportData = importData;
            UserName = userName;

            _mappedFields = new Lazy<IReadOnlyList<ImportFieldMapping>>(GetMappedFields);
        }

        /// <summary>
        /// Gets the name of the user importing the data.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; }

        /// <summary>
        /// Gets the import definition.
        /// </summary>
        /// <value>
        /// The import definition.
        /// </value>
        public ImportDefinition Definition { get; }

        /// <summary>
        /// Gets the import data.
        /// </summary>
        /// <value>
        /// The import data.
        /// </value>
        public ImportData ImportData { get; }

        /// <summary>
        /// Gets the mapped fields.
        /// </summary>
        /// <value>
        /// The mapped fields.
        /// </value>
        public IReadOnlyList<ImportFieldMapping> MappedFields => _mappedFields.Value;

        private List<ImportFieldMapping> GetMappedFields()
        {
            var list = new List<ImportFieldMapping>();

            var firstRow = ImportData.Data.FirstOrDefault();
            var columns = firstRow?.Length ?? 0;

            foreach (var field in Definition.Fields)
            {
                // if default value, include
                if (field.Default.HasValue)
                {
                    list.Add(new ImportFieldMapping(field));
                    continue;
                }

                var name = field.Name;

                // if mapped to an index, include
                var mapping = ImportData.Mappings.FirstOrDefault(f => f.Name == name);
                if (mapping?.Index == null)
                {
                    if (field.IsRequired)
                        throw new InvalidOperationException($"Missing required field mapping for '{name}'");

                    continue;
                }

                if (mapping.Index.Value >= columns)
                    throw new IndexOutOfRangeException(
                        $"The mapped index {mapping.Index.Value} for field '{name}' is out of range of {columns}");

                list.Add(new ImportFieldMapping(field, mapping));
            }

            return list;
        }
    }
}