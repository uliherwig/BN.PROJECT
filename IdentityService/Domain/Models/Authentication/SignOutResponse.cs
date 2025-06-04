namespace BN.PROJECT.IdentityService
{
    public class SignOutResponse
    {
        public bool? Success { get; set; } = false;
        public string? Errors { get; set; } = string.Empty;
    }
}