using System;
using System.Collections.Generic;

/// <remarks>
/// Deprecated: prefer DRG.Framework.ServiceLocator for hierarchical dependency management.
/// </remarks>
[Obsolete("Use DRG.Framework.ServiceLocator instead.")]
public class AppServiceLocator : IDisposable
{
    private static AppServiceLocator cachedInstance;
    public static AppServiceLocator instance
    {
        get
        {
            cachedInstance ??= new AppServiceLocator();
            return cachedInstance;
        }
    }

    private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    public void AddService<T>(T service) where T : class
    {
        services[typeof(T)] = service;
    }

    public T GetService<T>() where T : class
    {
        if (services.TryGetValue(typeof(T), out object service))
        {
            return (T)service;
        }

        throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
    }

    public void Dispose()
    {
        services.Clear();
        cachedInstance = null;
    }
}
