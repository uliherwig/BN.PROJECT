namespace BN.PROJECT.Core;

public static class NotificationMessageFactory
{
    public static NotificationMessage CreateNotificationMessage(
        Guid userId,
        NotificationEnum notificationType,
        string jsonData)
    {
        return new NotificationMessage
        {
            UserId = userId,
            NotificationType = notificationType,
            JsonData = jsonData,
            Timestamp = DateTime.UtcNow
        };
    }

    public static NotificationMessage CreateNotificationMessage(Guid userId, NotificationEnum notificationType)
    {
        return new NotificationMessage
        {
            UserId = userId,
            NotificationType = notificationType,
            JsonData = string.Empty,
            Timestamp = DateTime.UtcNow
        };
    }

    public static NotificationMessage CreateNotificationMessage(StrategyMessage strategyMessage)
    {

        var userId = strategyMessage.UserId;

        var notificationType = strategyMessage.StrategyTask switch
        {
            StrategyTaskEnum.Backtest => strategyMessage.MessageType switch
            {

                MessageTypeEnum.Start => NotificationEnum.BacktestStart,
                MessageTypeEnum.Stop => NotificationEnum.BacktestStop,
                _ => NotificationEnum.None
            },
            StrategyTaskEnum.Optimize => strategyMessage.MessageType switch
            {

                MessageTypeEnum.Start => NotificationEnum.OptimizeStart,
                MessageTypeEnum.Stop => NotificationEnum.OptimizeStop,
                _ => NotificationEnum.None
            },

            _ => NotificationEnum.None
        };  
        return CreateNotificationMessage(userId, notificationType);
    }
}
