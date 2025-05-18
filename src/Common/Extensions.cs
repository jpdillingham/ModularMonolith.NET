using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Common;

/// <summary>
///     Works around an issue with providing modules with application configuration at startup, but could be used for other things which 1) are singletons added to DI in
///     the Host Startup and 2) are things that legitimately should be accessible to modules at startup.  This should be a very short list, otherwise someone is doing
///     something wrong.
/// </summary>
public static class Extensions
{
    public static T GetSingleton<T>(this IServiceCollection services)
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(T));

        if (!descriptors.Any())
        {
            throw new MonolithStartupException($"An instance of {typeof(T)} has not been registered, but {nameof(GetSingleton)} was called");
        }

        if (descriptors.Count() > 1)
        {
            throw new MonolithStartupException($"More than one instance of {typeof(T)} has been registered; expected only one");
        }

        var service = descriptors.First();

        if (service.Lifetime != ServiceLifetime.Singleton)
        {
            throw new MonolithStartupException($"One and only one instance of {typeof(T)} has been registered (that's good!) but it doesn't have a lifetime of {nameof(ServiceLifetime.Singleton)}.  That doesn't make any sense!");
        }

        if (service.ImplementationInstance is null)
        {
            throw new MonolithStartupException($"The singleton instance of {typeof(T)} has been registered, but the implementation is null.  It's unusable!");
        }

        if (service.ImplementationInstance is T config)
        {
            return config;
        }

        throw new MonolithStartupException($"The implementation for the singleton instance of {typeof(T)} doesn't appear to be of type {typeof(T)}.  That's weird! And wrong.");
    }
}
