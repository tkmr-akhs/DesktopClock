using DesktopClock.Win32.Native;

namespace DesktopClock.Win32.Runtime;

/// <summary>
/// Provides package identity information by using Windows AppModel APIs.
/// </summary>
public sealed class PackageIdentityService : IPackageIdentityService
{
    /// <inheritdoc />
    public bool IsPackaged
    {
        get
        {
            var length = 0;
            return Kernel32.GetCurrentPackageFullName(ref length, null) != NativeConstants.AppModelErrorNoPackage;
        }
    }
}
