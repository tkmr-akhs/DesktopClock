using System.Text;
using System.Diagnostics;
using System.Runtime.Versioning;
using DesktopClock.Core.Helpers;
using System.Text.RegularExpressions;

namespace DesktopClock.Helpers;

/// <summary>
/// Provides utilities for interacting with the Windows Registry.
/// This class offers methods to retrieve and set registry values, manage subkeys, and handle different registry views.
/// It is designed to work specifically with the Windows operating system.
/// </summary>
[SupportedOSPlatform("windows")]
public static class RegistryHelper
{
    /// <summary>Current User Key. This key should be used as the root for all user specific settings.</summary>
    public static readonly RegistryKey CurrentUser = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Default);

    /// <summary>Local Machine key. This key should be used as the root for all machine specific settings.</summary>
    public static readonly RegistryKey LocalMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Default);

    /// <summary>Classes Root Key. This is the root key of class information.</summary>
    public static readonly RegistryKey ClassesRoot = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.ClassesRoot, Microsoft.Win32.RegistryView.Default);

    /// <summary>Users Root Key. This is the root of users.</summary>
    public static readonly RegistryKey Users = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.Users, Microsoft.Win32.RegistryView.Default);

    /// <summary>Performance Root Key. This is where dynamic performance data is stored on NT.</summary>
    public static readonly RegistryKey PerformanceData = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.PerformanceData, Microsoft.Win32.RegistryView.Default);

    /// <summary>Current Config Root Key. This is where current configuration information is stored.</summary>
    public static readonly RegistryKey CurrentConfig = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentConfig, Microsoft.Win32.RegistryView.Default);

    /// <summary>
    /// Asynchronously retrieves the value associated with the specified key and value name.
    /// If the key or value name does not exist, the default value is returned.
    /// </summary>
    /// <typeparam name="T">The expected type of the registry value.</typeparam>
    /// <param name="keyName">The full name of the registry key.</param>
    /// <param name="valueName">The name of the value within the key to retrieve.</param>
    /// <returns>The value associated with the specified key and value name, or the default value if not found.</returns>
    public static async Task<T> GetValueAsync<T>(string keyName, string? valueName)
    {
        RegistryKey basekey = GetBaseKeyFromKeyName(keyName, out var subKeyName);

        RegistryKey key = await basekey.OpenSubKeyAsync(subKeyName);
        return await key.GetValueAsync<T>(valueName);
    }

    /// <summary>
    /// Asynchronously sets the specified registry key to the given value.
    /// If the key does not exist, it will be created.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="keyName">The full name of the registry key.</param>
    /// <param name="valueName">The name of the value within the key to set.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="valueKind">The registry data type for the value. If not specified, the type will be inferred.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SetValueAsync<T>(string keyName, string? valueName, T value, Microsoft.Win32.RegistryValueKind valueKind = Microsoft.Win32.RegistryValueKind.Unknown)
    {
        RegistryKey basekey = GetBaseKeyFromKeyName(keyName, out var subKeyName);

        RegistryKey? key = await basekey.CreateSubKeyAsync(subKeyName);
        Debug.Assert(key != null, "An exception should be thrown if failed!");
        await key.SetValueAsync(valueName, value, valueKind);

    }

    /// <summary>
    /// Parse a keyName and returns the basekey for it.
    /// It will also store the subkey name in the out parameter.
    /// If the keyName is not valid, we will throw ArgumentException.
    /// The return value shouldn't be null.
    /// </summary>
    public static RegistryKey GetBaseKeyFromKeyName(string keyName, out string subKeyName)
    {
        ArgumentNullException.ThrowIfNull(keyName);

        int i = keyName.IndexOf('\\');
        int length = i != -1 ? i : keyName.Length;

        // Determine the potential base key from the length.
        RegistryKey? baseKey = null;
        switch (length)
        {
            case 10: baseKey = Users; break; // HKEY_USERS
            case 17: baseKey = char.ToUpperInvariant(keyName[6]) == 'L' ? ClassesRoot : CurrentUser; break; // HKEY_C[L]ASSES_ROOT, otherwise HKEY_CURRENT_USER
            case 18: baseKey = LocalMachine; break; // HKEY_LOCAL_MACHINE
            case 19: baseKey = CurrentConfig; break; // HKEY_CURRENT_CONFIG
            case 21: baseKey = PerformanceData; break; // HKEY_PERFORMANCE_DATA
        }

        // If a potential base key was found, see if keyName actually starts with the potential base key's name.
        if (baseKey != null && keyName.StartsWith(baseKey.Name, StringComparison.OrdinalIgnoreCase))
        {
            subKeyName = (i == -1 || i == keyName.Length) ?
                string.Empty :
                keyName.Substring(i + 1);

            return baseKey;
        }

        throw new ArgumentException("The key name is invalid.", nameof(keyName));
    }
}

