namespace BN.PROJECT.AlpacaService;

public class UserSettingsModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string UserId { get; set; }

    public string AlpacaKey { get; set; }

    public string AlpacaSecret { get; set; }
}