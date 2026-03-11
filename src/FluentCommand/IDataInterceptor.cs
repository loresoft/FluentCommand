namespace FluentCommand;

/// <summary>
/// A marker interface for FluentCommand interceptors.
/// </summary>
/// <remarks>
/// Implement <see cref="IDataConnectionInterceptor"/> or <see cref="IDataCommandInterceptor"/> to intercept
/// specific points in the data access pipeline.
/// </remarks>
public interface IDataInterceptor
{
}
