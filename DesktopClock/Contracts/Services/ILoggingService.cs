using DesktopClock.Services;

namespace DesktopClock.Contracts.Services;

public interface ILoggingService : IDisposable
{
    Task InitializeAsync();

    Task WriteLogAsync(string className, string methodName, string message = "", LogSeverity severity = LogSeverity.Information, Exception? exception = null);

    void WriteLog(string className, string methodName, string message = "", LogSeverity severity = LogSeverity.Information, Exception? exception = null);

    Task RemoveExpiredLogsAsync();
}
