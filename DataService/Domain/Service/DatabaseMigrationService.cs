namespace BN.PROJECT.DataService;

public class DatabaseMigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(IServiceProvider serviceProvider, ILogger<DatabaseMigrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<BNProjectDbContext>();
                _logger.LogInformation("Starting database migration...");
                await dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migration completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Hier kannst du Logik zum Stoppen des Dienstes hinzufügen, falls erforderlich
        return Task.CompletedTask;
    }
}
