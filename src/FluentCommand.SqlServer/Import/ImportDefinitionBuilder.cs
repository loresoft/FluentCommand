using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Reflection;

namespace FluentCommand.Import;

/// <summary>
/// Provides a fluent API for configuring and building an <see cref="ImportDefinition"/> for a specific model type.
/// Enables automatic mapping and explicit configuration of import fields based on the model's properties.
/// </summary>
/// <typeparam name="TModel">The type of the model to configure import mapping for.</typeparam>
public class ImportDefinitionBuilder<TModel> : ImportDefinitionBuilder
{
    private readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TModel>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportDefinitionBuilder{TModel}"/> class
    /// using the specified <see cref="ImportDefinition"/>.
    /// </summary>
    /// <param name="importDefinition">The <see cref="ImportDefinition"/> to configure.</param>
    public ImportDefinitionBuilder(ImportDefinition importDefinition) : base(importDefinition)
    {
        if (_typeAccessor.TableSchema.HasValue() && _typeAccessor.TableName.HasValue())
            TargetTable($"{_typeAccessor.TableSchema}.{_typeAccessor.TableName}");
        else if (_typeAccessor.TableName.HasValue())
            TargetTable(_typeAccessor.TableName);

        Name(_typeAccessor.Name);
        
        ValidatorKey(nameof(ImportValidator));
    }

    /// <summary>
    /// Automatically maps all eligible properties of the model type to import fields.
    /// Properties marked as not mapped or database generated are excluded.
    /// </summary>
    /// <returns>The current <see cref="ImportDefinitionBuilder{TModel}"/> instance for method chaining.</returns>
    public ImportDefinitionBuilder<TModel> AutoMap()
    {
        var properties = _typeAccessor.GetProperties()
            .Where(p => !p.IsNotMapped && !p.IsDatabaseGenerated)
            .ToList();

        foreach (var property in properties)
        {
            var fieldMapping = ImportDefinition.Fields.FirstOrDefault(m => m.Name == property.Column);
            if (fieldMapping == null)
            {
                fieldMapping = new FieldDefinition { Name = property.Column };
                ImportDefinition.Fields.Add(fieldMapping);
            }

            // set defaults from property
            fieldMapping.CanMap = true;
            fieldMapping.DisplayName = property.DisplayName;
            fieldMapping.DataType = property.MemberType;
            fieldMapping.IsKey = property.IsKey;
            fieldMapping.CanInsert = !property.IsDatabaseGenerated;
            fieldMapping.CanUpdate = !property.IsDatabaseGenerated;
            fieldMapping.IsRequired = property.IsRequired;
        }

        return this;
    }

    /// <summary>
    /// Configures an import field for the specified model property using a property expression.
    /// Allows further customization of the field mapping via the returned <see cref="FieldDefinitionBuilder"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to map (e.g., <c>x =&gt; x.Property</c>).</param>
    /// <returns>A <see cref="FieldDefinitionBuilder"/> for further configuration of the field.</returns>
    public FieldDefinitionBuilder Field<TValue>(Expression<Func<TModel, TValue>> property)
    {
        var memberAccessor = _typeAccessor.FindProperty(property);

        var fieldMapping = ImportDefinition.Fields.FirstOrDefault(m => m.Name == memberAccessor.Column);
        if (fieldMapping == null)
        {
            fieldMapping = new FieldDefinition { Name = memberAccessor.Column };
            ImportDefinition.Fields.Add(fieldMapping);
        }

        var fieldBuilder = new FieldDefinitionBuilder(fieldMapping);

        fieldBuilder
            .DisplayName(memberAccessor.DisplayName)
            .DataType(memberAccessor.MemberType)
            .Required(memberAccessor.IsRequired)
            .IsKey(memberAccessor.IsKey)
            .CanInsert(!memberAccessor.IsDatabaseGenerated)
            .CanUpdate(!memberAccessor.IsDatabaseGenerated);

        return fieldBuilder;
    }

    /// <summary>
    /// Builds an <see cref="ImportDefinition"/> for the specified model type using the provided builder action.
    /// </summary>
    /// <param name="builder">An action to configure the <see cref="ImportDefinitionBuilder{TModel}"/>.</param>
    /// <returns>A fully configured <see cref="ImportDefinition"/> instance.</returns>
    public static ImportDefinition Build(Action<ImportDefinitionBuilder<TModel>> builder)
    {
        var importDefinition = new ImportDefinition();
        var definitionBuilder = new ImportDefinitionBuilder<TModel>(importDefinition);
        builder(definitionBuilder);

        return importDefinition;
    }
}
