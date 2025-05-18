using Common;
using Common.Monolith;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Host.Monolith;

/// <summary>
///     Provides an <see cref="AddModule{TStartup}"/> extension method for <see cref="IServiceCollection"/>.
/// </summary>
internal static class ModuleServiceCollection
{
    /// <summary>
    ///     Adds a module.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="routePrefix">The route prefix for the module.</param>
    /// <typeparam name="TStartup">The type of the startup class of the module.</typeparam>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddModule<TStartup>(this IServiceCollection services, string routePrefix)
        where TStartup : IModuleStartup, new()
    {
        // register the assembly so the controllers can be discovered
        // the actual discovery takes place later
        services.AddControllers().ConfigureApplicationPartManager(manager =>
            manager.ApplicationParts.Add(new AssemblyPart(typeof(TStartup).Assembly)));

        var env = services.GetSingleton<IWebHostEnvironment>();

        // instantiate the module's startup class
        var startup = new TStartup();

        // configure the module's services
        startup.ConfigureServices(services, env);

        // add the module to the DI container
        services.AddSingleton(new Module(routePrefix, startup));

        return services;
    }
}
