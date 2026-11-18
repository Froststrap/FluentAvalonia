using Avalonia.Logging;
using Avalonia.Media;
using FluentAvalonia.Interop;
using FluentAvalonia.Interop.Win32;
using static FluentAvalonia.Interop.Win32Interop;

namespace FluentAvalonia.UI.Windowing;

internal class Win32AppWindowFeatures : IFAAppWindowPlatformFeatures
{
    public Win32AppWindowFeatures(FAAppWindow owner)
    {
        _owner = owner;
    }

    public unsafe void SetWindowBorderColor(Color color)
    {
        // This is only available on Windows 11 right now, but don't bring the whole app down
        // if called on Win 10
        if (OSVersionHelper.IsWindows11())
        {
            // DON'T USE .ToUint32, COLORREF has B & R components switched
            // and expects alpha to be 0 (it will return hr = Parameter not correct)
            COLORREF cr = ((uint)0 << 24) | ((uint)color.B << 16) | ((uint)color.G << 8) | (uint)color.R;
            var hr = (HRESULT)DwmSetWindowAttribute(_owner.TryGetPlatformHandle().Handle, DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR,
                &cr, sizeof(COLORREF));
            if (!hr.SUCCEEDED)
            {
                Logger.TryGet(LogEventLevel.Debug, "AppWindow")?
                    .Log("SetWindowBorderColor", "Failed to set the border color of the window with hr: {hr}", hr);
            }
        }
    }

    private FAAppWindow _owner;
}
