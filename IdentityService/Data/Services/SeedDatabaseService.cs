namespace BN.PROJECT.IdentityService;

public class SeedDatabaseService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public SeedDatabaseService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var keycloakServiceClient = scope.ServiceProvider.GetRequiredService<IKeycloakServiceClient>();
            var identityRepository = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();

            var defaultUsersCreated = await identityRepository.GetUserByNameAsync("bn-admin") != null;
            if (defaultUsersCreated)
            {
                return;
            }

            var user1 = await keycloakServiceClient.GetUserByName("bn-admin");
            var user2 = await keycloakServiceClient.GetUserByName("bn-user");

            await identityRepository.AddUserAsync(user1);
            await identityRepository.AddUserAsync(user2);

            var role1 = new Role
            {
                Id = Guid.NewGuid(),
                Name = "admin",
                Description = "Admin role"
            };
            await identityRepository.AddRoleAsync(role1);

            var role2 = new Role
            {
                Id = Guid.NewGuid(),
                Name = "trader",
                Description = "Trader role"
            };
            await identityRepository.AddRoleAsync(role2);

            var userRole1 = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = user1.UserId,
                RoleId = role1.Id,
                AssignedAt = DateTime.UtcNow
            };
            await identityRepository.AddUserRoleAsync(userRole1);

            var userRole2 = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = user2.UserId,
                RoleId = role2.Id,
                AssignedAt = DateTime.UtcNow
            };
            await identityRepository.AddUserRoleAsync(userRole2);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}