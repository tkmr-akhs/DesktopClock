using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using DesktopClock.ViewModels;
using Windows.Graphics;

namespace DesktopClock.Views;

public sealed partial class CalendarPage : Page
{
    private readonly IWindowRepositoryService _windowRepositoryService;
    private readonly IWindowAlignmentSelectorService _windowAlignmentSelectorService;
    private readonly IScreenChangeDetectionService _screenChangeDetectionService;

    private SizeInt32 CurrentSize = new(0, 0);

    public CalendarViewModel ViewModel
    {
        get;
    }

    public CalendarPage()
    {
        ViewModel = App.GetService<CalendarViewModel>();
        InitializeComponent();
        this.DataContext = ViewModel;

        _windowRepositoryService = App.GetService<IWindowRepositoryService>();
        _windowAlignmentSelectorService = App.GetService<IWindowAlignmentSelectorService>();
        _screenChangeDetectionService = App.GetService<IScreenChangeDetectionService>();
    }

    public Windows.Foundation.Size GetActualSize()
    {
        return new Windows.Foundation.Size(ActualContentArea.ActualWidth, ActualContentArea.ActualHeight);
    }

    public Windows.Foundation.Size GetRenderSize()
    {
        return ActualContentArea.RenderSize;
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var calendarWindow = _windowRepositoryService.GetWindowOfPage<CalendarPage>();
        AppWindow_ChangedCore();
        calendarWindow.AppWindow.Changed += AppWindow_Changed;
    }

    private void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
    {
        if (args.DidSizeChange)
        {
            if (CurrentSize.Width != sender.Size.Width)
            {
                AppWindow_ChangedCore();
            }
        }
    }

    private void AppWindow_ChangedCore()
    {
        var thisWindow = _windowRepositoryService.GetWindowOfPage<CalendarPage>();

        var columnWidth = thisWindow.Width / 8;

        PrevMonthButton.Width = columnWidth / 2;
        NextMonthButton.Width = columnWidth / 2;

        CalendarDataGrid.ColumnWidth = new DataGridLength(columnWidth, DataGridLengthUnitType.Pixel);
        
        CalendarDataGrid.RowHeight = columnWidth;
        CalendarDataGrid.MinHeight = columnWidth * 6;

        BaseTextStyleFont.Value = (int)Math.Round(columnWidth / 2);

        CurrentSize = thisWindow.AppWindow.Size;
    }

    private void CalendarPage_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
    {
        if (e.PreviousSize.Height != e.NewSize.Height)
        {

            _windowAlignmentSelectorService.AdjustSize();
            if (_windowAlignmentSelectorService.AlignmentSetting.IsBottom)
            {
                _windowAlignmentSelectorService.SetRequestedAlignment();
            }
        }
    }
}