[SupportedOSPlatform("windows")]
public class RegistryKey
{
    private const int MaxKeyLength = 255;
    private const int MaxValueLength = 16383;

    private static readonly Microsoft.Win32.RegistryKey regKey;

    private static readonly Dictionary<Microsoft.Win32.RegistryValueKind, string> _valueKindName = new()
        {
            { Microsoft.Win32.RegistryValueKind.Binary, "REG_BINARY" },
            { Microsoft.Win32.RegistryValueKind.DWord, "REG_DWORD" },
            { Microsoft.Win32.RegistryValueKind.QWord, "REG_QWORD" },
            { Microsoft.Win32.RegistryValueKind.String, "REG_SZ" },
            { Microsoft.Win32.RegistryValueKind.ExpandString, "REG_EXPAND_SZ" },
            { Microsoft.Win32.RegistryValueKind.MultiString, "REG_MULTI_SZ" },
        };

    private static readonly Dictionary<string, Microsoft.Win32.RegistryValueKind> _valueKindDict = new()
        {
            { "REG_BINARY", Microsoft.Win32.RegistryValueKind.Binary },
            { "REG_DWORD", Microsoft.Win32.RegistryValueKind.DWord },
            { "REG_QWORD", Microsoft.Win32.RegistryValueKind.QWord },
            { "REG_SZ", Microsoft.Win32.RegistryValueKind.String },
            { "REG_EXPAND_SZ", Microsoft.Win32.RegistryValueKind.ExpandString },
            { "REG_MULTI_SZ", Microsoft.Win32.RegistryValueKind.MultiString },
        };

    private static readonly Dictionary<Microsoft.Win32.RegistryHive, string> _hiveShortName = new()
        {
            { Microsoft.Win32.RegistryHive.LocalMachine, "HKLM" },
            { Microsoft.Win32.RegistryHive.CurrentUser, "HKCU" },
            { Microsoft.Win32.RegistryHive.ClassesRoot, "HKCR" },
            { Microsoft.Win32.RegistryHive.Users, "HKU" },
            { Microsoft.Win32.RegistryHive.CurrentConfig, "HKCC" }
        };
    private static readonly Dictionary<Microsoft.Win32.RegistryHive, string> _hiveFullName = new()
        {
            { Microsoft.Win32.RegistryHive.LocalMachine, "HKEY_LOCAL_MACHINE" },
            { Microsoft.Win32.RegistryHive.CurrentUser, "HKEY_CURRENT_USER" },
            { Microsoft.Win32.RegistryHive.ClassesRoot, "HKEY_CLASSES_ROOT" },
            { Microsoft.Win32.RegistryHive.Users, "HKEY_USERS" },
            { Microsoft.Win32.RegistryHive.CurrentConfig, "HKEY_CURRENT_CONFIG" }
        };

    private readonly Microsoft.Win32.RegistryHive _hive;
    private readonly string _computerName;
    private readonly string _keyName;
    private readonly bool _writable;
    private readonly Microsoft.Win32.RegistryView _regView;
    private readonly CommandExecuter _regCmd;

    /// <summary>
    /// Initializes a new instance of the RegistryKey class.
    /// Represents a key-level node in the Windows registry. This class is a wrapper for a registry key.
    /// </summary>
    /// <param name="hive">The top-level node in the Windows registry.</param>
    /// <param name="computerName">The name of the computer on which to open the key.</param>
    /// <param name="keyName">The name of the key.</param>
    /// <param name="writable">Whether the key is writable.</param>
    /// <param name="view">The registry view to use on a 64-bit operating system.</param>
    private RegistryKey(Microsoft.Win32.RegistryHive hive, string computerName = "", string keyName = "", bool writable = false, Microsoft.Win32.RegistryView view = Microsoft.Win32.RegistryView.Default)
    {
        ValidateKeyView(view);

        _hive = hive;
        _computerName = computerName;
        _keyName = keyName;
        _writable = writable;
        _regView = view;
        _regCmd = new CommandExecuter("REG");
    }

    public string Name => _keyName;

