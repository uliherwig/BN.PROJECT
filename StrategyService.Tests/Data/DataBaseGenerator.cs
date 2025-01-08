using Microsoft.EntityFrameworkCore;
using BN.PROJECT.Core;
using NuGet.Configuration;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.AspNetCore.SignalR;


namespace BN.PROJECT.StrategyService.Tests;

public static class DataBaseGenerator
{
    public static Guid TestGuid; 
    public static StrategyDbContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<StrategyDbContext>()
                .UseInMemoryDatabase(databaseName: "IdentityDb");
        var context = new StrategyDbContext(optionsBuilder.Options);
        return context;
    }



    public static StrategyDbContext SeedDataBase()
    {
        TestGuid = Guid.NewGuid();
        var context = CreateContext();

        // delete all existing data
        context.Strategies.RemoveRange(context.Strategies);
        context.Positions.RemoveRange(context.Positions);
        context.SaveChanges();

      

        // Erstellen von Beispiel-Daten
        var strategy1 = new StrategySettingsModel
        {
            Name = "Strategy 1",
            Broker = "Alpaca",
            Quantity = 10,
            StartDate = new DateTime(2023, 1, 1),
            EndDate = new DateTime(2023, 1, 2),
            Asset = "AAPL",
            StrategyType = StrategyEnum.Breakout,
            UserId = TestGuid,
            Id = TestGuid
        };

        var strategy2 = new StrategySettingsModel
        {
            Name = "Strategy 2",
            Broker = "Alpaca",
            Quantity = 10,
            StartDate = new DateTime(2023, 1, 1),
            EndDate = new DateTime(2023, 1, 2),
            Asset = "AAPL",
            StrategyType = StrategyEnum.Breakout,
            UserId = TestGuid,
            Id = Guid.NewGuid()
        };

        context.Strategies.Add(strategy1);
        context.Strategies.Add(strategy2);
        context.SaveChanges();

        var position1 = new Position
        {
            Id = TestGuid,
            StrategyId = TestGuid,
            Symbol = "AAPL",
            Quantity = 1,
            Side = SideEnum.Buy,
            PriceOpen = 100,
            PriceClose = 110,
            ProfitLoss = 10,
            StampClosed = new DateTime(2023, 1, 2),
            TakeProfit = 110,
            StopLoss = 90,
            StampOpened = new DateTime(2023, 1, 2),
            CloseSignal = "",

        };
        var position2 = new Position
        {
            Id = Guid.NewGuid(),
            StrategyId = Guid.NewGuid(),
            Symbol = "AAPL",
            Quantity = 1,
            Side = SideEnum.Sell,
            PriceOpen = 100,
            PriceClose = 110,
            ProfitLoss = 10,
            StampClosed = new DateTime(2023, 1, 2),
            TakeProfit = 110,
            StopLoss = 90,
            StampOpened = new DateTime(2023, 1, 2),
            CloseSignal = "",

        };
        context.Positions.Add(position1);
        context.Positions.Add(position2);
        context.SaveChanges();

        return context;
    }
}
