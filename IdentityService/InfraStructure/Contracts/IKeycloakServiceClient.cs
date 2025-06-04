namespace BN.PROJECT.IdentityService
{
    public interface IKeycloakServiceClient
    {
        Task<SignInResponse> SignIn(SignInRequest signInRequest);

        Task<SignOutResponse> SignOut(SignOutRequest signOutRequest);

        Task<SignUpResponse> SignUp(SignUpRequest registerRequest);

        Task<JwtToken> RefreshToken(RefreshTokenRequest refreshTokenRequest);

        Task<User> GetUserByName(string username);

        Task<SignOutResponse> DeleteUser(string userId);

        Task<bool> UserExistsById(string userId);

        Task<bool> VerifyEmail(User user);
    }
}