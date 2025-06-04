using Microsoft.EntityFrameworkCore;

namespace BN.PROJECT.AlpacaService.Tests;

public class DatabaseGenerator
{
    public static AlpacaDbContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AlpacaDbContext>()
                .UseInMemoryDatabase(databaseName: "AlpacaDb");
        var context = new AlpacaDbContext(optionsBuilder.Options);
        return context;
    }

    public static AlpacaDbContext SeedDataBase()
    {
        var testStamp = new DateTime(2024, 10, 11, 17, 12, 1);
        var context = CreateContext();
        context.Assets.RemoveRange(context.Assets);
        context.Bars.RemoveRange(context.Bars);
        context.Quotes.RemoveRange(context.Quotes);
        context.UserSettings.RemoveRange(context.UserSettings);
        context.SaveChanges();

        var assets = new List<AlpacaAsset>
        {
            new() { Symbol = "A", Name="A", AssetId = Guid.NewGuid() },
            new() { Symbol = "B", Name="B", AssetId = Guid.NewGuid() },
            new() { Symbol = "C", Name="C", AssetId = Guid.NewGuid() },
            new() { Symbol = "D", Name="D", AssetId = Guid.NewGuid() }
        };

        context.Assets.AddRange(assets);
        context.SaveChanges();

        var bars = new List<AlpacaBar>
        {
            new() { Symbol = "A", T = testStamp, O = 1, H = 2, L = 3, C = 4, V = 5 },
            new() { Symbol = "B", T = testStamp, O = 1, H = 2, L = 3, C = 4, V = 5 },
            new() { Symbol = "C", T = testStamp, O = 1, H = 2, L = 3, C = 4, V = 5 },
        };
        context.Bars.AddRange(bars);
        context.SaveChanges();

        var quotes = new List<AlpacaQuote>
        {
            new AlpacaQuote
            {
                Symbol = "A",
                TimestampUtc = testStamp,
                BidExchange = "NYSE",
                AskExchange = "NASDAQ",
                BidPrice = 150.25m,
                AskPrice = 150.30m,
                BidSize = 100,
                AskSize = 200,
                Tape = "A"
            },
            new AlpacaQuote
            {
                Symbol = "B",
                TimestampUtc = testStamp,
                BidExchange = "NYSE",
                AskExchange = "NASDAQ",
                BidPrice = 250.50m,
                AskPrice = 250.55m,
                BidSize = 150,
                AskSize = 250,
                Tape = "B"
            },
            new AlpacaQuote
            {
                Symbol = "C",
                TimestampUtc = testStamp,
                BidExchange = "NYSE",
                AskExchange = "NASDAQ",
                BidPrice = 350.75m,
                AskPrice = 350.80m,
                BidSize = 200,
                AskSize = 300,
                Tape = "C"
            }
        };
        context.Quotes.AddRange(quotes);
        context.SaveChanges();

        //var userSettings = new List<UserSettings>
        //{
        //    new() { UserId = "1", AlpacaKey = "123", AlpacaSecret = "123"},
        //    new() { UserId = "2", AlpacaKey = "456", AlpacaSecret = "456"}

        //};
        //context.UserSettings.AddRange(userSettings);
        //context.SaveChanges();

        return context;
    }
}