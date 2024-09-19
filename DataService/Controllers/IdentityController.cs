namespace BN.PROJECT.DataService.Controllers;

[ApiController]
[Route("[controller]")]
public class IdentityController : ControllerBase
{
    private readonly ILogger<IdentityController> _logger;
    private readonly IIdentityRepository _dbRepository;

    public IdentityController(ILogger<IdentityController> logger, IIdentityRepository dbRepository)
    {
        _logger = logger;
        _dbRepository = dbRepository;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<User>> GetUserByIdAsync(Guid userId)
    {
        var user = await _dbRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }


    [HttpGet("user/email/{email}")]
    public async Task<ActionResult<User>> GetUserByEmailAsync(string email)
    {
        var user = await _dbRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost("user")]
    public async Task<ActionResult> AddUserAsync(User user)
    {
        await _dbRepository.AddUserAsync(user);
        return CreatedAtAction(nameof(GetUserByIdAsync), new { userId = user.UserId }, user);
    }

    [HttpPut("user/{userId}")]
    public async Task<ActionResult> UpdateUserAsync(Guid userId, User user)
    {
        if (userId != user.UserId)
        {
            return BadRequest();
        }

        await _dbRepository.UpdateUserAsync(user);
        return NoContent();
    }

    [HttpDelete("user/{userId}")]
    public async Task<ActionResult> DeleteUserAsync(Guid userId)
    {
        await _dbRepository.DeleteUserAsync(userId);
        return NoContent();
    }

    [HttpPost("session")]
    public async Task<ActionResult> AddSessionAsync(Session session)
    {
        await _dbRepository.AddSessionAsync(session);
        return CreatedAtAction(nameof(GetSessionByUserIdAsync), new { sessionId = session.SessionId }, session);
    }

    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<Session>> GetSessionByUserIdAsync(Guid sessionId)
    {
        var session = await _dbRepository.GetSessionByUserIdAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }
        return Ok(session);
    }

    [HttpPut("session/{sessionId}")]
    public async Task<ActionResult> UpdateSessionAsync(Guid sessionId, Session session)
    {
        if (sessionId != session.SessionId)
        {
            return BadRequest();
        }

        await _dbRepository.UpdateSessionAsync(session);
        return NoContent();
    }

    [HttpDelete("session/{sessionId}")]
    public async Task<ActionResult> DeleteSessionAsync(Guid sessionId)
    {
        await _dbRepository.DeleteSessionAsync(sessionId);
        return NoContent();
    }

    [HttpPost("userRole")]
    public async Task<ActionResult> AddUserRoleAsync(UserRole userRole)
    {
        await _dbRepository.AddUserRoleAsync(userRole);
        return CreatedAtAction(nameof(GetUserRolesByUserIdAsync), new { userId = userRole.UserId }, userRole);
    }

    [HttpGet("userRoles/{userId}")]
    public async Task<ActionResult<List<UserRole>>> GetUserRolesByUserIdAsync(Guid userId)
    {
        var userRoles = await _dbRepository.GetUserRolesByUserIdAsync(userId);
        return Ok(userRoles);
    }

    [HttpPut("userRole/{userRoleId}")]
    public async Task<ActionResult> UpdateUserRoleAsync(Guid userRoleId, UserRole userRole)
    {
        if (userRoleId != userRole.UserRoleId)
        {
            return BadRequest();
        }

        await _dbRepository.UpdateUserRoleAsync(userRole);
        return NoContent();
    }

    [HttpDelete("userRole/{userRoleId}")]
    public async Task<ActionResult> DeleteUserRoleAsync(Guid userRoleId)
    {
        await _dbRepository.DeleteUserRoleAsync(userRoleId);
        return NoContent();
    }

    [HttpPost("role")]
    public async Task<ActionResult> AddRoleAsync(Role role)
    {
        await _dbRepository.AddRoleAsync(role);
        return Ok(role);

    }

    [HttpGet("role/{roleName}")]
    public async Task<ActionResult<Role>> GetRoleByNameAsync(string roleName)
    {
        var role = await _dbRepository.GetRoleByNameAsync(roleName);
        if (role == null)
        {
            return NotFound();
        }
        return Ok(role);
    }

    [HttpPut("role/{roleId}")]
    public async Task<ActionResult> UpdateRoleAsync(Role role)
    {        
        await _dbRepository.UpdateRoleAsync(role);
        return NoContent();
    }

    [HttpDelete("role/{roleId}")]
    public async Task<ActionResult> DeleteRoleAsync(Guid roleId)
    {
        await _dbRepository.DeleteRoleAsync(roleId);
        return NoContent();
    }
}

