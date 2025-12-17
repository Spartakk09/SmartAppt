namespace SmartAppt.Common.Logging;

internal static class LogContextAccessor
{
    private static readonly AsyncLocal<LogContext?> _current = new();

    public static LogContext Current
    {
        get => _current.Value ??= new LogContext();
        set => _current.Value = value;
    }
}
