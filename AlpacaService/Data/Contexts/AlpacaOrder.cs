namespace BN.TRADER.AlpacaService
{
    public class AlpacaOrder
    {
        [Key]
        private Guid OrderId { get; }

        private String? ClientOrderId { get; }

        private DateTime? CreatedAtUtc { get; }

        private DateTime? UpdatedAtUtc { get; }

        private DateTime? SubmittedAtUtc { get; }

        private DateTime? FilledAtUtc { get; }

        private DateTime? ExpiredAtUtc { get; }

        private DateTime? CancelledAtUtc { get; }

        private DateTime? FailedAtUtc { get; }

        private DateTime? ReplacedAtUtc { get; }

        private Guid AssetId { get; }

        private String Symbol { get; }

        private Decimal? Notional { get; }

        private Decimal? Quantity { get; }

        private Decimal FilledQuantity { get; }

        private Int64 IntegerQuantity { get; }

        private Int64 IntegerFilledQuantity { get; }

        private OrderType OrderType { get; }

        private OrderClass OrderClass { get; }

        private OrderSide OrderSide { get; }

        private TimeInForce TimeInForce { get; }

        private Decimal? LimitPrice { get; }

        private Decimal? StopPrice { get; }

        public Decimal? TrailOffsetInDollars { get; }

        public Decimal? TrailOffsetInPercent { get; }

        public Decimal? HighWaterMark { get; }

        private Decimal? AverageFillPrice { get; }

        private OrderStatus OrderStatus { get; }

        private Guid? ReplacedByOrderId { get; }

        private Guid? ReplacesOrderId { get; }

        private IReadOnlyList<IOrder> Legs { get; }
    }
}