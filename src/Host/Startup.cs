using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common;
using Common.Monolith;
using Host.Monolith;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Host
{
    public class Startup : IModuleStartup, IMigratable
    {
        public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            // Set up Serilog for additional Structured logging to Console and Application Insights
            services.AddHttpContextAccessor();

            var config = services.GetSingleton<Configuration>();

            /*
                adjust the ApplicationPartManager to work with modules. we want to explicitly control
                which assemblies are available for controller discovery
            */
            services.AddControllers()
                .ConfigureApplicationPartManager(manager =>
                {
                    // clear anything it might have discovered already
                    manager.ApplicationParts.Clear();

                    // enable discovery of controllers defined within the Host itself (version, health checks, etc)
                    manager.ApplicationParts.Add(new AssemblyPart(Assembly.GetExecutingAssembly()));

                    // enable the part manager to find classes derived from ControllerBase, potentially with
                    // with an 'internal' access modifier. modules should make everything internal to discourage
                    // direct project references; we want them to communicate over HTTP *only*
                    manager.FeatureProviders.Add(new InternalControllerFeatureProvider());
                })
                .AddJsonOptions(options =>
                {
                    // adjust to taste
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // enable the use of the [module] rout identifier in the Controller [Route()] attribute
            services.AddTransient<IPostConfigureOptions<MvcOptions>, ModuleRoutingMvcOptionsPostConfigure>();

            /*
                add each module, and assign the module's route

                this registers the module's assembly with the ApplicationPartManager, allowing the controllers
                to be discovered later in the process.

                each module's Startup class is insantiated and the ConfigureServices() method is called, allowing
                the module to register its dependencies independently. remember to *always* use keyed services;
                the dependency injection container is shared among the Host and all modules, and if care isn't taken
                to isolate dependencies with keys, dependencies may 'bleed' from one module to another
            */
            services.AddModule<Artists.Startup>("artists");
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            app.UseHttpLogging();

            app.UseRouting();

            // discover controllers among the assemblies registered with the ApplicationPartManager earlier
            app.MapControllers();

            // configure the pipeline for each of the modules
            // note that care must be taken when adding middleware, as it can create conflicts among modules
            var modules = app.Services.GetRequiredService<IEnumerable<Module>>();
            foreach (var module in modules)
            {
                module.Startup.Configure(app, env);
            }
        }

        public async Task Migrate(IServiceProvider services)
        {
            // get a list of all of the modules registered with DI and that implement
            // IMigratable, and invoke the Migrate() method within the module's Startup class
            var tasks = services
                .GetRequiredService<IEnumerable<Module>>()
                .Where(module => module.Startup is IMigratable)
                .Select(module => ((IMigratable)module.Startup).Migrate(services));

            // run these one at a time serially to avoid concurrent database updates
            // this may take a while if someone is running the project locally the first time
            foreach (var task in tasks)
            {
                await task;
            }
        }
    }
}
