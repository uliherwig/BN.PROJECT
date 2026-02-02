namespace BN.PROJECT.StrategyService;

public static class TradingOperations
{
    public static OrderMessage CreateOrderMessage(Guid strategyId, Guid userId, PositionModel position)
    {
        return new OrderMessage
        {
            MessageType = MessageTypeEnum.Order,
            StrategyId = strategyId,
            UserId = userId,
            Position = position
        };
    }

}
