namespace BN.PROJECT.AlpacaService;

public class AlpacaExecutionModel
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; } = Guid.Empty;
    public Guid StrategyId { get; set; }
    public StrategyEnum StrategyType { get; set; }
    public string Assets { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

}
