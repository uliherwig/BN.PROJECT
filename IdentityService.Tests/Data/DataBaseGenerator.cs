using Microsoft.EntityFrameworkCore;

namespace BN.PROJECT.IdentityService.Tests
{
    public class DatabaseGenerator
    {
        public static IdentityDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>()
                    .UseInMemoryDatabase(databaseName: "IdentityDb");
            var context = new IdentityDbContext(optionsBuilder.Options);
            return context;
        }

        public static IdentityDbContext SeedDataBase()
        {
            var context = CreateContext();

            // delete all existing data
            context.Users.RemoveRange(context.Users);
            context.UserRoles.RemoveRange(context.UserRoles);
            context.Roles.RemoveRange(context.Roles);
            context.Sessions.RemoveRange(context.Sessions);
            context.SaveChanges();

            // Erstellen von Beispiel-Daten
            var user1 = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = "Max",
                LastName = "Mustermann",
                Username = "user1",
                Email = "user1@example.com"
            };

            var user2 = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = "Erika",
                LastName = "Musterfrau",
                Username = "user2",
                Email = "user2@example.com"
            };

            var role1 = new Role
            {
                Id = Guid.NewGuid(),
                Description = "Administrator",
                Name = "Admin"
            };

            var role2 = new Role
            {
                Id = Guid.NewGuid(),
                Description = "Normaler Benutzer",
                Name = "User"
            };

            var userRole1 = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = user1.UserId,
                RoleId = role1.Id
            };

            var userRole2 = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = user2.UserId,
                RoleId = role2.Id
            };

            var session1 = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = user1.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                LastActive = DateTime.UtcNow,
                SignedOutAt = DateTime.UtcNow
            };

            var session2 = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = user2.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                LastActive = DateTime.UtcNow,
                SignedOutAt = DateTime.UtcNow
            };

            // Hinzufügen der Daten zur Datenbank
            context.Users.AddRange(user1, user2);
            context.Roles.AddRange(role1, role2);
            context.UserRoles.AddRange(userRole1, userRole2);
            context.Sessions.AddRange(session1, session2);

            context.SaveChanges();

            return context;
        }
    }
}