    /// <summary>
    /// Asynchronously creates a new subkey or opens an existing subkey for write access.
    /// </summary>
    /// <param name="subkey">The name or path of the subkey to create or open.</param>
    /// <returns>A task that represents the asynchronous create or open operation.</returns>
    public async Task<RegistryKey> CreateSubKeyAsync(string subkey)
    {
        var fullName = GenerateFullName(subkey);
        var fullPath = GenerateFullPath(fullName);

        if (!await IsExistSubkeyAsync(subkey))
        {
            await _regCmd.ExecuteAsync(new string[] { "ADD", fullPath, CreateViewOption() });
        }

        return new RegistryKey(_hive, computerName: _computerName, keyName: fullName, writable: _writable, view: _regView);
    }

    private string CreateViewOption()
    {
        if (_regView == Microsoft.Win32.RegistryView.Registry32) return "/reg:32";
        else if (_regView == Microsoft.Win32.RegistryView.Registry64) return "/reg:64";

        return null;
    }

    /// <summary>
    /// Asynchronously deletes a specified value from this key.
    /// </summary>
    /// <param name="name">The name of the value to delete.</param>
    /// <param name="throwOnMissingValue">Indicates whether to throw an exception if the value does not exist.</param>
    public async Task DeleteValueAsync(string name, bool throwOnMissingValue = true)
    {
        EnsureWriteable();

        if (throwOnMissingValue)
        {
            try { await EnsureValueExistanceAsync(name); }
            catch (ArgumentException)
            {
                if (throwOnMissingValue) throw;
                else return;
            }
        }


        var fullName = GenerateFullName();
        var fullPath = GenerateFullPath(fullName);

        await _regCmd.ExecuteAsync(new string[] { "DELETE", fullPath, "/v", name, CreateViewOption(), "/f" });
    }

    /// <summary>
    /// Opens a new RegistryKey that represents the requested key on the local machine with the specified view.
    /// </summary>
    /// <param name="hKey">The top-level node in the Windows registry.</param>
    /// <param name="view">The registry view to use on a 64-bit operating system.</param>
    /// <returns>A new RegistryKey instance.</returns>
    public static RegistryKey OpenBaseKey(Microsoft.Win32.RegistryHive hKey, Microsoft.Win32.RegistryView view = Microsoft.Win32.RegistryView.Default)
    {
        ValidateKeyView(view);
        return new RegistryKey(hKey, view: view);
    }

    /// <summary>
    /// Asynchronously opens a specified subkey with optional write access.
    /// </summary>
    /// <param name="subkey">The name or path of the subkey to open.</param>
    /// <param name="writable">Indicates whether the subkey is opened with write access.</param>
    /// <returns>A task that represents the asynchronous open operation.</returns>
    public async Task<RegistryKey> OpenSubKeyAsync(string subkey, bool writable = false)
    {
        ValidateKeyName(subkey);

        var fullName = GenerateFullName(subkey);
        var fullPath = GenerateFullPath(fullName);

        await EnsureKeyExistanceAsync(fullPath);

        return new RegistryKey(_hive, _computerName, fullName, writable, _regView);
    }


    /// <summary>
    /// Asynchronously retrieves the specified value from this key.
    /// </summary>
    /// <param name="name">The name of the value to retrieve. If null or empty, retrieves the default value.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<T> GetValueAsync<T>(string? name)
    {
        ValidateValueName(name);

        var fullPath = GenerateFullPath(GenerateFullName());

        string stdout;

        try
        {
            if (String.IsNullOrEmpty(name))
            {
                stdout = await _regCmd.ExecuteAsync(new string[] { "QUERY", fullPath, "/ve", CreateViewOption() });
            }
            else
            {
                stdout = await _regCmd.ExecuteAsync(new string[] { "QUERY", fullPath, "/v", name, CreateViewOption() });
            }
        }
        catch (CommandExecutionException)
        {
            return default;
        }

        var value = ParseToRegistryValue(stdout);

        var valueKind = _valueKindDict[value.Kind];

        ValidateValueKind<T>(valueKind);

        switch (valueKind)
        {
            case Microsoft.Win32.RegistryValueKind.Binary:
                throw new NotImplementedException();
                break;
            case Microsoft.Win32.RegistryValueKind.DWord:
                return (T)Convert.ChangeType(UInt32.Parse(value.Data), typeof(T));
                throw new NotImplementedException();
                break;
            case Microsoft.Win32.RegistryValueKind.QWord:
                return (T)Convert.ChangeType(UInt64.Parse(value.Data), typeof(T));
                break;
            case Microsoft.Win32.RegistryValueKind.String:
                return (T)Convert.ChangeType(value.Data, typeof(T));
            case Microsoft.Win32.RegistryValueKind.ExpandString:
                throw new NotImplementedException();
                break;
            case Microsoft.Win32.RegistryValueKind.MultiString:
                throw new NotImplementedException();
                break;
            default:
                throw new Exception();
        }
    }

