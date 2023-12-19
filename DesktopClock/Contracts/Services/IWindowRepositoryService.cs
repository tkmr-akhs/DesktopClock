using Microsoft.UI.Xaml.Controls;

namespace DesktopClock.Contracts.Services;

public interface IWindowRepositoryService
{
    Task InitializeAsync();
    bool TryGetWindowOfPage<TPage>(out WindowEx? window) where TPage : Page;
    WindowEx GetWindowOfPage<TPage>() where TPage : Page;
    bool TryAddWindowOfPage<TPage>() where TPage : Page, new();
    bool TryAddWindowOfPage<TPage>(TPage page) where TPage : Page;
    bool RemoveWindowOfPage<TPage>() where TPage : Page;
    bool Contains<TPage>() where TPage : Page;
}
