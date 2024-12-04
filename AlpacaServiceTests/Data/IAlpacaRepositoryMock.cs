using BN.PROJECT.AlpacaService;

namespace BN.PROJECT.AlpacaServiceTests;

public class IAlpacaRepositoryMock
{
    public static IAlpacaRepository GetMock()
    {
        List<AlpacaAsset> assets = GenerateTestData();
        AlpacaDbContext dbContextMock = DbContextMock.GetMock<AlpacaAsset, AlpacaDbContext>(assets, x => x.Assets);  
        return new AlpacaRepository(dbContextMock);
    }

    private static List<AlpacaAsset> GenerateTestData()
    {
       return new List<AlpacaAsset>
            {
                new AlpacaAsset { Symbol = "B", Name="B", AssetId = Guid.NewGuid() },
                new AlpacaAsset { Symbol = "A", Name="A", AssetId = Guid.NewGuid()  }
            };
    }
}
