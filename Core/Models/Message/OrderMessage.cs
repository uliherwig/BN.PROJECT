namespace BN.PROJECT.Core;

public class OrderMessage
{
    public MessageTypeEnum MessageType { get; set; }
    public Guid StrategyId { get; set; }
    public Guid UserId { get; set; }
    public PositionModel? Position { get; set; }

    //public StrategySettingsModel? Settings { get; set; }

}