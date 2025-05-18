using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Host;

public class Program
{
    private static string AppName { get; } = "ModularMonolith.NET";
    public static string Version { get; } = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    public static string Env => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    public static bool IsLocal => Env?.Equals("Local", StringComparison.OrdinalIgnoreCase) ?? false;
    public static bool IsTest => Env?.Equals("Test", StringComparison.OrdinalIgnoreCase) ?? false;

    public static async Task Main(string[] args)
    {
        var log = CreateBootstrapLogger();

        try
        {
            /*
                basic setup of an ASP.NET application; this is mostly boilerplate and can be adjusted to taste.

                note that the configuration setup diverges a bit from the standard appsettings.json approach, simply
                because *I* personally find environment variables and .env to be more straightforward to manage; it
                doesn't have anything to do with the monolith approach.
            */
            log.Information("{App} {Version} is initializing", AppName, Version);
            log.Information("Environment: {Env}", Env);

            // load environment variables from a .env file, if one is present
            // this is meant to simplify local development; we shouldn't ship a .env file in a container image
            Dotenv.Load(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.Parent.FullName, ".env"));

            // use the built-in ASP.NET configuration pattern to source configuration details
            // from environment variables; note that all application-defined items must be prefixed
            // with MM.NET_, for example MM.NET_DATABASE_CONNECTION_STRING
            var config = new Configuration();
            new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "MM.NET_")
                .Build()
                .Bind(config);

            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls($"http://*:{config.PORT}");

            // make the application configuration available in the DI container
            builder.Services.AddSingleton<Configuration>(config);

            // ensure background worker exceptions don't kill the app (override default behavior)
            builder.Services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });

            // configure logging
            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
                options.RequestBodyLogLimit = 1024 * 16;
                options.CombineLogs = true;
            });
            builder.Host.UseSerilog((_, _, options) =>
            {
                options
                    //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", v => v.StartsWith("/health")))
                    .WriteTo
                        .Console(formatProvider: new CultureInfo("en-US"), theme: AnsiConsoleTheme.Sixteen, applyThemeToRedirectedOutput: true);
            });

            /*
                begin bootstrapping the monolith.

                this approach uses the older ASP.NET pattern that splits service configuration (ConfigureServices())
                and HTTP pipeline configuration (Configure()) into two steps.

                this is necessary because we must configure services for all modules before we can configure the pipeline,
                and there's simply no way we can follow the more modern ASP.NET pattern of using a single method
                to configure both services and the pipeline in one step.

                for consistency, the Host module uses the same Startup class as the other modules, and we'll use it
                to configure the rest of the application, including all of the modules.
            */

            // instantiate the Host's startup class
            var startup = new Startup();

            // configure dependency injection/services
            // the Host will bootstrap the modules
            startup.ConfigureServices(builder.Services, builder.Environment);

            var app = builder.Build();

            // configure the pipeline
            startup.Configure(app, builder.Environment);

            /*
                run database migrations to aid in local development and testing scenarios

                whether you'd actually want to do this is highly personal, but it's included here to make the example
                easier to work with, and to illustrate how it would be done if you wanted to do it
            */
            if (IsLocal || IsTest || true)
            {
                using var scope = app.Services.CreateScope();
                await startup.Migrate(scope.ServiceProvider);
            }

            // everything is configured, let's go!
            log.Information("Starting...");
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            log.Fatal(ex, "Failed to start application: {Message}", ex.Message);
            throw;
        }
    }

    private static ILogger CreateBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: new CultureInfo("en-US"), theme: AnsiConsoleTheme.Sixteen, applyThemeToRedirectedOutput: true)
            .CreateBootstrapLogger();

        return Log.ForContext<Program>();
    }
}