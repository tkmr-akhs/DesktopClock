using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DesktopClock.ViewModels;
using DesktopClock.Win32.Input;

namespace DesktopClock.Views;

public sealed partial class ClockPage : Page
{
    private readonly IWindowRepositoryService _windowRepositoryService;
    private readonly IWindowAlignmentSelectorService _windowAlignmentSelectorService;
    private readonly ICursorService _cursorService;
    private bool _isWaitingForPointerExit;

    public ClockViewModel ViewModel
    {
        get;
    }

    public ClockPage()
    {
        ViewModel = App.GetService<ClockViewModel>();
        InitializeComponent();
        this.DataContext = ViewModel;

        _windowRepositoryService = App.GetService<IWindowRepositoryService>();
        _windowAlignmentSelectorService = App.GetService<IWindowAlignmentSelectorService>();
        _cursorService = App.GetService<ICursorService>();
    }

    public Windows.Foundation.Size GetActualSize()
    {
        return new Windows.Foundation.Size(ActualContentArea.ActualWidth, ActualContentArea.ActualHeight);
    }

    public double GetClockWidth()
    {
        return HourTens.ActualWidth + HourOnes.ActualWidth + HourMinuteSeparator.ActualWidth + MinuteTens.ActualWidth + MinuteOnes.ActualWidth;
    }

    private void ClockPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _windowAlignmentSelectorService.AdjustSize();

        _windowAlignmentSelectorService.SetRequestedAlignment();
    }

    private async void Page_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        await HideWindowAndWaitPointerExitAsync();
    }

    private async void Page_DragEnter(object sender, DragEventArgs e)
    {
        await HideWindowAndWaitPointerExitAsync();
    }

    private async Task HideWindowAndWaitPointerExitAsync()
    {
        if (_isWaitingForPointerExit)
        {
            return;
        }

        _isWaitingForPointerExit = true;

        try
        {
            var clockWindow = _windowRepositoryService.GetWindowOfPage<ClockPage>();
            clockWindow.Hide();

            var winPosSize = new WindowPosAndSize
            {
                Left = clockWindow.AppWindow.Position.X,
                Top = clockWindow.AppWindow.Position.Y,
                Width = clockWindow.AppWindow.Size.Width,
                Height = clockWindow.AppWindow.Size.Height
            };

            await WaitMouseLeaveAsync(winPosSize);
            clockWindow.Show();
        }
        finally
        {
            _isWaitingForPointerExit = false;
        }
    }

    /// <summary>
    /// ウィンドウ上からマウスが出るのを待機する。
    /// </summary>
    /// <param name="winPosSize">ウィンドウ位置およびサイズ。</param>
    private async Task WaitMouseLeaveAsync(WindowPosAndSize winPosSize)
    {
        System.Drawing.Point cursorPos;
        do
        {
            cursorPos = _cursorService.GetCursorPosition();
            await Task.Delay(500);
        } while (OnWindow(cursorPos, winPosSize));
    }

    /// <summary>
    /// マウス カーソルの位置とウィンドウ位置およびサイズとを比較し、マウス カーソルがウィンドウ上にあるかを判断する。
    /// </summary>
    /// <param name="cursorPos">マウス カーソル位置</param>
    /// <param name="winPosSize">ウィンドウ位置およびサイズ</param>
    /// <returns>ウィンドウ上なら true。それ以外の場合は false。</returns>
    private static bool OnWindow(System.Drawing.Point cursorPos, WindowPosAndSize winPosSize)
    {
        return !(
            cursorPos.X < winPosSize.Left
            || cursorPos.Y < winPosSize.Top
            || cursorPos.X > winPosSize.Left + winPosSize.Width
            || cursorPos.Y > winPosSize.Top + winPosSize.Height);
    }

    /// <summary>
    /// ウィンドウの位置およびサイズを格納する構造体。
    /// </summary>
    private struct WindowPosAndSize
    {
        internal double Left;
        internal double Top;
        internal double Width;
        internal double Height;
    }
}
