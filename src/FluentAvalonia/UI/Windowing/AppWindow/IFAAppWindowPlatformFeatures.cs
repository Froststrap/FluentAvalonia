using Avalonia.Media;

namespace FluentAvalonia.UI.Windowing;

/// <summary>
/// Provides a set of function that enable platform-specific functionality through AppWindow
/// These function are only available on Windows at the current time
/// </summary>
public interface IFAAppWindowPlatformFeatures
{
    /// <summary>
    /// Windows11 only, sets the border color of the current window to the specified color
    /// </summary>
    void SetWindowBorderColor(Color color);
}
