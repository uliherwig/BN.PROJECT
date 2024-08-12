public class DatabaseMigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseMigrationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Hier kannst du Logik zum Stoppen des Dienstes hinzufügen, falls erforderlich
        return Task.CompletedTask;
    }
}
