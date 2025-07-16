using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FluentCommand.Import;

/// <summary>
/// Represents the configuration and rules for an import operation, including target table, field definitions, and validation settings.
/// </summary>
public class ImportDefinition
{
    /// <summary>
    /// Gets or sets the name of the import operation.
    /// </summary>
    /// <value>
    /// The name of the import.
    /// </value>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the target table into which the uploaded data will be merged.
    /// </summary>
    /// <value>
    /// The target table name.
    /// </value>
    [JsonPropertyName("targetTable")]
    public string TargetTable { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether new data can be inserted into the <see cref="TargetTable"/> if it does not already exist.
    /// </summary>
    /// <value>
    /// <c>true</c> if data can be inserted; otherwise, <c>false</c>. The default is <c>true</c>.
    /// </value>
    [JsonPropertyName("canInsert")]
    public bool CanInsert { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether existing data in the <see cref="TargetTable"/> can be updated.
    /// </summary>
    /// <value>
    /// <c>true</c> if data can be updated; otherwise, <c>false</c>. The default is <c>true</c>.
    /// </value>
    [JsonPropertyName("canUpdate")]
    public bool CanUpdate { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of field definitions that describe how each field is handled during import.
    /// </summary>
    /// <value>
    /// A list of <see cref="FieldDefinition"/> objects representing the import fields.
    /// </value>
    [JsonPropertyName("fields")]
    public List<FieldDefinition> Fields { get; set; } = [];

    /// <summary>
    /// Gets or sets the maximum number of errors allowed before the import operation is aborted.
    /// </summary>
    /// <value>
    /// The maximum number of errors permitted.
    /// </value>
    [JsonPropertyName("maxErrors")]
    public int MaxErrors { get; set; }

    /// <summary>
    /// Gets or sets the type of the validator used to validate data rows during import.
    /// The type must implement <see cref="IImportValidator"/>.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the validator, which must implement <see cref="IImportValidator"/>.
    /// </value>
    [JsonPropertyName("validator")]
    [JsonConverter(typeof(Converters.TypeJsonConverter))]
    [Obsolete("Use ValidatorKey instead. This property will be removed in a future version.")]
    public Type? Validator { get; set; }

    /// <summary>
    /// Gets or sets the service key used to resolve the <see cref="IImportValidator"/> service from the dependency injection container.
    /// The service must be registered in the DI container with this key and implement <see cref="IImportValidator"/>.
    /// </summary>
    /// <value>
    /// The service key used to resolve the validator service from the dependency injection container.
    /// </value>
    [JsonPropertyName("validatorKey")]
    public string? ValidatorKey { get; set; }

    /// <summary>
    /// Retrieves a field definition by its name, ignoring case.
    /// </summary>
    /// <param name="name">The name of the field to retrieve.</param>
    /// <returns>
    /// The <see cref="FieldDefinition"/> that matches the specified name, or <see langword="null"/> if no matching field is found.
    /// </returns>
    public FieldDefinition? GetField(string name)
    {
        return Fields.Find(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Builds a mapping between import fields and source headers using regular expressions defined in <see cref="Fields"/>.
    /// </summary>
    /// <param name="headers">The list of source headers to match against field expressions.</param>
    /// <returns>
    /// A list of <see cref="FieldMap"/> objects representing the mapping between import fields and source headers.
    /// </returns>
    public List<FieldMap> BuildMapping(IReadOnlyCollection<FieldMap>? headers)
    {
        var list = new List<FieldMap>();

        // create a mapping for all fields
        foreach (var field in Fields)
        {
            // Skip fields that cannot be mapped
            if (!field.CanMap)
                continue;

            var map = new FieldMap { Name = field.Name };
            list.Add(map);

            // no expression, don't set mapped index
            if (field.Expressions == null || field.Expressions.Count == 0
                || headers == null || headers.Count == 0)
            {
                continue;
            }

            // for each expression, try to match against headers
            foreach (var expression in field.Expressions)
            {
                foreach (var header in headers)
                {
                    try
                    {
                        if (Regex.IsMatch(header.Name, expression))
                        {
                            map.Index = header.Index;
                            break; // Stop after the first match
                        }
                    }
                    catch
                    {
                        // skip error
                    }
                }

                // Stop checking expressions if we found a match
                if (map.Index.HasValue)
                    break;
            }
        }

        return list;

    }

    /// <summary>
    /// Builds an <see cref="ImportDefinition"/> using the specified builder action.
    /// </summary>
    /// <param name="builder">The action that configures the <see cref="ImportDefinitionBuilder"/>.</param>
    /// <returns>
    /// A fully configured <see cref="ImportDefinition"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public static ImportDefinition Build(Action<ImportDefinitionBuilder> builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var importDefinition = new ImportDefinition();

        var importBuilder = new ImportDefinitionBuilder(importDefinition);
        builder(importBuilder);

        return importDefinition;
    }

    /// <summary>
    /// Returns a string that represents the current <see cref="ImportDefinition"/> instance, including the name, target table, and insert/update settings.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> representation of this <see cref="ImportDefinition"/>.
    /// </returns>
    public override string ToString()
    {
        return $"Name: {Name}, Table: {TargetTable}, Insert: {CanInsert}, Update: {CanUpdate}";
    }
}
