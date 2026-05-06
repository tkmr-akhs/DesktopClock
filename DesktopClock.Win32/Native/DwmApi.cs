using System.Runtime.InteropServices;

namespace DesktopClock.Win32.Native;

internal static class DwmApi
{
    [DllImport("dwmapi.dll")]
    internal static extern int DwmEnableBlurBehindWindow(IntPtr windowHandle, ref DwmBlurBehind blurBehind);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref uint attributeValue, int attributeSize);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int attributeValue, int attributeSize);
}
