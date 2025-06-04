namespace BN.PROJECT.IdentityService;

public interface IIdentityRepository
{
    Task<User> GetUserByIdAsync(Guid userId);

    Task<User> GetUserByNameAsync(string name);

    Task<User> GetUserByEmailAsync(string email);

    Task AddUserAsync(User user);

    Task UpdateUserAsync(User user);

    Task DeleteUserAsync(Guid userId);

    Task AddSessionAsync(Session session);

    Task<Session> GetSessionByUserIdAsync(Guid sessionId);

    Task UpdateSessionAsync(Session session);

    Task DeleteSessionAsync(Guid sessionId);

    Task AddUserRoleAsync(UserRole userRole);

    Task<List<UserRole>> GetUserRolesByUserIdAsync(Guid userId);

    Task UpdateUserRoleAsync(UserRole userRole);

    Task DeleteUserRoleAsync(Guid userRoleId);

    Task AddRoleAsync(Role role);

    Task<Role> GetRoleByNameAsync(string roleName);

    Task UpdateRoleAsync(Role role);

    Task DeleteRoleAsync(Guid roleId);
}