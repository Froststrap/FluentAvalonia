using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public static class ProgressBarHelper
{
    public static readonly AttachedProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.RegisterAttached<ProgressBar, CornerRadius>("CornerRadius", typeof(ProgressBarHelper));

    public static readonly AttachedProperty<CornerRadius> IndicatorCornerRadiusProperty =
        AvaloniaProperty.RegisterAttached<ProgressBar, CornerRadius>("IndicatorCornerRadius", typeof(ProgressBarHelper));

    public static void SetCornerRadius(AvaloniaObject element, CornerRadius value)
        => element.SetValue(CornerRadiusProperty, value);

    public static CornerRadius GetCornerRadius(AvaloniaObject element)
        => element.GetValue(CornerRadiusProperty);

    public static void SetIndicatorCornerRadius(AvaloniaObject element, CornerRadius value)
        => element.SetValue(IndicatorCornerRadiusProperty, value);

    public static CornerRadius GetIndicatorCornerRadius(AvaloniaObject element)
        => element.GetValue(IndicatorCornerRadiusProperty);
}