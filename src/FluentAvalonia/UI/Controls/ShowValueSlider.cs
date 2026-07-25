#nullable enable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace FluentAvalonia.UI.Controls;

public class ShowValueSlider : Slider
{
    public static readonly StyledProperty<bool> ShowValueProperty =
        AvaloniaProperty.Register<ShowValueSlider, bool>(nameof(ShowValue));

    public bool ShowValue
    {
        get => GetValue(ShowValueProperty);
        set => SetValue(ShowValueProperty, value);
    }

    private Thumb? _thumb;
    private Popup? _popup;
    private TextBlock? _popupText;
    private bool _isPressed;

    public ShowValueSlider()
    {
        AddHandler(PointerPressedEvent, OnSliderPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnSliderPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _thumb = e.NameScope.Find<Thumb>("thumb") ?? e.NameScope.Find<Thumb>("SliderThumb");

        if (_thumb != null && _popup is null)
        {
            _popupText = new TextBlock();

            var tip = new ToolTip
            {
                Content = _popupText
            };

            _popup = new Popup
            {
                PlacementTarget = _thumb,
                Placement = PlacementMode.Top,
                VerticalOffset = -8,
                IsLightDismissEnabled = false,
                Child = tip
            };

            ((ISetLogicalParent)_popup).SetParent(this);
        }
    }

    private void OnSliderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!ShowValue || _popup is null || _popupText is null)
            return;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _isPressed = true;
        _popupText.Text = Value.ToString("F0");
        _popup.IsOpen = true;
    }

    private void OnSliderPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPressed || _popup is null)
            return;

        _isPressed = false;
        _popup.IsOpen = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty && _isPressed && _popupText != null)
        {
            _popupText.Text = Value.ToString("F0");
        }
    }
}
