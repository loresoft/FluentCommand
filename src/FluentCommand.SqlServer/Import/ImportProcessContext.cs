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
        private readonly Dictionary<Type, object> _serviceCache;
        private readonly ImportFactory _importFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportProcessContext" /> class.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="importData">The import data.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="importFactory">The import service factory.</param>
        public ImportProcessContext(ImportDefinition definition, ImportData importData, string userName, ImportFactory importFactory)
        {
            Definition = definition;
            _serviceCache = new Dictionary<Type, object>();
            ImportData = importData;
            UserName = userName;
            _importFactory = importFactory;
            Errors = new List<Exception>();

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


        /// <summary>
        /// Gets or sets the list errors that occurred.
        /// </summary>
        /// <value>
        /// The list errors that occurred.
        /// </value>
        public List<Exception> Errors { get; set; }


        /// <summary>
        /// Gets the service instance.
        /// </summary>
        /// <param name="type">The type of service.</param>
        /// <returns></returns>
        public object GetService(Type type)
        {
            if (_serviceCache.TryGetValue(type, out object instance))
                return instance;

            instance = _importFactory(type);
            _serviceCache.Add(type, instance);

            return instance;
        }


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