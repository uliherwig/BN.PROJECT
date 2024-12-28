using BN.PROJECT.AlpacaService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BN.PROJECT.AlpacaServiceTests
{
    public class DatabaseGenerator
    {
        public static AlpacaDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AlpacaDbContext>()
                    .UseInMemoryDatabase(databaseName: "AlpacaDb");
            var context = new AlpacaDbContext(optionsBuilder.Options);
            return context;
        }

        public static void SeedDataBase()
        {
            var context = CreateContext();
            var assets = new List<AlpacaAsset>
            {
                new() { Symbol = "A", Name="A", AssetId = Guid.NewGuid() },
                new() { Symbol = "B", Name="B", AssetId = Guid.NewGuid() },
                new() { Symbol = "C", Name="C", AssetId = Guid.NewGuid() },
                new() { Symbol = "D", Name="D", AssetId = Guid.NewGuid() }
            };

            context.Assets.AddRange(assets);
            context.SaveChanges();

            var userSettings = new List<UserSettings> 
            {
                new() { UserId = "123", AlpacaKey = "123", AlpacaSecret = "123"},
                new() { UserId = "456", AlpacaKey = "456", AlpacaSecret = "456"}
            
            };
            context.UserSettings.AddRange(userSettings);
            context.SaveChanges();
        }
    }
}