    private struct RegistryValue
    {
        internal string Data
        {
            get; set;
        }
        internal string Kind
        {
            get; set;
        }
    }

    private RegistryValue ParseToRegistryValue(string stdout)
    {
        var pattern = ".*\\n\\s*\\S.*\\s+(REG_SZ|REG_MULTI_SZ|REG_EXPAND_SZ|REG_DWORD|REG_QWORD|REG_BINARY)\\s+(\\S*)";
        var match = Regex.Match(stdout, pattern);

        if (match.Success)
        {
            return new RegistryValue()
            {
                Kind = match.Groups[1].Value,
                Data = match.Groups[2].Value,
            };
        }

        throw new ArgumentException("cannot parse");
    }

    /// <summary>
    /// Asynchronously sets the specified value for this key.
    /// </summary>
    /// <param name="name">The name of the value to set. If null or empty, sets the default value.</param>
    /// <param name="value">The data to store.</param>
    /// <param name="valueKind">The type of data to store, as a RegistryValueKind.</param>
    public async Task SetValueAsync<T>(string? name, T value, Microsoft.Win32.RegistryValueKind valueKind = Microsoft.Win32.RegistryValueKind.Unknown)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateValueName(name);

        if (name != null && name.Length > MaxValueLength)
        {
            throw new ArgumentException("SR.Arg_RegValStrLenBug", nameof(name));
        }

        if (!Enum.IsDefined(typeof(Microsoft.Win32.RegistryValueKind), valueKind))
        {
            throw new ArgumentException("SR.Arg_RegBadKeyKind", nameof(valueKind));
        }

        EnsureWriteable();

        if (valueKind == Microsoft.Win32.RegistryValueKind.Unknown)
        {
            // this is to maintain compatibility with the old way of autodetecting the type.
            // SetValue(string, object) will come through this codepath.
            valueKind = CalculateValueKind(value);
        }
        else
        {
            ValidateValueKind<T>(valueKind);
        }

