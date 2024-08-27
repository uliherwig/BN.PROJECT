namespace BN.TRADER.IdentityService
{
    public class KeycloakConfigStartUp : IHostedService
    {
        private readonly ILogger<KeycloakConfigStartUp> _logger;

        public KeycloakConfigStartUp(ILogger<KeycloakConfigStartUp> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KeycloakConfigStartUp is starting.");
            return Task.CompletedTask; // Indicate that the task has completed
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KeycloakConfigStartUp is stopping.");
            return Task.CompletedTask; // Indicate that the task has completed
        }

    }
}
