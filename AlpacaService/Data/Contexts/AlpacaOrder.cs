namespace BN.TRADER.AlpacaService
{
    public class AlpacaOrder
    {
        [Key]
        public Guid OrderId {get; set; }

        public String? ClientOrderId {get; set; }

        public DateTime? CreatedAtUtc {get; set; }

        public DateTime? UpdatedAtUtc {get; set; }

        public DateTime? SubmittedAtUtc {get; set; }

        public DateTime? FilledAtUtc {get; set; }

        public DateTime? ExpiredAtUtc {get; set; }

        public DateTime? CancelledAtUtc {get; set; }

        public DateTime? FailedAtUtc {get; set; }

        public DateTime? ReplacedAtUtc {get; set; }

        public Guid AssetId {get; set; }

        public String Symbol {get; set; }

        public Decimal? Notional {get; set; }

        public Decimal? Quantity {get; set; }

        public Decimal FilledQuantity {get; set; }

        public Int64 IntegerQuantity {get; set; }

        public Int64 IntegerFilledQuantity {get; set; }

        public OrderType OrderType {get; set; }

        public OrderClass OrderClass {get; set; }

        public OrderSide OrderSide {get; set; }

        public TimeInForce TimeInForce {get; set; }

        public Decimal? LimitPrice {get; set; }

        public Decimal? StopPrice {get; set; }

        public Decimal? TrailOffsetInDollars {get; set; }

        public Decimal? TrailOffsetInPercent {get; set; }

        public Decimal? HighWaterMark {get; set; }

        public Decimal? AverageFillPrice {get; set; }

        public OrderStatus OrderStatus {get; set; }

        public Guid? ReplacedByOrderId {get; set; }

        public Guid? ReplacesOrderId {get; set; }

    }
}