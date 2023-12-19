using DesktopClock.Core.Models;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DesktopClock.Views;

public sealed partial class 詳細を一覧表示DetailControl : UserControl
{
    public SampleOrder? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as SampleOrder;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }

    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(SampleOrder), typeof(詳細を一覧表示DetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public 詳細を一覧表示DetailControl()
    {
        InitializeComponent();
    }

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is 詳細を一覧表示DetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
