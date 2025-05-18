using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Monolith;

public interface IModuleStartup
{
    void ConfigureServices(IServiceCollection services, IWebHostEnvironment env);
    void Configure(WebApplication app, IWebHostEnvironment env);
}
