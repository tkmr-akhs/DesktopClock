using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DesktopClock.ViewModels;

namespace DesktopClock.Views;

public sealed partial class ClockPage : Page
{
    private readonly IWindowRepositoryService _windowRepositoryService;
    private readonly IWindowAlignmentSelectorService _windowAlignmentSelectorService;

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
    }

    public Windows.Foundation.Size GetActualSize()
    {
        return new Windows.Foundation.Size(ActualContentArea.ActualWidth, ActualContentArea.ActualHeight);
    }

    public Windows.Foundation.Size GetRenderSize()
    {
        return ActualContentArea.RenderSize;
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
        HideWindowAndWaitPointerExit();
    }

    private void Page_DragEnter(object sender, DragEventArgs e)
    {
        HideWindowAndWaitPointerExit();
    }

    private async void HideWindowAndWaitPointerExit()
    {
        var clockWindow = _windowRepositoryService.GetWindowOfPage<ClockPage>();

        //System.Diagnostics.Debug.WriteLine("In");
        clockWindow.Hide();

        var winPosSize = new WindowPosAndSize()
        {
            Left = clockWindow.AppWindow.Position.X,
            Top = clockWindow.AppWindow.Position.Y,
            Width = clockWindow.Width,
            Height = clockWindow.Height
        };

        var task = new Task<Visibility>((x) =>
        {
            WaitMouseLeave(x);
            return Visibility.Visible;
        }, winPosSize);
        task.Start();


        if (await task == Visibility.Visible) {
            clockWindow.Show();
        }

        return;
    }

    /// <summary>
    /// ウィンドウ上からマウスが出るのを待機する。
    /// </summary>
    /// <param name="winPosSizeObject">ウィンドウ位置およびサイズ</param>
    private static void WaitMouseLeave(object winPosSizeObject)
    {
        if (winPosSizeObject is WindowPosAndSize winPosSize)
        {
            System.Drawing.Point cursorPos;
            do
            {
                cursorPos = System.Windows.Forms.Cursor.Position;
                //System.Diagnostics.Debug.WriteLine($"In: cursorPos({cursorPos.X}, {cursorPos.X}) / winPosSize({winPosSize.Left} {winPosSize.Width}, {winPosSize.Top} {winPosSize.Height})");
                System.Threading.Thread.Sleep(500);
            } while (OnWindow(cursorPos, winPosSize));

            //System.Diagnostics.Debug.WriteLine("Out");
        }
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
