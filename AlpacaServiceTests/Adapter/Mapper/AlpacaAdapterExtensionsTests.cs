using Alpaca.Markets;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests;

public class AlpacaAdapterExtensionsTests
{
    [Fact]
    public void ToAlpacaBar_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var mockBar = new Mock<IBar>();
        mockBar.SetupGet(b => b.Close).Returns(100);
        mockBar.SetupGet(b => b.High).Returns(110);
        mockBar.SetupGet(b => b.Low).Returns(90);
        mockBar.SetupGet(b => b.TradeCount).Returns(1000);
        mockBar.SetupGet(b => b.Open).Returns(95);
        mockBar.SetupGet(b => b.TimeUtc).Returns(DateTime.UtcNow);
        mockBar.SetupGet(b => b.Volume).Returns(5000);
        mockBar.SetupGet(b => b.Vwap).Returns(98);

        var symbol = "AAPL";

        // Act
        var result = mockBar.Object.ToAlpacaBar(symbol);

        // Assert
        Assert.Equal(symbol, result.Symbol);
        Assert.Equal(mockBar.Object.Close, result.C);
        Assert.Equal(mockBar.Object.High, result.H);
        Assert.Equal(mockBar.Object.Low, result.L);
        Assert.Equal(mockBar.Object.TradeCount, result.N);
        Assert.Equal(mockBar.Object.Open, result.O);
        Assert.Equal(mockBar.Object.TimeUtc, result.T);
        Assert.Equal(mockBar.Object.Volume, result.V);
        Assert.Equal(mockBar.Object.Vwap, result.Vw);
    }

    [Fact]
    public void ToAlpacaAsset_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var mockAsset = new Mock<IAsset>();
        mockAsset.SetupGet(a => a.AssetId).Returns(Guid.NewGuid());
        mockAsset.SetupGet(a => a.Name).Returns("Asset Name");
        mockAsset.SetupGet(a => a.Symbol).Returns("AAPL");

        // Act
        var result = mockAsset.Object.ToAlpacaAsset();

        // Assert
        Assert.Equal(mockAsset.Object.AssetId, result.AssetId);
        Assert.Equal(mockAsset.Object.Name, result.Name);
        Assert.Equal(mockAsset.Object.Symbol, result.Symbol);
    }

    [Fact]
    public void ToAlpacaOrder_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var mockOrder = new Mock<IOrder>();
        mockOrder.SetupGet(o => o.OrderId).Returns(Guid.NewGuid());
        mockOrder.SetupGet(o => o.ClientOrderId).Returns("client-order-id");
        mockOrder.SetupGet(o => o.CreatedAtUtc).Returns(DateTime.UtcNow);
        mockOrder.SetupGet(o => o.UpdatedAtUtc).Returns(DateTime.UtcNow);
        mockOrder.SetupGet(o => o.SubmittedAtUtc).Returns(DateTime.UtcNow);
        mockOrder.SetupGet(o => o.FilledAtUtc).Returns(DateTime.UtcNow);
        mockOrder.SetupGet(o => o.ExpiredAtUtc).Returns(DateTime.UtcNow);
        mockOrder.SetupGet(o => o.CancelledAtUtc).Returns(DateTime.UtcNow);
        mockOrder.SetupGet(o => o.FailedAtUtc).Returns(DateTime.UtcNow);
        mockOrder.SetupGet(o => o.ReplacedAtUtc).Returns(DateTime.UtcNow);
        mockOrder.SetupGet(o => o.AssetId).Returns(Guid.NewGuid());
        mockOrder.SetupGet(o => o.Symbol).Returns("AAPL");
        mockOrder.SetupGet(o => o.Notional).Returns(1000);
        mockOrder.SetupGet(o => o.Quantity).Returns(10);
        mockOrder.SetupGet(o => o.FilledQuantity).Returns(5);
        mockOrder.SetupGet(o => o.IntegerQuantity).Returns(10);
        mockOrder.SetupGet(o => o.IntegerFilledQuantity).Returns(5);
        mockOrder.SetupGet(o => o.OrderType).Returns(OrderType.Stop);
        mockOrder.SetupGet(o => o.OrderClass).Returns(OrderClass.Simple);
        mockOrder.SetupGet(o => o.OrderSide).Returns(OrderSide.Buy);
        mockOrder.SetupGet(o => o.TimeInForce).Returns(TimeInForce.Day);
        mockOrder.SetupGet(o => o.LimitPrice).Returns(100);
        mockOrder.SetupGet(o => o.StopPrice).Returns(90);
        mockOrder.SetupGet(o => o.TrailOffsetInDollars).Returns(1);
        mockOrder.SetupGet(o => o.TrailOffsetInPercent).Returns(0.01m);
        mockOrder.SetupGet(o => o.HighWaterMark).Returns(110);
        mockOrder.SetupGet(o => o.AverageFillPrice).Returns(95);
        mockOrder.SetupGet(o => o.OrderStatus).Returns(OrderStatus.Accepted);
        mockOrder.SetupGet(o => o.ReplacedByOrderId).Returns(Guid.NewGuid());
        mockOrder.SetupGet(o => o.ReplacesOrderId).Returns(Guid.NewGuid());

        // Act
        var result = mockOrder.Object.ToAlpacaOrder();

        // Assert
        Assert.Equal(mockOrder.Object.OrderId, result.OrderId);
        Assert.Equal(mockOrder.Object.ClientOrderId, result.ClientOrderId);
        Assert.Equal(mockOrder.Object.CreatedAtUtc, result.CreatedAtUtc);
        Assert.Equal(mockOrder.Object.UpdatedAtUtc, result.UpdatedAtUtc);
        Assert.Equal(mockOrder.Object.SubmittedAtUtc, result.SubmittedAtUtc);
        Assert.Equal(mockOrder.Object.FilledAtUtc, result.FilledAtUtc);
        Assert.Equal(mockOrder.Object.ExpiredAtUtc, result.ExpiredAtUtc);
        Assert.Equal(mockOrder.Object.CancelledAtUtc, result.CancelledAtUtc);
        Assert.Equal(mockOrder.Object.FailedAtUtc, result.FailedAtUtc);
        Assert.Equal(mockOrder.Object.ReplacedAtUtc, result.ReplacedAtUtc);
        Assert.Equal(mockOrder.Object.AssetId, result.AssetId);
        Assert.Equal(mockOrder.Object.Symbol, result.Symbol);
        Assert.Equal(mockOrder.Object.Notional, result.Notional);
        Assert.Equal(mockOrder.Object.Quantity, result.Quantity);
        Assert.Equal(mockOrder.Object.FilledQuantity, result.FilledQuantity);
        Assert.Equal(mockOrder.Object.IntegerQuantity, result.IntegerQuantity);
        Assert.Equal(mockOrder.Object.IntegerFilledQuantity, result.IntegerFilledQuantity);
        Assert.Equal(mockOrder.Object.OrderType, result.OrderType);
        Assert.Equal(mockOrder.Object.OrderClass, result.OrderClass);
        Assert.Equal(mockOrder.Object.OrderSide, result.OrderSide);
        Assert.Equal(mockOrder.Object.TimeInForce, result.TimeInForce);
        Assert.Equal(mockOrder.Object.LimitPrice, result.LimitPrice);
        Assert.Equal(mockOrder.Object.StopPrice, result.StopPrice);
        Assert.Equal(mockOrder.Object.TrailOffsetInDollars, result.TrailOffsetInDollars);
        Assert.Equal(mockOrder.Object.TrailOffsetInPercent, result.TrailOffsetInPercent);
        Assert.Equal(mockOrder.Object.HighWaterMark, result.HighWaterMark);
        Assert.Equal(mockOrder.Object.AverageFillPrice, result.AverageFillPrice);
        Assert.Equal(mockOrder.Object.OrderStatus, result.OrderStatus);
        Assert.Equal(mockOrder.Object.ReplacedByOrderId, result.ReplacedByOrderId);
        Assert.Equal(mockOrder.Object.ReplacesOrderId, result.ReplacesOrderId);
    }

    [Fact]
    public void ToAlpacaPosition_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var mockPosition = new Mock<IPosition>();
        mockPosition.SetupGet(p => p.AssetId).Returns(Guid.NewGuid());
        mockPosition.SetupGet(p => p.Symbol).Returns("AAPL");
        mockPosition.SetupGet(p => p.Exchange).Returns(Exchange.Nyse);
        mockPosition.SetupGet(p => p.AssetClass).Returns(AssetClass.UsEquity);
        mockPosition.SetupGet(p => p.AverageEntryPrice).Returns(100);
        mockPosition.SetupGet(p => p.Quantity).Returns(10);
        mockPosition.SetupGet(p => p.IntegerQuantity).Returns(10);
        mockPosition.SetupGet(p => p.AvailableQuantity).Returns(5);
        mockPosition.SetupGet(p => p.IntegerAvailableQuantity).Returns(5);
        mockPosition.SetupGet(p => p.Side).Returns(PositionSide.Long);
        mockPosition.SetupGet(p => p.MarketValue).Returns(1000);
        mockPosition.SetupGet(p => p.CostBasis).Returns(950);
        mockPosition.SetupGet(p => p.UnrealizedProfitLoss).Returns(50);
        mockPosition.SetupGet(p => p.UnrealizedProfitLossPercent).Returns(0.05m);
        mockPosition.SetupGet(p => p.IntradayUnrealizedProfitLoss).Returns(10);
        mockPosition.SetupGet(p => p.IntradayUnrealizedProfitLossPercent).Returns(0.01m);
        mockPosition.SetupGet(p => p.AssetCurrentPrice).Returns(105);
        mockPosition.SetupGet(p => p.AssetLastPrice).Returns(104);
        mockPosition.SetupGet(p => p.AssetChangePercent).Returns(0.01m);

        // Act
        var result = mockPosition.Object.ToAlpacaPosition();

        // Assert
        Assert.Equal(mockPosition.Object.AssetId, result.AssetId);
        Assert.Equal(mockPosition.Object.Symbol, result.Symbol);
        Assert.Equal(mockPosition.Object.Exchange, result.Exchange);
        Assert.Equal(mockPosition.Object.AssetClass, result.AssetClass);
        Assert.Equal(mockPosition.Object.AverageEntryPrice, result.AverageEntryPrice);
        Assert.Equal(mockPosition.Object.Quantity, result.Quantity);
        Assert.Equal(mockPosition.Object.IntegerQuantity, result.IntegerQuantity);
        Assert.Equal(mockPosition.Object.AvailableQuantity, result.AvailableQuantity);
        Assert.Equal(mockPosition.Object.IntegerAvailableQuantity, result.IntegerAvailableQuantity);
        Assert.Equal(mockPosition.Object.Side, result.Side);
        Assert.Equal(mockPosition.Object.MarketValue, result.MarketValue);
        Assert.Equal(mockPosition.Object.CostBasis, result.CostBasis);
        Assert.Equal(mockPosition.Object.UnrealizedProfitLoss, result.UnrealizedProfitLoss);
        Assert.Equal(mockPosition.Object.UnrealizedProfitLossPercent, result.UnrealizedProfitLossPercent);
        Assert.Equal(mockPosition.Object.IntradayUnrealizedProfitLoss, result.IntradayUnrealizedProfitLoss);
        Assert.Equal(mockPosition.Object.IntradayUnrealizedProfitLossPercent, result.IntradayUnrealizedProfitLossPercent);
        Assert.Equal(mockPosition.Object.AssetCurrentPrice, result.AssetCurrentPrice);
        Assert.Equal(mockPosition.Object.AssetLastPrice, result.AssetLastPrice);
        Assert.Equal(mockPosition.Object.AssetChangePercent, result.AssetChangePercent);
    }

    [Fact]
    public void ToAlpacaQuote_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var mockQuote = new Mock<IQuote>();
        mockQuote.SetupGet(q => q.Symbol).Returns("AAPL");
        mockQuote.SetupGet(q => q.AskPrice).Returns(150);
        mockQuote.SetupGet(q => q.AskSize).Returns(100);
        mockQuote.SetupGet(q => q.BidPrice).Returns(149);
        mockQuote.SetupGet(q => q.BidSize).Returns(200);
        mockQuote.SetupGet(q => q.TimestampUtc).Returns(DateTime.UtcNow);
        mockQuote.SetupGet(q => q.BidExchange).Returns("NASDAQ");
        mockQuote.SetupGet(q => q.AskExchange).Returns("NYSE");
        mockQuote.SetupGet(q => q.Tape).Returns("A");

        // Act
        var result = mockQuote.Object.ToAlpacaQuote();

        // Assert
        Assert.Equal(mockQuote.Object.Symbol, result.Symbol);
        Assert.Equal(mockQuote.Object.AskPrice, result.AskPrice);
        Assert.Equal(mockQuote.Object.AskSize, result.AskSize);
        Assert.Equal(mockQuote.Object.BidPrice, result.BidPrice);
        Assert.Equal(mockQuote.Object.BidSize, result.BidSize);
        Assert.Equal(mockQuote.Object.TimestampUtc, result.TimestampUtc);
        Assert.Equal(mockQuote.Object.BidExchange, result.BidExchange);
        Assert.Equal(mockQuote.Object.AskExchange, result.AskExchange);
        Assert.Equal(mockQuote.Object.Tape, result.Tape);
    }
}