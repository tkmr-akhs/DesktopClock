namespace DesktopClock.Win32.Native;

internal static class NativeConstants
{
    internal const long AppModelErrorNoPackage = 15700L;
    internal const int NativeBooleanTrue = 1;
    internal const int WindowLongIndexOwner = -8;
    internal const int WindowLongIndexStyle = -16;
    internal const int WindowLongIndexExtendedStyle = -20;
    internal const long NativeWindowStyleBorder = 0x00800000L;
    internal const long NativeWindowStyleCaption = 0x00C00000L;
    internal const long NativeWindowStyleDialogFrame = 0x00400000L;
    internal const long NativeWindowStyleThickFrame = 0x00040000L;
    internal const long NativeWindowFrameStyles = NativeWindowStyleBorder | NativeWindowStyleCaption | NativeWindowStyleDialogFrame | NativeWindowStyleThickFrame;
    internal const long ExtendedWindowStyleDialogModalFrame = 0x00000001L;
    internal const long ExtendedWindowStyleWindowEdge = 0x00000100L;
    internal const long ExtendedWindowStyleClientEdge = 0x00000200L;
    internal const long ExtendedWindowStyleStaticEdge = 0x00020000L;
    internal const long NativeWindowShadowExtendedStyles =
        ExtendedWindowStyleDialogModalFrame |
        ExtendedWindowStyleWindowEdge |
        ExtendedWindowStyleClientEdge |
        ExtendedWindowStyleStaticEdge;
    internal const uint SetWindowPositionNoSize = 0x0001;
    internal const uint SetWindowPositionNoMove = 0x0002;
    internal const uint SetWindowPositionNoZOrder = 0x0004;
    internal const uint SetWindowPositionNoActivate = 0x0010;
    internal const uint SetWindowPositionFrameChanged = 0x0020;
    internal const uint SetWindowPositionNoOwnerZOrder = 0x0200;
    internal const uint SetWindowPositionFrameChangedFlags =
        SetWindowPositionNoSize |
        SetWindowPositionNoMove |
        SetWindowPositionNoZOrder |
        SetWindowPositionNoActivate |
        SetWindowPositionFrameChanged |
        SetWindowPositionNoOwnerZOrder;
    internal const int DwmBlurBehindEnable = 0x00000001;
    internal const int DwmBlurBehindBlurRegion = 0x00000002;
    internal const int DwmWindowAttributeNonClientRenderingPolicy = 2;
    internal const int DwmWindowAttributeWindowCornerPreference = 33;
    internal const int DwmWindowAttributeBorderColor = 34;
    internal const int DwmWindowAttributeSystemBackdropType = 38;
    internal const int DwmWindowAttributeRedirectionBitmapAlpha = 39;
    internal const int DwmNonClientRenderingPolicyDisabled = 1;
    internal const int DwmWindowCornerPreferenceDoNotRound = 1;
    internal const int DwmSystemBackdropTypeNone = 1;
    internal const uint DwmColorNone = 0xFFFFFFFE;
    internal const int WindowActivateInactive = 0x00;
    internal const int WindowActivateActive = 0x01;
    internal const int WindowMessageActivate = 0x0006;
}
