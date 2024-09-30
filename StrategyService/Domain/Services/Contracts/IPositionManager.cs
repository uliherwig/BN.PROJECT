
namespace BN.PROJECT.StrategyService
{
    public interface IPositionManager
    {
        bool ClosePosition(Guid id, decimal priceClose, string closeSignal);
        Position CreatePosition(string symbol, int quantity, Side side, decimal priceOpen, decimal stopLoss, decimal takeProfit);
        List<Position> GetAllPositions();
        List<Position> GetOpenPositionsBySide(Side side);
        bool UpdateTakeProfitAndStopLoss(Guid id, decimal newTakeProfit, decimal newStopLoss);
    }
}