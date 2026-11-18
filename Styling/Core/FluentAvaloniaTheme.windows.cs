using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using FluentAvalonia.Interop;
using FluentAvalonia.Interop.Win32;

namespace FluentAvalonia.Styling;

public partial class FluentAvaloniaTheme
{
    [SupportedOSPlatform("windows")]
    private ThemeVariant ResolveWindowsSystemSettings(IPlatformSettings platformSettings)
    {
        ThemeVariant theme = null;
        if (PreferSystemTheme)
        {
            theme = GetThemeFromIPlatformSettings(platformSettings);
        }

        if (CustomAccentColor != null)
        {
            LoadCustomAccentColor();
        }
        else if (PreferUserAccentColor)
        {
            try
            {
                TryLoadWindowsAccentColor();
            }
            catch
            {
                Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                        .Log("FluentAvaloniaTheme", "Unable to read the Windows accent color");
                LoadDefaultAccentColor();
            }
        }
        else
        {
            LoadDefaultAccentColor();
        }

        if (UseSystemFontOnWindows)
        {
            try
            {
                if (OSVersionHelper.IsWindows11())
                {
                    AddOrUpdateSystemResource("ContentControlThemeFontFamily", new FontFamily("Segoe UI Variable"));
                }
                else
                {
                    AddOrUpdateSystemResource("ContentControlThemeFontFamily", new FontFamily("Segoe UI"));
                }
            }
            catch
            {
                Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                    .Log("FluentAvaloniaTheme", "Error in detecting Windows system font.");

                AddOrUpdateSystemResource("ContentControlThemeFontFamily", FontFamily.Default);
            }
        }

        return theme;
    }

    private void TryLoadHighContrastThemeColors()
    {
        void TryAddResource(string resKey, SYS_COLOR_INDEX systemColor)
        {
            try
            {
                var color = Win32Interop.GetSystemColor(systemColor);
                var res = Resources.MergedDictionaries[0] as ResourceDictionary;
                (res.ThemeDictionaries[HighContrastTheme] as ResourceDictionary)[resKey] = color;
            }
            catch
            {
                Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                .Log("FluentAvaloniaTheme", $"Loading high contrast theme resources failed. Unable to load {resKey} resource");
            }
        }

        TryAddResource("SystemColorWindowTextColor", SYS_COLOR_INDEX.COLOR_WINDOWTEXT);
        TryAddResource("SystemColorGrayTextColor", SYS_COLOR_INDEX.COLOR_GRAYTEXT);
        TryAddResource("SystemColorButtonFaceColor", SYS_COLOR_INDEX.COLOR_BTNFACE);
        TryAddResource("SystemColorWindowColor", SYS_COLOR_INDEX.COLOR_WINDOW);
        TryAddResource("SystemColorButtonTextColor", SYS_COLOR_INDEX.COLOR_BTNTEXT);
        TryAddResource("SystemColorHighlightColor", SYS_COLOR_INDEX.COLOR_HIGHLIGHT);
        TryAddResource("SystemColorHighlightTextColor", SYS_COLOR_INDEX.COLOR_HIGHLIGHTTEXT);
        TryAddResource("SystemColorHotlightColor", SYS_COLOR_INDEX.COLOR_HOTLIGHT);
    }

    [SupportedOSPlatform("windows")]
    private void TryLoadWindowsAccentColor()
    {
        try
        {
            // Prefer the exact shade ramp the shell uses (the same values WinRT's
            // IUISettings3 exposes); fall back to one derived locally if the palette
            // isn't present.
            if (Win32Interop.TryGetAccentPalette(out var accent,
                    out var light1, out var light2, out var light3,
                    out var dark1, out var dark2, out var dark3))
            {
                UpdateAccentColors(accent, light1, light2, light3, dark1, dark2, dark3);
                return;
            }

            if (Win32Interop.TryGetAccentColor(out accent))
            {
                UpdateAccentColorsFromBase(accent);
                return;
            }

            LoadDefaultAccentColor();
        }
        catch
        {
            Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                .Log("FluentAvaloniaTheme", "Loading system accent color failed, using fallback (SlateBlue)");

            // We don't know where it failed, so override all
            LoadDefaultAccentColor();
        }
    }

    /// <summary>
    /// On Windows, forces a specific <see cref="Window"/> to the current theme
    /// </summary>
    /// <param name="window">The window to force</param>
    /// <param name="theme">The theme to use, or null to use the current RequestedTheme</param>
    /// <exception cref="ArgumentNullException">If window is null</exception>
    public void ForceWin32WindowToTheme(Window window, ThemeVariant theme = null)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            Win32Interop.ApplyTheme(window.TryGetPlatformHandle().Handle, theme == ThemeVariant.Dark);
        }
        catch
        {
            Logger.TryGet(LogEventLevel.Information, "FluentAvaloniaTheme")?
                        .Log("FluentAvaloniaTheme", "Unable to set window to theme.");
        }
    }
}
