namespace BN.PROJECT.StrategyService;

public record BarSignalModel(
    long index, DateTime Timestamp, decimal Open, decimal High, decimal Low, decimal Close, int Signal);