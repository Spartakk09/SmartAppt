namespace SmartAppt.Common.Logging;

internal static class LogSettings
{
    static LogSettings()
    {
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environment == "Development")
        {
            MinimumLevel = LogLevel.Debug;
            WriteToConsole = true;
        }
        else
        {
            MinimumLevel = LogLevel.Warn;
            WriteToConsole = false;
        }
    }

    public static LogLevel MinimumLevel { get; set; } = LogLevel.Warn;
    public static bool WriteToConsole { get; set; } = true;
}
