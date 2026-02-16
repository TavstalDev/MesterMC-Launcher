namespace Tavstal.MesterMC.Api.Services.Database;

/// <summary>
/// A background service that periodically cleans up expired content from the database.
/// </summary>
public class DatabaseCleanerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseCleanerService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The factory to create service scopes.</param>
    /// <param name="logger">The logger instance for logging messages.</param>
    public DatabaseCleanerService(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseCleanerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background service, periodically invoking the cleanup process.
    /// </summary>
    /// <param name="stoppingToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the background execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupExpiredContent();

            // run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    /// <summary>
    /// Cleans up expired content from the database, including user logins, play sessions, and server joins.
    /// </summary>
    /// <returns>A task that represents the cleanup operation.</returns>
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