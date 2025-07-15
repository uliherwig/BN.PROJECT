namespace BN.PROJECT.StrategyService;

public class OptimizationResultModel
{
    public Guid StrategyId { get; set; }
    public decimal Profit { get; set; } = -10000m;
    public TestResult Result { get; set; } = new();

    public StrategySettingsModel Settings { get; set; } = new();
    public List<PositionModel> Positions { get; set; } = [];
}
