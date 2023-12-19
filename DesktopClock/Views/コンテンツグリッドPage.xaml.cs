using DesktopClock.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace DesktopClock.Views;

public sealed partial class コンテンツグリッドPage : Page
{
    public コンテンツグリッドViewModel ViewModel
    {
        get;
    }

    public コンテンツグリッドPage()
    {
        ViewModel = App.GetService<コンテンツグリッドViewModel>();
        InitializeComponent();
    }
}
