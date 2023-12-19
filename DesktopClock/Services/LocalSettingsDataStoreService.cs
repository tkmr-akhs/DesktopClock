using System.Reflection;

namespace DesktopClock.Services;

internal class LocalSettingsDataStoreService : ILocalSettingsDataStoreService
{
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IDictionary<string, Type> _keys;

    public LocalSettingsDataStoreService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
        _keys = new Dictionary<string, Type>();
    }

    public async Task ClearAsync()
    {
        // Retrieve the generic method definition for SaveSettingAsync once outside the loop to improve performance.
        var genericSaveMethod = typeof(ILocalSettingsService).GetMethod(nameof(ILocalSettingsService.SaveSettingAsync));

        if (genericSaveMethod == null)
        {
            throw new InvalidOperationException("The SaveSettingAsync method is not defined on the ILocalSettingsService interface.");
        }

        foreach (var pair in _keys)
        {
            var defaultValue = GetDefault(pair.Value);
            // Create a closed generic method for the specific type.
            var saveMethod = genericSaveMethod.MakeGenericMethod(pair.Value);

            // Invoke the closed generic method with the key and the default value.
            await (Task)saveMethod.Invoke(_localSettingsService, new object[] { pair.Key, defaultValue });
        }
    }

    private object? GetDefault(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }

    public async Task DeleteAsync<T>(string key)
    {
        await _localSettingsService.SaveSettingAsync<T>(key, default);
        _keys[key] = typeof(T);
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var value = await _localSettingsService.ReadSettingAsync<T>(key);
        _keys[key] = typeof(T);
        return value;
    }

    public async Task StoreAsync<T>(string key, T value)
    {
        await _localSettingsService.SaveSettingAsync(key, value);
        _keys[key] = typeof(T);
    }

}
