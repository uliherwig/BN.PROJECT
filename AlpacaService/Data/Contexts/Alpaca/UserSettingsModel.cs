namespace BN.PROJECT.AlpacaService;

public class UserSettingsModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string UserId { get; set; } = string.Empty;

    public string AlpacaKey { get; set; } = string.Empty;

    public string AlpacaSecret { get; set; } = string.Empty;
}