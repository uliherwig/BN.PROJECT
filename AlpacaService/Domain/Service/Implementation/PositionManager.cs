namespace BN.PROJECT.AlpacaService;

public class PositionManager
{
    private readonly List<Position> _positions = [];

    public Position CreatePosition(
        string symbol,
        int quantity,
        Side side,
        decimal priceOpen,
        decimal stopLoss,
        decimal takeProfit)
    {
        var p = _positions.FirstOrDefault(p => p.Symbol == symbol && p.Side == side && p.PriceClose == 0);
        if (p != null)
        {
            return null;
        }

        var position = new Position
        {
            Id = Guid.NewGuid(),
            Symbol = symbol,
            Quantity = quantity,
            Side = side,
            PriceOpen = priceOpen,
            TakeProfit = takeProfit,
            StopLoss = stopLoss,
            StampOpened = DateTime.UtcNow
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
        return true;
    }

    public bool ClosePosition(Guid id, decimal priceClose, string closeSignal)
    {
        var position = _positions.FirstOrDefault(p => p.Id == id);
        if (position == null)
        {
            return false;
        }

        position.PriceClose = priceClose;
        position.StampClosed = DateTime.UtcNow;
        position.CloseSignal = closeSignal;
        position.ProfitLoss = (priceClose - position.PriceOpen) * position.Quantity * (position.Side == Side.Buy ? 1 : -1);
        return true;
    }

    public List<Position> GetOpenPositionsBySide(Side side)
    {
        return _positions.Where(p => p.Side == side && p.PriceClose == 0).ToList();
    }

    public List<Position> GetAllPositions()
    {
        return _positions;
    }
}
