using System.Drawing;
using Forms = System.Windows.Forms;

namespace DesktopClock.Win32.Tray;

/// <summary>
/// Provides a Windows notification area icon by using Windows Forms.
/// </summary>
public sealed class WinFormsTrayIconService : ITrayIconService
{
    private readonly List<Image> _menuImages = new();
    private Forms.NotifyIcon? _notifyIcon;
    private Forms.ContextMenuStrip? _contextMenuStrip;
    private Icon? _icon;
    private bool _disposed;

    /// <inheritdoc />
    public void Initialize(TrayIconOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ThrowIfDisposed();
        DisposeManagedResources();

        _icon = new Icon(options.IconStream);
        _contextMenuStrip = new Forms.ContextMenuStrip();

        foreach (var item in options.MenuItems.Where(item => item.IsVisible))
        {
            _contextMenuStrip.Items.Add(CreateMenuItem(item));
        }

        _notifyIcon = new Forms.NotifyIcon
        {
            ContextMenuStrip = _contextMenuStrip,
            Icon = _icon,
            Text = options.TooltipText,
            Visible = true
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DisposeManagedResources();
        _disposed = true;
    }

    private Forms.ToolStripItem CreateMenuItem(TrayIconMenuItem item)
    {
        if (item.Kind == TrayIconMenuItemKind.Separator)
        {
            return new Forms.ToolStripSeparator();
        }

        var menuItem = new Forms.ToolStripMenuItem
        {
            Checked = item.IsChecked,
            Enabled = item.IsEnabled,
            Image = CreateMenuItemImage(item),
            Text = item.Text
        };

        if (item.Click is not null)
        {
            menuItem.Click += (_, _) => item.Click.Invoke(this, EventArgs.Empty);
        }

        foreach (var child in item.Children.Where(child => child.IsVisible))
        {
            menuItem.DropDownItems.Add(CreateMenuItem(child));
        }

        return menuItem;
    }

    private Image? CreateMenuItemImage(TrayIconMenuItem item)
    {
        if (item.ImageStreamFactory is null)
        {
            return null;
        }

        using var stream = item.ImageStreamFactory.Invoke();
        if (stream is null)
        {
            return null;
        }

        using var image = Image.FromStream(stream);
        var bitmap = new Bitmap(image);
        _menuImages.Add(bitmap);

        return bitmap;
    }

    private void DisposeManagedResources()
    {
        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        _contextMenuStrip?.Dispose();
        _contextMenuStrip = null;

        _icon?.Dispose();
        _icon = null;

        foreach (var image in _menuImages)
        {
            image.Dispose();
        }

        _menuImages.Clear();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(WinFormsTrayIconService));
        }
    }
}
