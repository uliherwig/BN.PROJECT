namespace BN.PROJECT.IdentityService
{
    public class SignInResponse
    {
        public bool? Success { get; set; }
        public AuthErrorCode ErrorCode { get; set; }    
        public JwtToken? JwtToken { get; set; }
    }
}