
namespace BN.PROJECT.StrategyService;

public interface IPositionManager
{
    void ClosePosition(Guid id, DateTime stampClose,  decimal priceClose, string closeSignal);
    Position CreatePosition(Guid testId, string symbol, int quantity, Side side, decimal priceOpen, decimal stopLoss, decimal takeProfit, DateTime stampOpen);
    List<Position> GetAllPositions();
    List<Position> GetOpenPositionsBySide(Side side);
    bool UpdateTakeProfitAndStopLoss(Guid id, decimal newTakeProfit, decimal newStopLoss);
    void ClearPositions();
    Position? GetPositionById(Guid id);

}