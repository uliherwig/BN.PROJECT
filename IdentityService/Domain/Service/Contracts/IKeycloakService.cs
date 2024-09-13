namespace BN.PROJECT.IdentityService
{
    public interface IKeycloakService
    {
        Task<SignInResponse> SignIn([FromBody] SignInRequest signInRequest);

        Task<SignOutResponse> SignOut(SignOutRequest signOutRequest);

        Task<SignInResponse> SignUp(SignUpRequest registerRequest);
    }
}
