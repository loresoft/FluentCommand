namespace FluentCommand.Import;

/// <summary>
/// Factory method used to resolve all services. For multiple instances, it will resolve against <see cref="IEnumerable{T}" />
/// </summary>
/// <param name="serviceType">Type of service to resolve</param>
/// <returns>An instance of type <paramref name="serviceType" /></returns>
public delegate object ImportFactory(Type serviceType);
