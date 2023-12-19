using Microsoft.UI.Dispatching;

namespace DesktopClock.Contracts.Services;

/// <summary>
/// Provides a service for executing code on the UI thread through a DispatcherQueue.
/// This service is intended to facilitate UI operations from non-UI threads safely and efficiently.
/// </summary>
public interface IDispatcherQueueService
{
    /// <summary>
    /// Initializes the DispatcherQueue associated with the current UI thread.
    /// This method should be called from the UI thread to ensure the DispatcherQueue is correctly associated.
    /// Failure to initialize on the UI thread may result in incorrect behavior when invoking UI operations.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task InitializeAsync();

    /// <summary>
    /// Invokes a method on the UI thread with normal priority.
    /// If the DispatcherQueue has not been initialized, the method is invoked directly but may not be thread-safe for UI operations.
    /// </summary>
    /// <param name="callback">The method to invoke on the UI thread.</param>
    /// <returns>True if the method was successfully queued or invoked; false otherwise.</returns>
    public bool TryInvoke(DispatcherQueueHandler callback);

    /// <summary>
    /// Invokes a method on the UI thread with the specified priority.
    /// If the DispatcherQueue has not been initialized, the method is invoked directly but may not be thread-safe for UI operations.
    /// </summary>
    /// <param name="priority">The priority at which to invoke the method on the UI thread.</param>
    /// <param name="callback">The method to invoke on the UI thread.</param>
    /// <returns>True if the method was successfully queued or invoked; false otherwise.</returns>
    public bool TryInvoke(DispatcherQueuePriority priority, DispatcherQueueHandler callback);
}
