using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Host.Monolith;

/// <summary>
///     Enables the use of <see cref="ModuleRoutingConvention"/>
/// </summary>
internal class ModuleRoutingMvcOptionsPostConfigure : IPostConfigureOptions<MvcOptions>
{
    public ModuleRoutingMvcOptionsPostConfigure(IEnumerable<Module> modules)
    {
        Modules = modules;
    }

    private IEnumerable<Module> Modules { get; }

    public void PostConfigure(string name, MvcOptions options)
    {
        options.Conventions.Add(new ModuleRoutingConvention(Modules));
    }
}
