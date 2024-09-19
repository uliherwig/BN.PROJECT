
namespace BN.PROJECT.IdentityService;
public class IdentityRepository : IIdentityRepository
{

    private readonly IdentityDbContext _context;
    private readonly ILogger<IdentityRepository> _logger;

    public IdentityRepository(IdentityDbContext context, ILogger<IdentityRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddRoleAsync(Role role)
    {
        try
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding role");
        }
    }

    public async Task AddSessionAsync(Session session)
    {
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task AddUserRoleAsync(UserRole userRole)
    {
        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(Guid roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role != null)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

    }

    public async Task DeleteSessionAsync(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteUserRoleAsync(Guid userRoleId)
    {
        var userRole = await _context.UserRoles.FindAsync(userRoleId);
        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Role> GetRoleByNameAsync(string roleName)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        return role;
    }

    public async Task<Session> GetSessionByUserIdAsync(Guid userId)
    {
        var userSession = await _context.Sessions
           .Where(s => s.UserId == userId)
           .OrderByDescending(s => s.CreatedAt)
           .FirstOrDefaultAsync();
        return userSession;
    }

    public async Task<User> GetUserByNameAsync(string name)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == name);
        return user;
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user;
    }

    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<List<UserRole>> GetUserRolesByUserIdAsync(Guid userId)
    {
        return await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
    }

    public async Task UpdateRoleAsync(Role role)
    {
        var roleToUpdate = await _context.Roles.FindAsync(role.Name);
        if (roleToUpdate != null)
        {
            roleToUpdate.Name = role.Name;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateSessionAsync(Session session)
    {
        var sessionToUpdate = await _context.Sessions.OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync(s => s.UserId == session.UserId);
        if (sessionToUpdate != null)
        {
            sessionToUpdate.ExpiresAt = session.ExpiresAt;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        var userToUpdate = await _context.Users.FindAsync(user.UserId);
        if (userToUpdate != null)
        {
            userToUpdate.Email = user.Email;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateUserRoleAsync(UserRole userRole)
    {
        var userRoleToUpdate = await _context.UserRoles.FindAsync(userRole.UserRoleId);
        if (userRoleToUpdate != null)
        {
            userRoleToUpdate.RoleId = userRole.RoleId;
            await _context.SaveChangesAsync();
        }
    }
}
