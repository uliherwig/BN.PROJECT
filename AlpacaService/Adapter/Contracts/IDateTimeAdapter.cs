namespace BN.TRADER.AlpacaService
{
    public interface IDateTimeAdapter
    {
        DateTimeOffset Now();

        DateTimeOffset UtcNow();
    }
}