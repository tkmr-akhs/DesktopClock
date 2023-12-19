using DesktopClock.Helpers;

namespace DesktopClock.Services;

public class AutoStartSelectorService : IAutoStartSelectorService
{
    private const string VALUE_NAME = "DesktopClock for Windows";

    public bool AutoStart { get; private set; }

    //private RegistryKey _registryKey;

    public AutoStartSelectorService()
    {
    }

    public async Task InitializeAsync()
    {
        AutoStart = await LoadSettingAsync();

        await Task.CompletedTask;
    }

    public async Task SetAsync(bool autoStart)
    {
        if (autoStart != AutoStart)
        {
            await SaveSettingAsync(autoStart);
            AutoStart = autoStart;
        }
    }

    private async Task<bool> LoadSettingAsync()
    {
        using var _registryKeyRoot = Microsoft.Win32.Registry.CurrentUser;
        using var _registryKey = _registryKeyRoot.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        return _registryKey?.GetValue(VALUE_NAME) != null;
    }

    private async Task SaveSettingAsync(bool autoStart)
    {
        var _registryKey = await RegistryHelper.CurrentUser.OpenSubKeyAsync("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        var exePath = System.Reflection.Assembly.GetEntryAssembly().Location;

        if (exePath.EndsWith(".dll")) exePath = exePath.Substring(0, exePath.Length - 4) + ".exe";
        else if (!exePath.EndsWith(".exe")) exePath += ".exe";

        if (autoStart)
        {
            await _registryKey.SetValueAsync(VALUE_NAME, exePath);

        }
        else
        {
            await _registryKey.DeleteValueAsync(VALUE_NAME);
        }
    } 
}
