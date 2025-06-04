using Alpaca.Markets;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests;

public class AlpacaDataControllerTests
{
    private readonly Mock<IAlpacaRepository> _mockAlpacaRepository;
    private readonly Mock<IAlpacaTradingService> _mockTradingService;
    private readonly Mock<IAlpacaDataService> _mockDataService;
    private readonly AlpacaDataController _alpacaDataController;

    public AlpacaDataControllerTests()
    {
        _mockAlpacaRepository = new Mock<IAlpacaRepository>();
        _mockTradingService = new Mock<IAlpacaTradingService>();
        _mockDataService = new Mock<IAlpacaDataService>();
        _alpacaDataController = new AlpacaDataController(_mockDataService.Object);
    }

    [Fact]
    public async Task GetHistoricalBarsBySymbol_ShouldReturnOkWithBars()
    {
        // Arrange
        var symbol = "AAPL";
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 2);
        var mockBars = new List<AlpacaBar>
            {
                new() { T = startDate, O = 150, H = 155, L = 149, C = 152, V = 1000, Symbol = symbol },
                new() { T = endDate, O = 152, H = 156, L = 151, C = 154, V = 1200, Symbol = symbol }
            };

        _mockDataService.Setup(x => x.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute))
            .ReturnsAsync(mockBars);

        // Act
        var result = await _alpacaDataController.GetHistoricalBarsBySymbol(symbol, startDate, endDate);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(mockBars, okResult.Value);
    }

    [Fact]
    public async Task GetLatestBarBySymbol_ShouldReturnOkWithBar()
    {
        // Arrange
        var symbol = "AAPL";
        var mockBar = new AlpacaBar { T = DateTime.Now, O = 150, H = 155, L = 149, C = 152, V = 1000, Symbol = symbol };

        _mockDataService.Setup(x => x.GetLatestBarBySymbol(symbol))
            .ReturnsAsync(mockBar);

        // Act
        var result = await _alpacaDataController.GetLatestBarBySymbol(symbol);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(mockBar, okResult.Value);
    }

    [Fact]
    public async Task GetHistoricalQuotesBySymbol_ShouldReturnOkWithQuotes()
    {
        // Arrange
        var symbol = "AAPL";
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 2);
        var mockQuotes = new List<AlpacaQuote>
        {
                new() { TimestampUtc = startDate, BidPrice = 150.25m, AskPrice = 150.30m, Symbol = symbol },
                new() { TimestampUtc = endDate, BidPrice = 152.25m, AskPrice = 152.30m, Symbol = symbol }
            };

        _mockDataService.Setup(x => x.GetQuotesBySymbol(symbol, startDate, endDate))
            .ReturnsAsync(mockQuotes);

        // Act
        var result = await _alpacaDataController.GetHistoricalQuotesBySymbol(symbol, startDate, endDate);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(mockQuotes, okResult.Value);
    }

    [Fact]
    public async Task GetLatestQuoteBySymbol_ShouldReturnOkWithQuote()
    {
        // Arrange
        var symbol = "AAPL";
        var mockQuote = new AlpacaQuote { TimestampUtc = DateTime.Now, BidPrice = 150.25m, AskPrice = 150.30m, Symbol = symbol };

        _mockDataService.Setup(x => x.GetLatestQuoteBySymbol(symbol))
            .ReturnsAsync(mockQuote);

        // Act
        var result = await _alpacaDataController.GetLatestQuoteBySymbol(symbol);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(mockQuote, okResult.Value);
    }
}