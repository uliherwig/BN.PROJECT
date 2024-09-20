namespace BN.PROJECT.AlpacaService;

public class UserSettings
{
    [Key]
    public Guid UserId { get; set; }

    public string Symbols { get; set; }

    public string AlpacaKey { get; set; }

    public string AlpacaSecret { get; set; }

  


}
