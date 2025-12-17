using Microsoft.Extensions.Hosting;

namespace SmartAppt.Common.Logging;

internal sealed class LogBackgroundWriter : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var line in LogQueue.ReadAllAsync(stoppingToken))
        {
            WriteToFile(line);
        }
    }

    private static void WriteToFile(string line)
    {
        try
        {
            // Desktop Logs folder
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var logFolder = Path.Combine(desktop, "Logs");
            Directory.CreateDirectory(logFolder);

            // Daily log file
            var filePath = Path.Combine(
                logFolder,
                $"app-{DateTime.UtcNow:yyyy-MM-dd}.log");

            File.AppendAllText(filePath, line + Environment.NewLine);
        }
        catch
        {
            // Never crash logging
        }
    }
}
