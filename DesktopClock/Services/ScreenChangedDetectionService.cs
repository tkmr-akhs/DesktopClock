using System.Drawing;
using WinFormsWrapper;
using DesktopClock.Models;
using DesktopClock.Contracts.Services;

namespace DesktopClock.Services;

internal class ScreenChangedDetectionService : IScreenChangeDetectionService
{
    private const int DetectionIntervalMillisecond = 10000;

    private readonly ILoggingService _loggingService;
    private readonly IDispatcherQueueService _dispatcherQueueService;

    public IReadOnlyList<Rectangle> ScreenBounds { get; private set; } = new List<Rectangle>().AsReadOnly();

    public event ScreenChangedEventHandler? ScreenChanged;

    public ScreenChangedDetectionService(ILoggingService loggingService, IDispatcherQueueService dispatcherQueueSerivce)
    {
        _loggingService = loggingService;
        _dispatcherQueueService = dispatcherQueueSerivce;
    }

    public async Task InitializeAsync()
    {
        // 初期スクリーンバウンドを設定
        ScreenBounds = ScreenInformation.GetScreensBounds();

        // スクリーンサイズの監視を開始
        MonitorScreenChangesAsync();

        await Task.CompletedTask;

    }

    private async void MonitorScreenChangesAsync()
    {
        IReadOnlyList<Rectangle> previousBounds;

        while (true)
        {
            await Task.Delay(DetectionIntervalMillisecond);

            previousBounds = this.ScreenBounds;
            ScreenBounds = ScreenInformation.GetScreensBounds();

            // スクリーンの増減をチェック
            if (previousBounds.Count < ScreenBounds.Count)
            {
                // 増えた場合
                for (int i = previousBounds.Count; i < ScreenBounds.Count; i++)
                {
                    OnScreenChanged(i, Rectangle.Empty, ScreenBounds[i], ScreenChangeType.ScreenAdded);
                }
            }
            else if (ScreenBounds.Count < previousBounds.Count)
            {
                // 減った場合
                for (int i = ScreenBounds.Count; i < previousBounds.Count; i++)
                {
                    OnScreenChanged(i, previousBounds[i], Rectangle.Empty, ScreenChangeType.ScreenRemoved);
                }
            }

            // スクリーンサイズに変更があるかチェック
            for (int i = 0; i < Math.Min(previousBounds.Count, ScreenBounds.Count); i++)
            {
                if (!ScreenBounds[i].Equals(previousBounds[i]))
                {
                    OnScreenChanged(i, previousBounds[i], ScreenBounds[i], ScreenChangeType.ScreenSizeChanged);
                }
            }
        }
    }

    protected virtual void OnScreenChanged(int screenId, Rectangle oldBounds, Rectangle newBounds, ScreenChangeType changeType)
    {
        var changed = GetChangedFlags(oldBounds, newBounds);
        var args = new ScreenChangedEventArgs(screenId, oldBounds, newBounds, changed, changeType);

        var invokeResult = _dispatcherQueueService.TryInvoke(() => { ScreenChanged?.Invoke(this, args); });

        if (!invokeResult) {
            _loggingService.WriteLogAsync(nameof(ScreenChangedDetectionService), nameof(OnScreenChanged), "Failed to execute delegates subscribed to the ScreenChanged event.", severity: LogSeverity.Error);
        }
    }

    private ScreenChangedSize GetChangedFlags(Rectangle oldBounds, Rectangle newBounds)
    {
        ScreenChangedSize flags = ScreenChangedSize.None;
        if (oldBounds.X != newBounds.X) flags |= ScreenChangedSize.X;
        if (oldBounds.Y != newBounds.Y) flags |= ScreenChangedSize.Y;
        if (oldBounds.Width != newBounds.Width) flags |= ScreenChangedSize.Width;
        if (oldBounds.Height != newBounds.Height) flags |= ScreenChangedSize.Height;

        return flags;
    }
}