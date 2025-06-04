namespace BN.PROJECT.IdentityService
{
    public class SignOutRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
       
    }
}