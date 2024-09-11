namespace BN.TRADER.IdentityService
{
    public class SignInResponse
    {
        public bool? Success { get; set; }
        public string? Errors { get; set; }
        public JwtToken? JwtToken { get; set; }      
    }
}