
namespace BN.PROJECT.IdentityService
{
    public interface IKeycloakServiceClient
    {
        Task<SignInResponse> SignIn(SignInRequest signInRequest);
        Task<SignOutResponse> SignOut(SignOutRequest signOutRequest);
        Task<SignUpResponse> SignUp(SignUpRequest registerRequest);
        Task<JwtToken> RefreshToken(RefreshTokenRequest refreshTokenRequest);
        Task<User> GetUserByName(string username);
    }
}