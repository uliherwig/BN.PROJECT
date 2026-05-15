namespace BN.PROJECT.Core;

public enum IndicatorEnum
{
    NONE,      // 0

    // Price trends
    MACD,      // 5 - Moving Average Convergence/Divergence

    // Price channels
    DONCHIAN,  // 7 - Donchian Channel
    BBANDS,    // 11 - Bollinger Bands
    // Oscillators

    RSI,       // 6 - Relative Strength Index


    // Stop and reverse

    // Candlestick patterns


    // Volume-based indicators


    // Moving Averages

    SMA,       // 1 - Simple Moving Average
    EMA,       // 2 - Exponential Moving Average
    WMA,       // 3 - Weighted Moving Average
    TEMA,      // 4 - Triple Exponential Moving Average
    ALMA,      // 5 - Arnaud Legoux Moving Average
    DEMA,      // 6 - Double Exponential Moving Average


    // Price transforms


    // Price characteristics
    ATR,       // 10 - Average True Range
    ROC,       // 12 - Rate of Change Indicator

    // Numerical analysis

    VOLA,      // 9 - Volatility Strategy



    // deprecated indicators
    BREAKOUT,  // 8 - Breakout Strategy
    
    
    
    
}