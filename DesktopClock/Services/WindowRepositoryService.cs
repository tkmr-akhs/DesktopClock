using Microsoft.UI.Xaml.Controls;
using DesktopClock.Helpers;
using DesktopClock.Views;

namespace DesktopClock.Services;

internal class WindowRepositoryService : IWindowRepositoryService
{
    private readonly IDictionary<Type, WindowEx> windowDictionary;

    public WindowRepositoryService()
    {
        windowDictionary = new Dictionary<Type, WindowEx>();
        //Initialize();
    }

    public async Task InitializeAsync()
    {
        var mainWindow = App.MainWindow;

        if (mainWindow == null) throw new InvalidOperationException("Main window is null. Ensure that the window is properly initialized before calling Initialize.");
        if (Contains<MainPage>() && windowDictionary[typeof(MainPage)] != mainWindow) throw new InvalidOperationException("Main window is already initialized with a different instance of MainPage. Ensure that the main window is not being set twice.");

        windowDictionary.TryAdd(typeof(MainPage), mainWindow);
    }

    public bool TryAddWindowOfPage<TPage>() where TPage : Page, new()
    {
        return TryAddWindowOfPage(new TPage());
    }

    public bool TryAddWindowOfPage<TPage>(TPage page) where TPage : Page
    {
        if (Contains<TPage>()) return false;

        var window = SubWindowHelper.CreateWindow();
        window.Content = page;
        return windowDictionary.TryAdd(typeof(TPage), window);
    }

    public WindowEx GetWindowOfPage<TPage>() where TPage : Page
    {
        return windowDictionary[typeof(TPage)];
    }

    public bool TryGetWindowOfPage<TPage>(out WindowEx? window) where TPage : Page
    {
        return windowDictionary.TryGetValue(typeof(TPage), out window);
    }

    public bool RemoveWindowOfPage<TPage>() where TPage : Page
    {
        if(!Contains<TPage>()) return false;

        var window = windowDictionary[typeof(TPage)];

        try { window.Close(); }
        catch(Exception e) { System.Diagnostics.Debug.Write(e); }

        return windowDictionary.Remove(typeof(TPage));
    }

    public bool Contains<TPage>() where TPage : Page
    {
        return windowDictionary.ContainsKey(typeof(TPage));
    }
}
