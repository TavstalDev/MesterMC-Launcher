using DotEnv.Core;

namespace Tavstal.MesterMC.Api;

public static class Program
{
    private static bool _isDevelopment;
    public static bool IsDevelopment => _isDevelopment;
    private static string _contentRoot;
    public static string ContentRoot => _contentRoot;
    
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
    
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