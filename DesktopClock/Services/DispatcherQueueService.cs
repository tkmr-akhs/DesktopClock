using Microsoft.UI.Dispatching;

namespace DesktopClock.Services;

public class DispatcherQueueService : IDispatcherQueueService
{
    private DispatcherQueue? _dispatcherQueue;

    public DispatcherQueueService()
    {
    }

    public async Task InitializeAsync()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public bool TryInvoke(DispatcherQueueHandler callback)
    {
        return TryInvoke(DispatcherQueuePriority.Normal, callback);
    }

    public bool TryInvoke(DispatcherQueuePriority priority, DispatcherQueueHandler callback)
    {
        if (_dispatcherQueue != default)
        {
            return _dispatcherQueue.TryEnqueue(priority, callback);
        }
        else
        {
            callback.Invoke();
            return true;
        }
    }
}
