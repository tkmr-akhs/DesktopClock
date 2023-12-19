using System.Drawing;
using DesktopClock.Models;

namespace DesktopClock.Contracts.Services;

public interface IScreenChangeDetectionService
{
    IReadOnlyList<Rectangle> ScreenBounds { get; }

    event ScreenChangedEventHandler? ScreenChanged;

    Task InitializeAsync();
}
