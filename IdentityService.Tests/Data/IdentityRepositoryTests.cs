using Microsoft.Extensions.Logging;
using Moq;

namespace BN.PROJECT.IdentityService.Tests
{
    public class IdentityRepositoryTests
    {
        private readonly IdentityDbContext _dbContext;
        private readonly Mock<ILogger<IdentityRepository>> _mockLogger;

        private readonly Guid _testGuid = Guid.Empty;

        public IdentityRepositoryTests()
        {
            _dbContext = DatabaseGenerator.SeedDataBase();
            _mockLogger = new Mock<ILogger<IdentityRepository>>();
        }

        [Fact]
        public async Task AddRoleAsync_ShouldAddRole()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "TestRole",
                Description = "TestDescription"
            };

            // Act
            await repo.AddRoleAsync(role);

            // Assert
            Assert.Equal(3, _dbContext.Roles.Count());
            Assert.Equal(1, _dbContext.Roles.Count(x => x.Name == "TestRole"));
        }

        [Fact]
        public async Task AddSessionAsync_ShouldAddSession()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);
            var session = new Session
            {
                SessionId = _testGuid,
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                LastActive = DateTime.UtcNow,
                SignedOutAt = DateTime.UtcNow
            };

            // Act
            await repo.AddSessionAsync(session);

            // Assert
            Assert.Equal(3, _dbContext.Sessions.Count());
        }

        [Fact]
        public async Task AddUserAsync_ShouldAddUser()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);
            var user = new User
            {
                UserId = _testGuid,
                Username = "TestUser",
                Email = "",
                FirstName = "Max",
                LastName = "Mustermann"
            };

            // Act
            await repo.AddUserAsync(user);

            // Assert
            Assert.Equal(3, _dbContext.Users.Count());
        }

        [Fact]
        public async Task AddUserRoleAsync_ShouldAddUserRole()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);
            var userRole = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                RoleId = _testGuid
            };

            // Act
            await repo.AddUserRoleAsync(userRole);

            // Assert
            Assert.Equal(3, _dbContext.UserRoles.Count());
            Assert.Equal(_testGuid, _dbContext.UserRoles.First(x => x.RoleId == _testGuid).RoleId);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldDeleteRole()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "TestRole"
            };
            await repo.AddRoleAsync(role);

            // Act
            await repo.DeleteRoleAsync(role.Id);

            // Assert
            Assert.Equal(2, _dbContext.Roles.Count());
            Assert.Null(_dbContext.Roles.FirstOrDefault(x => x.Id == role.Id));
        }

        [Fact]
        public async Task DeleteSessionAsync_ShouldDeleteSession()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);
            var session = new Session
            {
                SessionId = _testGuid,
                UserId = Guid.NewGuid()
            };
            await repo.AddSessionAsync(session);

            // Act
            await repo.DeleteSessionAsync(session.SessionId);

            // Assert
            Assert.Equal(2, _dbContext.Sessions.Count());
            Assert.Null(_dbContext.Sessions.FirstOrDefault(x => x.SessionId == session.SessionId));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);
            var user = await repo.GetUserByNameAsync("user1");

            // Act
            await repo.DeleteUserAsync(user.UserId);

            // Assert
            Assert.Equal(1, _dbContext.Users.Count());
        }

        [Fact]
        public async Task DeleteUserRoleAsync_ShouldDeleteUserRole()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);
            var userRole = new UserRole
            {
                UserRoleId = _testGuid,
                UserId = Guid.NewGuid(),
                RoleId = Guid.NewGuid()
            };
            await repo.AddUserRoleAsync(userRole);

            // Act
            await repo.DeleteUserRoleAsync(userRole.UserRoleId);

            // Assert
            Assert.Equal(2, _dbContext.UserRoles.Count());
            Assert.Null(_dbContext.UserRoles.FirstOrDefault(x => x.UserRoleId == userRole.UserRoleId));
        }

        [Fact]
        public async Task GetRoleByNameAsync_ShouldReturnRole()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);

            // Act
            var result = await repo.GetRoleByNameAsync("Admin");

            // Assert
            Assert.Equal("Admin", result.Name);
        }

        [Fact]
        public async Task GetSessionByUserIdAsync_ShouldReturnSession()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);
            var user = await repo.GetUserByNameAsync("user1");

            // Act
            var result = await repo.GetSessionByUserIdAsync(user.UserId);

            // Assert
            Assert.Equal(user.UserId, result.UserId);
        }

        [Fact]
        public async Task GetUserByNameAsync_ShouldReturnUser()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);

            // Act
            var result = await repo.GetUserByNameAsync("user1");

            // Assert
            Assert.Equal("Mustermann", result.LastName);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser()
        {
            // Arrange
            var repo = new IdentityRepository(_dbContext, _mockLogger.Object);

            // Act
            var result = await repo.GetUserByEmailAsync("user2@example.com");

            // Assert
            Assert.Equal("Musterfrau", result.LastName);
        }
    }
}