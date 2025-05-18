using Common.Monolith;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Artists;

public class Startup : IModuleStartup
{
    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
    {
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
    }
}
