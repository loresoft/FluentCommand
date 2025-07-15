using FluentCommand.Import;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentCommand;

/// <summary>
/// Provides extension methods for registering FluentCommand import services with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ImportServiceCollectionExtensions
{
    /// <summary>
    /// Registers FluentCommand import services and related dependencies with the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.
    /// </returns>
    /// <remarks>
    /// This method registers the following services:
    /// <list type="bullet">
    /// <item><description><see cref="ImportValidator"/> as a transient service.</description></item>
    /// <item><description><see cref="IImportProcessor"/> as a transient service implemented by <see cref="ImportProcessor"/>.</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddFluentImport(this IServiceCollection services)
    {
        services.TryAddTransient<ImportValidator>();
        services.TryAddKeyedTransient<IImportValidator, ImportValidator>(nameof(ImportValidator));

        services.TryAddTransient<IImportProcessor, ImportProcessor>();

        return services;
    }
}
