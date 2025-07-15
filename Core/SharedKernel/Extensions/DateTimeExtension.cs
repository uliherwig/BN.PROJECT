namespace BN.PROJECT.Core;

public static class DateTimeExtension
{
    public static DateTime PostgresMinValue()
    {
        return new DateTime(1000, 1, 1, 0, 0, 0);
    }
}

