namespace SmartAppt.Common.Logging;

internal static class LogFormatter
{
    public static string Format(
        DateTime timestamp,
        LogLevel level,
        string category,
        string correlationId,
        string message,
        Exception? ex)
    {
        var line =
            $"{timestamp:O} | " +
            $"{level,-5} | " +
            $"{category} | " +
            $"{correlationId} | " +
            $"{message}";

        if (ex != null)
            line += $" | {ex.GetType().Name}: {ex.Message}";

        return line;
    }
}
