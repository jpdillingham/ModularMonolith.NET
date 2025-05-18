using System.Reflection;
using Common.Monolith;

/// <summary>
///     Provides a simple representation of a Module.
/// </summary>
internal class Module
{
    public Module(string routePrefix, IModuleStartup startup)
    {
        RoutePrefix = routePrefix;
        Startup = startup;
    }

    public string RoutePrefix { get; }
    public IModuleStartup Startup { get; }
    public Assembly Assembly => Startup.GetType().Assembly;
}
