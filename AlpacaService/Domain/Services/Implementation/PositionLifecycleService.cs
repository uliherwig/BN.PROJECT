namespace BN.PROJECT.AlpacaService;

public class PositionLifecycleService : IPositionLifecycleService
{
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly ILogger<PositionLifecycleService> _logger;

    public PositionLifecycleService(
        IAlpacaTradingService alpacaTradingService,
        IAlpacaRepository alpacaRepository,
        ILogger<PositionLifecycleService> logger)
    {
        _alpacaTradingService = alpacaTradingService;
        _alpacaRepository = alpacaRepository;
        _logger = logger;
    }

    public async Task CloseExpiredPositionsAsync(TimeSpan maxHoldingPeriod, bool testMode = false)
    {
        // Open-time comes from tracking records written when a position is opened (e.g. CreateAIMarketOrder); no need to poll Alpaca.
        var trackedPositions = await _alpacaRepository.GetAllOpenPositionTrackings();
        var now = DateTime.UtcNow;

        foreach (var tracking in trackedPositions)
        {
            try
            {
                if (!testMode)
                {
                    if (now - tracking.OpenedAtUtc < maxHoldingPeriod)
                    {
                        continue;
                    }
                    await _alpacaTradingService.ClosePositionOrder(tracking.Symbol);
                }
                await _alpacaRepository.ClosePositionTrackingAsync(tracking.Symbol);
                _logger.LogInformation("Closed position {Symbol} after exceeding max holding period of {Minutes} min", tracking.Symbol, maxHoldingPeriod.TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to close expired position {Symbol}", tracking.Symbol);
            }
        }
    }

    public async Task CloseAllPositionsEndOfDayAsync()
    {
        try
        {
            await _alpacaTradingService.CloseAllPositions();
            await _alpacaRepository.CloseAllPositionTrackingsAsync();
            _logger.LogInformation("End-of-day liquidation executed: all open positions closed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute end-of-day liquidation");
        }
    }
}
