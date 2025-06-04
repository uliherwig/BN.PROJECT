namespace BN.PROJECT.Core;

public class TestLogger<T> : ILogger<T>
{
    public List<LogEntry> LoggedMessages { get; } = new List<LogEntry>();

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        LoggedMessages.Add(new LogEntry
        {
            LogLevel = logLevel,
            Message = formatter(state, exception),
            Exception = exception
        });
    }
}