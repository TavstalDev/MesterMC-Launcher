namespace Tavstal.MesterMC.Api.Services.Database;

public class DatabaseCleanerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    public DatabaseCleanerService(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseCleanerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupExpiredContent();

            // run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task CleanupExpiredContent()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CustomDbContext>();
            
            // User Logins
            await db.ClearExpiredUserLoginsAsync();
            
            // User Play Sessions
            await db.ClearExpiredUserPlaySessionsAsync();
            
            // Server joins
            await db.ClearExpiredServerJoinsAsync();

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cleanup failed");
        }
    }
}