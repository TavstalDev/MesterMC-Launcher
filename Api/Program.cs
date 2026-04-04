using DotEnv.Core;

namespace Tavstal.MesterMC.Api;

/// <summary>
/// Application entry point and host builder factory for the API.
/// 
/// This static class exposes:
/// <br/>- a public read-only flag <see cref="IsDevelopment"/> indicating if the host is running in Development mode,
/// <br/>- a public read-only <see cref="ContentRoot"/> path for the application's content root,
/// <br/>- the <see cref="Main(string[])"/> method which starts the host,
/// <br/>- and a private factory <see cref="CreateHostBuilder(string[])"/> which builds the configured host.
/// </summary>
public static class Program
{
    private static bool _isDevelopment;
    /// <summary>
    /// Gets a value indicating whether the host is running in the Development environment.
    /// This value is initialized when the host configuration runs (see <see cref="CreateHostBuilder(string[])"/>).
    /// </summary>
    public static bool IsDevelopment => _isDevelopment;
    private static string _contentRoot = string.Empty;
    /// <summary>
    /// Gets the content root path that the host uses (usually the application's folder).
    /// This is initialized during host configuration and can be used by other startup code to resolve files.
    /// </summary>
    public static string ContentRoot => _contentRoot;
    
    /// <summary>
    /// Application entry point. Builds and runs the configured host.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to the host builder.</param>
    public static async Task Main(string[] args)
    {
        try
        {
            using var cts = new CancellationTokenSource();
            await CreateHostBuilder(args).Build().RunAsync(cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Host terminated unexpectedly: {ex}");
        }
    }
    
    /// <summary>
    /// Creates and configures an <see cref="IHostBuilder"/> for the application.
    /// 
    /// The host builder is configured to:
    /// <br/>- use the <see cref="Startup"/> class to configure services and the request pipeline,
    /// <br/>- capture and expose whether the host is running in Development mode via <see cref="IsDevelopment"/>,
    /// <br/>- capture and expose the content root via <see cref="ContentRoot"/>,
    /// <br/>- load environment variables from a local ".env" file (if present) using <c>EnvLoader</c>,
    /// <br/>- load the appropriate appsettings JSON file depending on the environment,
    /// <br/>- and include environment variables into the configuration.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to Host.CreateDefaultBuilder.</param>
    /// <returns>A configured <see cref="IHostBuilder"/> instance.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                _isDevelopment = builderContext.HostingEnvironment.IsDevelopment();
                _contentRoot = builderContext.HostingEnvironment.ContentRootPath;
                
                // Load .env file
                new EnvLoader()
                    .AddEnvFile(Path.Combine(_contentRoot, ".env"))
                    .Load();

                
                config.AddJsonFile(_isDevelopment ? "appsettings.Development.json" : "appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .UseContentRoot(Directory.GetCurrentDirectory());
}