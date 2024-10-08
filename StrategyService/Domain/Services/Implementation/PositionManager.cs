namespace BN.PROJECT.StrategyService;

public class PositionManager
{
    private readonly List<Position> _positions = [];

    public Position CreatePosition(
        Guid testId,
        string symbol,
        int quantity,
        Side side,
        decimal priceOpen,
        decimal stopLoss,
        decimal takeProfit,
        DateTime stampOpen)
    {
        var p = _positions.FirstOrDefault(p => p.Symbol == symbol && p.Side == side && p.PriceClose == 0);
        if (p != null)
        {
            return null;
        }

        var position = new Position
        {
            Id = Guid.NewGuid(),
            TestId = testId,
            Symbol = symbol,
            Quantity = quantity,
            Side = side,
            PriceOpen = priceOpen,
            TakeProfit = takeProfit,
            StopLoss = stopLoss,
            StampOpened = stampOpen.ToUniversalTime(),
            CloseSignal = "",
        };

        _positions.Add(position);
        return position;
    }

    public bool UpdateTakeProfitAndStopLoss(Guid id, decimal newTakeProfit, decimal newStopLoss)
    {
        var position = _positions.FirstOrDefault(p => p.Id == id);
        if (position == null)
        {
            return false;
        }

        position.TakeProfit = newTakeProfit;
        position.StopLoss = newStopLoss;
        position.CloseSignal = "Update ";
        return true;
    }

    public Position? GetPositionById(Guid id) => _positions.FirstOrDefault(p => p.Id == id);

    public void ClosePosition(Guid id, DateTime stampClose, decimal priceClose, string closeSignal)
    {
        var position = _positions.FirstOrDefault(p => p.Id == id);
        if (position == null)
        {
            return;
        }

        position.PriceClose = priceClose;
        position.StampClosed = stampClose.ToUniversalTime();
        position.CloseSignal = closeSignal;
        position.ProfitLoss = (priceClose - position.PriceOpen) * position.Quantity * (position.Side == Side.Buy ? 1 : -1);
        return;
    }
    public List<Position> GetOpenPositionsBySide(Side side) => _positions.Where(p => p.Side == side && p.PriceClose == 0).ToList();
    public List<Position> GetAllPositions() => _positions;
    public List<Position> GetAllClosedPositions() => _positions.Where(p => p.PriceClose > 0).ToList();
    public void ClearPositions() => _positions.Clear();
}