        var fullPath = GenerateFullPath(GenerateFullName());
        string stdout;
        if (String.IsNullOrEmpty(name))
        {
            stdout = await _regCmd.ExecuteAsync(new string[] { "ADD", fullPath, "/ve", "/d", value.ToString(), "/t", _valueKindName[valueKind], CreateViewOption(), "/f" });
        }
        else
        {
            stdout = await _regCmd.ExecuteAsync(new string[] { "ADD", fullPath, "/v", name, "/d", value.ToString(), "/t", _valueKindName[valueKind], CreateViewOption(), "/f" });
        }
        System.Diagnostics.Debug.WriteLine(stdout);
    }

    private Microsoft.Win32.RegistryValueKind CalculateValueKind<T>(T value)
    {
        Type type = typeof(T);

        if (type.IsAssignableTo(typeof(IReadOnlyList<byte>))) return Microsoft.Win32.RegistryValueKind.Binary;
        if (type == typeof(uint)) return Microsoft.Win32.RegistryValueKind.DWord;
        if (type == typeof(ulong)) return Microsoft.Win32.RegistryValueKind.QWord;
        if (type.IsAssignableTo(typeof(IReadOnlyList<string>))) return Microsoft.Win32.RegistryValueKind.MultiString;

        return Microsoft.Win32.RegistryValueKind.String;
    }

    private static void ValidateValueKind<T>(Microsoft.Win32.RegistryValueKind valueKind)
    {
        Type type = typeof(T);

        switch (valueKind)
        {
            case Microsoft.Win32.RegistryValueKind.Binary:
                if (!type.IsAssignableTo(typeof(IReadOnlyList<byte>))) throw new ArgumentException();
                break;
            case Microsoft.Win32.RegistryValueKind.DWord:
                if (type != typeof(uint)) throw new ArgumentException();
                break;
            case Microsoft.Win32.RegistryValueKind.QWord:
                if (type != typeof(ulong)) throw new ArgumentException();
                break;
            case Microsoft.Win32.RegistryValueKind.MultiString:
                if (!type.IsAssignableTo(typeof(IReadOnlyList<string>))) throw new ArgumentException();
                break;
            case Microsoft.Win32.RegistryValueKind.String:
            case Microsoft.Win32.RegistryValueKind.ExpandString:
                if (type != typeof(string)) throw new ArgumentException();
                break;
            default:
                throw new ArgumentException("Microsoft.Win32.RegistryValueKind.Unknown");
        }
    }

    private string GenerateFullPath(string keyName = "")
    {
        var fullPath = new StringBuilder();

        if (!String.IsNullOrEmpty(_computerName)) fullPath.Append("\\\\").Append(_computerName).Append("\\");
        fullPath.Append(_hiveShortName[_hive]);
        if (!String.IsNullOrEmpty(keyName)) fullPath.Append("\\").Append(keyName);

        return fullPath.ToString();
    }

    private string GenerateFullName(string subkey = "")
    {
        if (String.IsNullOrEmpty(subkey)) return _keyName;

        var fullName = new StringBuilder(_keyName);
        fullName.Append("\\").Append(subkey);

        FixupName(fullName);

        return fullName.ToString();
    }

    private static void FixupName(StringBuilder name)
    {
        Debug.Assert(name != null, "[FixupName]name!=null");

        var nameStr = name.ToString();

        if (!nameStr.Contains('\\')) return;

        FixupPath(name);

        // Remove trailing slash
        int temp = name.Length - 1;
        if (temp >= 0 && name[temp] == '\\')
        {
            name.Length = temp;
        }

        // Remove leading slash
        if (name.Length > 0 && name[0] == '\\')
        {
            name.Remove(0, 1);
        }
    }

    private static void FixupPath(StringBuilder path)
    {
        Debug.Assert(path != null);

        int length = path.Length;
        bool fixup = false;
        char markerChar = (char)0xFFFF;

        int i = 1;
        while (i < length - 1)
        {
            if (path[i] == '\\')
            {
                i++;
                while (i < length && path[i] == '\\')
                {
                    path[i] = markerChar;
                    i++;
                    fixup = true;
                }
            }
            i++;
        }

        if (fixup)
        {
            i = 0;
            int j = 0;
            while (i < length)
            {
                if (path[i] == markerChar)
                {
                    i++;
                    continue;
                }
                path[j] = path[i];
                i++;
                j++;
            }
            path.Length += j - i;
        }
    }

    private void EnsureWriteable()
    {
        if (!_writable) throw new UnauthorizedAccessException("not writable.");
    }

    private async Task EnsureKeyExistanceAsync(string fullPath)
    {
        await _regCmd.ExecuteAsync(new string[] { "QUERY", fullPath, CreateViewOption() });
    }

    private async Task EnsureValueExistanceAsync(string name)
    {
        var fullPath = GenerateFullPath(GenerateFullName());
        await _regCmd.ExecuteAsync(new string[] { "QUERY", fullPath, "/v", name, CreateViewOption() });
    }

    private async Task<bool> IsExistSubkeyAsync(string subkey)
    {
        var fullPath = GenerateFullPath(GenerateFullName(subkey));

        try
        {
            await _regCmd.ExecuteAsync(new string[] { "QUERY", fullPath, CreateViewOption() });
            return true;
        }
        catch (CommandExecutionException)
        {
            return false;
        }
    }

    private static void ValidateKeyName(string fullPath)
    {
        ArgumentNullException.ThrowIfNull(fullPath);

        int nextSlash = fullPath.IndexOf('\\');
        int current = 0;
        while (nextSlash >= 0)
        {
            if ((nextSlash - current) > MaxKeyLength)
            {
                throw new ArgumentException("SR.Arg_RegKeyStrLenBug");
            }
            current = nextSlash + 1;
            nextSlash = fullPath.IndexOf('\\', current);
        }

        if ((fullPath.Length - current) > MaxKeyLength)
        {
            throw new ArgumentException("SR.Arg_RegKeyStrLenBug");
        }
    }

    private static void ValidateValueName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (name.IndexOf('\\') >= 0) throw new ArgumentException("name contains \\");
        if (name.Length > MaxKeyLength) throw new ArgumentException("SR.Arg_RegKeyStrLenBug");
    }

    private static void ValidateKeyView(Microsoft.Win32.RegistryView view)
    {
        if (view != Microsoft.Win32.RegistryView.Default && view != Microsoft.Win32.RegistryView.Registry32 && view != Microsoft.Win32.RegistryView.Registry64)
        {
            throw new ArgumentException("SR.Argument_InvalidRegistryViewCheck", nameof(view));
        }
    }
}

