#nullable enable
using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class TabItemHelper
{
    public static readonly AttachedProperty<object?> IconProperty =
        AvaloniaProperty.RegisterAttached<TabItem, object?>("Icon", typeof(TabItemHelper));

    public static object? GetIcon(TabItem element) => element.GetValue(IconProperty);

    public static void SetIcon(TabItem element, object? value) => element.SetValue(IconProperty, value);
}
