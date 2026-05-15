namespace BN.PROJECT.Core;

public class BacktestMessage

{  
    public Guid StrategyId { get; set; }
    public StrategyTaskEnum StrategyTask { get; set; } = StrategyTaskEnum.Backtest;
    public StrategyEnum Strategy { get; set; } = StrategyEnum.IndicatorBased;
  
}