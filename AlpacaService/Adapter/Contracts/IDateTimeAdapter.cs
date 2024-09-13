namespace BN.PROJECT.AlpacaService
{
    public interface IDateTimeAdapter
    {
        DateTimeOffset Now();

        DateTimeOffset UtcNow();
    }
}