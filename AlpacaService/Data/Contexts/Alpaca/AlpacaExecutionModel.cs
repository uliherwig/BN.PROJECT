namespace BN.PROJECT.AlpacaService;

public class AlpacaExecutionModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid StrategyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

}
