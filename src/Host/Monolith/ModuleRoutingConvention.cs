using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serilog;

namespace Host.Monolith;

/// <summary>
///     Enables the use of the [module] route identifier in the Controller <see cref="Route"/> attribute
/// </summary>
internal class ModuleRoutingConvention : IActionModelConvention
{
    public ModuleRoutingConvention(IEnumerable<Module> modules)
    {
        ModuleCache = modules.ToDictionary(m => m.Assembly.FullName, m => m);
    }

    private Dictionary<string, Module> ModuleCache { get; }
    private ILogger Log { get; } = Serilog.Log.ForContext<ModuleRoutingConvention>();

    public void Apply(ActionModel action)
    {
        if (ModuleCache.TryGetValue(action.Controller.ControllerType.Assembly.FullName, out var module))
        {
            action.RouteValues.Add("module", module.RoutePrefix);
        }
    }
}
