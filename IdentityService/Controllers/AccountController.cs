namespace BN.PROJECT.IdentityService;

[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{

    private readonly IIdentityRepository _identityRepository;
    private readonly IKeycloakServiceClient _keycloakServiceClient;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly IAlpacaServiceClient _alpacaServiceClient;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AccountController(
        IIdentityRepository identityRepository,
        IStrategyServiceClient strategyServiceClient,
        IAlpacaServiceClient alpacaServiceClient,
        IKeycloakServiceClient keycloakServiceClient,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _identityRepository = identityRepository;
        _keycloakServiceClient = keycloakServiceClient;
        _strategyServiceClient = strategyServiceClient;
        _alpacaServiceClient = alpacaServiceClient;
        _emailService = emailService;
        _configuration = configuration;
    }

    [HttpGet("my-account")]
    [AuthorizeUser(["user", "admin"])]
    public async Task<IActionResult> GetMyAccount()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        var user = await _identityRepository.GetUserByIdAsync(new Guid(userId!));
        if (user != null)
        {
            return Ok(user);
        }
        return NotFound();
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest signInRequest)
    {
        var user = await _identityRepository.GetUserByNameAsync(signInRequest.Username);  
        var response = await _keycloakServiceClient.SignIn(signInRequest);

        if (user != null && user.IsEmailVerified == false)
        {
            response.Success = false;
            response.Errors = "Email not verified";
            response.JwtToken = null;
        }

        if ((bool)response.Success)
        {
            if (user != null)
            {
                var session = new Session
                {
                    SessionId = Guid.NewGuid(),
                    UserId = user.UserId,
                    CreatedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    SignedOutAt = DateTime.MinValue
                };
                await _identityRepository.AddSessionAsync(session);
            }
        }
        return Ok(response);
    }

    [HttpPost("sign-out")]
    [AuthorizeUser(["user", "admin"])]
    public async Task<IActionResult> Logout([FromBody] SignOutRequest signOutRequest)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        signOutRequest.UserId = userId!;
        var response = await _keycloakServiceClient.SignOut(signOutRequest!);
        if (response != null)
        {
            var session = await _identityRepository.GetSessionByUserIdAsync(new Guid(userId!));
            if (session != null)
            {
                session.SignedOutAt = DateTime.UtcNow;
                session.LastActive = DateTime.UtcNow;
                await _identityRepository.UpdateSessionAsync(session);
            }
            return Ok(true);
        }
        return Ok(false);
    }

    [HttpPost("refresh-token")]
 

    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var response = await _keycloakServiceClient.RefreshToken(refreshTokenRequest);
        if (!string.IsNullOrEmpty(response.RefreshToken))
        {
            var claims = JwtTokenDecoder.DecodeJwtToken(response.RefreshToken);
            var userId = new Guid(claims["sub"]);
            var session = await _identityRepository.GetSessionByUserIdAsync(userId);
            if (session != null)
            {
                session.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
                session.LastActive = DateTime.UtcNow;
                await _identityRepository.UpdateSessionAsync(session);
            }
        }

        return Ok(response);
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest signUpRequest)
    {
        var response = await _keycloakServiceClient.SignUp(signUpRequest);
        if (response != null && response.Success == true)
        {
            var user = new User
            {
                UserId = new Guid(response.UserId),
                Username = signUpRequest.Username,
                Email = signUpRequest.Email,
                FirstName = signUpRequest.FirstName,
                LastName = signUpRequest.LastName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsEmailVerified = false,

            };
            await _identityRepository.AddUserAsync(user);

            // E-Mail senden
            var subject = "Willkommen bei BN Project!";
            var body = $"Hallo {signUpRequest.FirstName},<br/><br/>" +
                       "Vielen Dank für Ihre Registrierung bei BN Project.<br/><br/>" +
                       "Bitte klicken Sie auf den folgenden Link, um Ihre E-Mail-Adresse zu bestätigen:<br/>" +
                         $"<a href='{_configuration["UI"]}/auth/confirm?token={user.UserId}'>E-Mail-Adresse bestätigen</a><br/><br/>" +
                       "Mit freundlichen Grüßen,<br/>Das BN Project Team";
            var success = await _emailService.SendEmailAsync(signUpRequest.Email, subject, body);
            if (success)
            {

                return Ok(response);
            }
            return BadRequest(new { Error = "Email not valid" });
        }

        return BadRequest(new { response.Errors });
    }

    [HttpPut("confirm")]
    public async Task<IActionResult> ConfirmSignUp([FromBody] ConfirmSignUpRequest confirmSignUp)
    {
        var userId = new Guid(confirmSignUp.Token!);
        var user = await _identityRepository.GetUserByIdAsync(userId);      

        if (user != null)
        {
            if (user.IsEmailVerified)
            {
                return BadRequest(new { Success = false, Message = "EmailVerified" });
            }
            user.IsEmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _identityRepository.UpdateUserAsync(user);
            await _keycloakServiceClient.VerifyEmail(user);
            return Ok(new { Success = true, Data = user});
        }
        return BadRequest(new { Success = false, Message = ErrorCode.NotFound });
    }

    [HttpDelete]
    [AuthorizeUser(["user", "admin"])]
    public async Task<IActionResult> Delete()
    {
        // to delete
        // 1.  postions strategies
        // 2.  sessions, userroles, user
        // 3.  usersettings, positions, orders, executions

        var userId = HttpContext.Items["UserId"]?.ToString();
        var token = HttpContext.Items["token"]?.ToString();


        await _strategyServiceClient.RemoveUserData(token!);

        await _alpacaServiceClient.DeleteExecutions(token!);
        await _alpacaServiceClient.DeleteUserSettings(token!);

        await _identityRepository.DeleteUserAsync(new Guid(userId!));

        var response = await _keycloakServiceClient.DeleteUser(userId!);

        return Ok(response);

    }

    [HttpGet("get-user")]
    [AuthorizeUser(["user", "admin"])]
    public async Task<IActionResult> GetUserByName(string userName)
    {
        var response = await _keycloakServiceClient.GetUserByName(userName);
        return Ok(new { Message = $"User {userName} Exists =  {response.FirstName} {response.LastName}" });
    }

    [AuthorizeUser("admin")]
    [HttpGet("test-auth")]
    public IActionResult TestAuthorizationAdmin()
    {
        // Only accessible to users with the "admin" role
        return Ok(new { Message = "Access granted!" });
    }


    [HttpGet("send-testmail")]
    public async Task<IActionResult> SendTestMail([FromQuery] string email)
    {
        // E-Mail senden
        var subject = "Willkommen bei BN Project!";
        var body = $"Hallo {email}," +
                   "Vielen Dank für Ihre Registrierung bei BN Project.";
        await _emailService.SendEmailAsync(email, subject, body);
        return Ok(new { Message = "Mail sent!" });
    }
}