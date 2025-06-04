using Alpaca.Markets;
using Microsoft.Extensions.Logging;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests;

public class AlpacaDataServiceTests
{
    private readonly Mock<IAlpacaClient> _mockAlpacaClient;
    private readonly Mock<IAlpacaDataClient> _mockAlpacaDataClient;
    private readonly Mock<ILogger<AlpacaDataService>> _mockLogger;
    private readonly AlpacaDataService _alpacaDataService;

    public AlpacaDataServiceTests()
    {
        _mockAlpacaClient = new Mock<IAlpacaClient>();
        _mockAlpacaDataClient = new Mock<IAlpacaDataClient>();
        _mockLogger = new Mock<ILogger<AlpacaDataService>>();

        // Setup the mock to return the mocked IAlpacaDataClient
        _mockAlpacaClient.Setup(client => client.GetAlpacaDataClient())
            .Returns(_mockAlpacaDataClient.Object);

        _alpacaDataService = new AlpacaDataService(_mockAlpacaClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetHistoricalBarsBySymbol_ShouldReturnBars()
    {
        // Arrange
        var symbol = "AAPL";
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 2);
        var mockBars = new List<MockBar>
        {
            new() {Symbol = symbol, TimeUtc = startDate, Open = 150, High = 155, Low = 149, Close = 152, Volume = 1000 },
            new() {Symbol = symbol, TimeUtc = endDate, Open = 152, High = 156, Low = 151, Close = 154, Volume = 1200 }
        };

        var mockPage = new Mock<IPage<IBar>>();
        mockPage.Setup(p => p.Items).Returns(mockBars);

        _mockAlpacaDataClient.Setup(client => client.ListHistoricalBarsAsync(It.IsAny<HistoricalBarsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPage.Object);

        // Act
        var result = await _alpacaDataService.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute);

        // Assert
        Assert.Equal(mockBars.Count, result.Count);
    }

    [Fact]
    public async Task GetLatestBarBySymbol_ShouldReturnLatestBar()
    {
        // Arrange
        var symbol = "AAPL";
        var startDate = new DateTime(2023, 1, 1);
        var mockBar = new MockBar { Symbol = symbol, TimeUtc = startDate, Open = 150, High = 155, Low = 149, Close = 152, Volume = 1000 };

        _mockAlpacaDataClient.Setup(client => client.GetLatestBarAsync(It.IsAny<LatestMarketDataRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockBar);

        // Act
        var result = await _alpacaDataService.GetLatestBarBySymbol(symbol);

        // Assert
        Assert.Equal(mockBar.Open, result.O);
    }

    [Fact]
    public async Task GetTradesBySymbol_ShouldReturnTrades()
    {
        // Arrange
        var symbol = "AAPL";
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 2);

        var tradeTest = new TradeTests();
        var mockTrade = tradeTest.MockITrade(symbol);
        var mockTrades = new Dictionary<string, IReadOnlyList<ITrade>>
            {
                { symbol, new List<ITrade> { mockTrade } }
            };

        var mockPage = new Mock<IMultiPage<ITrade>>();
        mockPage.Setup(p => p.Items).Returns((IReadOnlyDictionary<string, IReadOnlyList<ITrade>>)mockTrades);

        _mockAlpacaDataClient.Setup(client => client.GetHistoricalTradesAsync(It.IsAny<HistoricalTradesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPage.Object);

        // Act
        var result = await _alpacaDataService.GetTradesBySymbol(symbol, startDate, endDate);

        // Assert
        Assert.Equal(mockTrades.Count, result.Count);
    }

    [Fact]
    public async Task GetLatestQuoteBySymbol_ShouldReturnLatestQuote()
    {
        // Arrange
        var symbol = "AAPL";
        var date = new DateTime(2023, 1, 1).ToUniversalTime();
        var mockQuote = new MockQuote { AskExchange = "AskExchange", AskPrice = 150, AskSize = 100, BidExchange = "BidExchange", BidPrice = 149, BidSize = 200, Symbol = symbol, TimestampUtc = date };

        _mockAlpacaDataClient.Setup(client => client.GetLatestQuoteAsync(It.IsAny<LatestMarketDataRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockQuote);

        // Act
        var result = await _alpacaDataService.GetLatestQuoteBySymbol(symbol);

        // Assert
        Assert.Equal(mockQuote.Symbol, result.Symbol);
    }

    [Fact]
    public async Task GetQuotesBySymbol_ShouldReturnQuotes()
    {
        // Arrange

        var symbol = "AAPL";
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 2);
        var mockQuote1 = new MockQuote { Symbol = symbol, AskExchange = "AskExchange", AskPrice = 150, AskSize = 100, BidExchange = "BidExchange", BidPrice = 149, BidSize = 200, TimestampUtc = startDate };
        var mockQuote2 = new MockQuote { Symbol = symbol, AskExchange = "AskExchange", AskPrice = 150, AskSize = 100, BidExchange = "BidExchange", BidPrice = 149, BidSize = 200, TimestampUtc = endDate };

        var mockQuotes = new Dictionary<string, IReadOnlyList<IQuote>>
            {
                { symbol, new List<IQuote> {  mockQuote1, mockQuote2  } }
            };

        var mockPage = new Mock<IMultiPage<IQuote>>();
        mockPage.Setup(p => p.Items).Returns((IReadOnlyDictionary<string, IReadOnlyList<IQuote>>)mockQuotes);

        _mockAlpacaDataClient.Setup(client => client.GetHistoricalQuotesAsync(It.IsAny<HistoricalQuotesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPage.Object);

        // Act
        var result = await _alpacaDataService.GetQuotesBySymbol(symbol, startDate, endDate);

        // Assert
        Assert.Equal(mockQuotes[symbol].Count, result.Count);
    }
}