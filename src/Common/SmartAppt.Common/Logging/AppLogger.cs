namespace SmartAppt.Common.Logging;

internal sealed class AppLogger<T> : IAppLogger<T>
{
    private static readonly string Category = typeof(T).Name;

    public void Debug(string message) => Write(LogLevel.Debug, message);
    public void Info(string message) => Write(LogLevel.Info, message);
    public void Warn(string message) => Write(LogLevel.Warn, message);
    public void Error(string message, Exception ex)
        => Write(LogLevel.Error, message, ex);

    private static void Write(LogLevel level, string message, Exception? ex = null)
    {
        if (level < LogSettings.MinimumLevel)
            return;

        var ctx = LogContextAccessor.Current;

        var line = LogFormatter.Format(
            DateTime.UtcNow,
            level,
            Category,
            ctx.CorrelationId,
            message,
            ex
        );

        if (LogSettings.WriteToConsole && level >= LogLevel.Debug)
            Console.WriteLine(line);

        // Only write important logs (Warn & Error) to file
        if (level >= LogLevel.Warn)
            LogQueue.Enqueue(line);
    }
}