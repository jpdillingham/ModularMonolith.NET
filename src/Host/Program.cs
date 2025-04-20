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
    
    public static async Task Main(string[] args)
    {
        var log = CreateBootstrapLogger();

        try
        {
            log.Information("{App} {Version} is initializing", AppName, Version);
            log.Information("Environment: {Env}", Env);

            // load environment variables from a .env file, if one is present
            // this is meant to simplify local development; we shouldn't ship a .env file in a container image
            Dotenv.Load(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.Parent.FullName, ".env"));
            
            var builder = WebApplication.CreateBuilder(args);

            // use the built-in ASP.NET configuration pattern to source configuration details
            // from environment variables; note that all application-defined items must be prefixed
            // with MM.NET_, for example MM.NET_DATABASE_CONNECTION_STRING
            var config = new Configuration();
            new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "MM.NET_")
                .Build()
                .Bind(config);

            // make the application configuration injectable (modules must not try to source them directly)
            builder.Services.AddSingleton<Configuration>(config);
            
            // stop background worker exceptions from killing the app
            builder.Services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });

            builder.WebHost.UseUrls($"http://*:{config.PORT}");
            
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

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            log.Information("Configured {Services} services", builder.Services.Count);

            var app = builder.Build();

            app.UseHttpLogging();
            app.MapOpenApi();
            
            app.UseAuthorization();
            app.MapControllers();

            log.Information("Configured pipeline");

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