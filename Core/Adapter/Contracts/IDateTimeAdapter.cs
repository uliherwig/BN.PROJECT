namespace BN.PROJECT.Core;

public interface IDateTimeAdapter
{
    DateTimeOffset Now();

    DateTimeOffset UtcNow();
}