using System.Text;
using DesktopClock.Helpers;
using Serilog;
using Windows.Storage;

namespace DesktopClock.Services;

internal class LoggingService : ILoggingService
{
    private const string _defaultApplicationDataFolder = "DesktopClock/ApplicationData";
    private const string _defaultLoggingFile = "desktopclock.log";
    private const int _retentionPeriodDays = 90;
    private const RollingInterval _defaultRollingInterval = RollingInterval.Month;

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _applicationDataFolder;

    public LoggingService()
    {
        _applicationDataFolder = Path.Combine(_localApplicationData, _defaultApplicationDataFolder);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(GetLoggingFilePath(), rollingInterval: _defaultRollingInterval)
            .CreateLogger();
    }

    public async Task InitializeAsync()
    {
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }

    private string GetLoggingFilePath()
    {
        return Path.Combine(GetLoggingFolderPath(), _defaultLoggingFile);
    }

    private string GetLoggingFolderPath()
    {
        if (RuntimeHelper.IsMSIX)
        {
            var dirInfo = new DirectoryInfo(ApplicationData.Current.LocalFolder.Path);
            return Path.Combine(dirInfo.Parent.FullName, "AppData");
        }
        else
        {
            return _applicationDataFolder;
        }
    }

    public async Task WriteLogAsync(string className, string methodName, string message = "", LogSeverity severity = LogSeverity.Information, Exception? exception = null)
    {
        await Task.Run(() =>
        {
            WriteLog(className, methodName, message, severity, exception);
        });
    }

    public void WriteLog(string className, string methodName, string message = "", LogSeverity severity = LogSeverity.Information, Exception? exception = null)
    {
        var escapedMessage = EscapeSpecialCharacters(message);
        var messageLine = String.IsNullOrEmpty(message) ? $"class: {className} | method: {methodName}" : $"class: {className} | method: {methodName} | message: {escapedMessage}";

        switch (severity)
        {
            case LogSeverity.Fatal:
                Log.Fatal(messageLine, exception);
                break;
            case LogSeverity.Error:
                Log.Error(messageLine, exception);
                break;
            case LogSeverity.Warning:
                Log.Warning(messageLine, exception);
                break;
            case LogSeverity.Debug:
                Log.Debug(messageLine, exception);
                break;
            case LogSeverity.Verbose:
                Log.Verbose(messageLine, exception);
                break;
            default:
                Log.Information(messageLine, exception);
                break;
        }
    }

    private string EscapeSpecialCharacters(string message)
    {
        if (String.IsNullOrEmpty(message)) return String.Empty;

        return new StringBuilder(message)
                .Replace("{", "{{")
                .Replace("}", "}}")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .ToString();
    }

    public async Task RemoveExpiredLogsAsync()
    {
        var loggingFolder = GetLoggingFolderPath();
        try
        {
            await Task.Run(async () =>
            {
                var directoryInfo = new DirectoryInfo(loggingFolder);
                if (!directoryInfo.Exists) return;

                var retentionPeriod = TimeSpan.FromDays(_retentionPeriodDays);
                var cutoffDate = DateTime.Now.Subtract(retentionPeriod);
                var deleted = false;
                foreach (var file in directoryInfo.GetFiles("*.log"))
                {
                    try
                    {
                        if (file.CreationTime < cutoffDate)
                        {
                            await WriteLogAsync(nameof(LoggingService), nameof(RemoveExpiredLogsAsync), $"Delete log file {file.Name}.");
                            file.Delete();
                            deleted = true;
                        }
                    }
                    catch (Exception exp)
                    {
                        await WriteLogAsync(nameof(LoggingService), nameof(RemoveExpiredLogsAsync), $"Failed to delete log file {file.Name}.", LogSeverity.Warning, exp);
                    }
                }
                if (!deleted) await WriteLogAsync(nameof(LoggingService), nameof(RemoveExpiredLogsAsync), $"No log files are to be deleted.");
            });
        }
        catch (Exception exp)
        {
            await WriteLogAsync(nameof(LoggingService), nameof(RemoveExpiredLogsAsync), $"Error occurred while trying to remove expired.", LogSeverity.Warning, exp);
        }
    }
}

public enum LogSeverity
{
    Verbose, Debug, Information, Warning, Error, Fatal
}
