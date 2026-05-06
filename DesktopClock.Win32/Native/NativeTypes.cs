using System.Runtime.InteropServices;

namespace DesktopClock.Win32.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeRect
{
    internal int Left;
    internal int Top;
    internal int Right;
    internal int Bottom;
    internal readonly int Width => Right - Left;
    internal readonly int Height => Bottom - Top;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativePoint
{
    internal int X;
    internal int Y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MonitorInfo
{
    internal int Size;
    internal NativeRect Monitor;
    internal NativeRect Work;
    internal uint Flags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DwmBlurBehind
{
    internal int Flags;

    [MarshalAs(UnmanagedType.Bool)]
    internal bool Enable;

    internal IntPtr BlurRegion;

    [MarshalAs(UnmanagedType.Bool)]
    internal bool TransitionOnMaximized;
}

internal delegate bool MonitorEnumDelegate(IntPtr monitorHandle, IntPtr monitorDeviceContext, ref NativeRect monitorBounds, IntPtr data);
