using Alpaca.Markets;

namespace BN.PROJECT.AlpacaService.Tests
{
    public class MockQuote : IQuote
    {
        public string Symbol { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string BidExchange { get; set; }
        public string AskExchange { get; set; }
        public decimal BidPrice { get; set; }
        public decimal AskPrice { get; set; }
        public decimal BidSize { get; set; }
        public decimal AskSize { get; set; }
        public string Tape { get; set; }
        public IReadOnlyList<string> Conditions { get; set; }
    }
}