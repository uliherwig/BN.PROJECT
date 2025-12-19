namespace BN.PROJECT.IdentityService
{
    public class SignUpResponse
    {
        public bool? Success { get; set; }
        public AuthErrorCode ErrorCode { get; set; } = AuthErrorCode.None;
        public string? UserId { get; set; } = string.Empty;
    }
}