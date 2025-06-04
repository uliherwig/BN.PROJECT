using Alpaca.Markets;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests;

public class TradeTests
{
    public ITrade MockITrade(string symbol)
    {
        // Erstellen Sie ein Mock-Objekt für das ITrade-Interface
        var mockTrade = new Mock<ITrade>();

        // Richten Sie die Eigenschaften des Mock-Objekts ein
        mockTrade.Setup(trade => trade.Symbol).Returns(symbol);
        mockTrade.Setup(trade => trade.TimestampUtc).Returns(DateTime.UtcNow);
        mockTrade.Setup(trade => trade.Price).Returns(150.25m);
        mockTrade.Setup(trade => trade.Size).Returns(100m);
        mockTrade.Setup(trade => trade.TradeId).Returns(1234567890UL);
        mockTrade.Setup(trade => trade.Exchange).Returns("NASDAQ");
        mockTrade.Setup(trade => trade.Tape).Returns("C");
        mockTrade.Setup(trade => trade.Update).Returns("UpdateReason");
        mockTrade.Setup(trade => trade.Conditions).Returns(new List<string> { "Condition1", "Condition2" });
        mockTrade.Setup(trade => trade.TakerSide).Returns(TakerSide.Buy);

        // Verwenden Sie das Mock-Objekt in Ihren Tests
        var trade = mockTrade.Object;

        return trade;
    }
}