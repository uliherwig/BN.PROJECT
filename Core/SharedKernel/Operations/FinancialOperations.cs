

using BN.PROJECT.Core;


public static class FinancialOperations
{

    public static decimal CalculateSharpeRatio(List<PositionModel> positions, DateTime startDate, DateTime endDate)
    {
        var riskFreeRate = 0.01m; // Assuming risk-free rate is 1% per year, you can adjust this as needed

        if (positions.Count == 0)
            return 0;

        // Group positions by day and calculate daily returns
        var dailyReturns = positions
            .Where(p => p.StampOpened >= startDate && p.StampClosed <= endDate)
            .GroupBy(p => p.StampClosed.Date)
            .Select(g => g.Sum(p => p.ProfitLoss))
            .ToList();

        if (dailyReturns.Count == 0)
            return 0;

        decimal averageReturn = dailyReturns.Average();
        decimal standardDeviation = (decimal)Math.Sqrt(dailyReturns.Select(r => Math.Pow((double)(r - averageReturn), 2)).Average());

        // Assuming risk-free rate is 0 for simplicity
        return standardDeviation != 0 ? (averageReturn - riskFreeRate) / standardDeviation : 0;
    }    
}