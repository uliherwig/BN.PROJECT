namespace BN.PROJECT.Core;

// This enum categorizes indicators (Skender.Stock.Indicators) into broader classes for better organization and filtering in the application.
public enum IndicatorClassEnum
{
    NONE,      // 0
    MOVING_AVERAGE, // 1 - Moving Average Indicators (SMA, EMA, WMA, TEMA)
    MOMENTUM,   // 2 - Momentum Indicators (RSI, ROC)
    VOLATILITY, // 3 - Volatility Indicators (ATR, BBANDS)
    TREND,      // 4 - Trend Indicators (MACD)
    CHANNELS,   // 5 - Channel Indicators (Donchian)
    BREAKOUT    // 6 - Breakout Strategies
}