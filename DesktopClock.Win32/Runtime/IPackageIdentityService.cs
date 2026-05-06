namespace DesktopClock.Win32.Runtime;

/// <summary>
/// Provides information about the current Windows package identity.
/// </summary>
public interface IPackageIdentityService
{
    /// <summary>
    /// Gets a value indicating whether the current process has an MSIX package identity.
    /// </summary>
    bool IsPackaged { get; }
}
