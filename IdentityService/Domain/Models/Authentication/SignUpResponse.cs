namespace BN.PROJECT.IdentityService
{
    public class SignUpResponse
    {
        public bool? Success { get; set; }
        public string? Errors { get; set; }         
        public string? UserId { get; set; }
    }
}