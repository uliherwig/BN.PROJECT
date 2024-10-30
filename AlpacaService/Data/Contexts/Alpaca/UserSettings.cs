namespace BN.PROJECT.AlpacaService;

public class UserSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required string UserId { get; set; }

    public required string AlpacaKey { get; set; }

    public required string AlpacaSecret { get; set; }

}
