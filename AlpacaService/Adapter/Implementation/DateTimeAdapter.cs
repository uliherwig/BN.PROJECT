namespace BN.TRADER.AlpacaService
{
    public class DateTimeAdapter : IDateTimeAdapter
    {
        public DateTimeOffset Now()
        {
            return DateTimeOffset.Now;
        }

        public DateTimeOffset UtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}