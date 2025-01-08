using Alpaca.Markets;

namespace BN.PROJECT.AlpacaService.Tests
{
    public class MockBar : IBar
    {
        public string Symbol { get; set; }
        public DateTime TimeUtc { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
        public decimal Vwap { get; set; }
        public ulong TradeCount { get; set; }
    }
}