#nullable enable

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace FluentAvalonia.UI.Controls;

public class AutoCompleteDropdownBehaviour : Behavior<AutoCompleteBox>
{
    private Popup? _popup;
    private TopLevel? _topLevel;

    protected override void OnAttached()
    {
        if (AssociatedObject is null)
            return;

        AssociatedObject.ApplyTemplate();
        AssociatedObject.TemplateApplied += OnTemplateApplied;
        AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += OnDetachedFromVisualTree;

        AssociatedObject.KeyUp += OnKeyUp;
        AssociatedObject.DropDownOpening += DropDownOpening;
        AssociatedObject.LostFocus += OnLostFocus;

        base.OnAttached();
    }

    private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (AssociatedObject is null)
            return;

        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_popup is not null)
        {
            _popup.IsLightDismissEnabled = false;
        }
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _topLevel = TopLevel.GetTopLevel(AssociatedObject);
        if (_topLevel is not null)
        {
            _topLevel.AddHandler(InputElement.PointerPressedEvent, OnGlobalPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(InputElement.PointerPressedEvent, OnGlobalPointerPressed);
        _topLevel = null;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.TemplateApplied -= OnTemplateApplied;
            AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
            AssociatedObject.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            AssociatedObject.KeyUp -= OnKeyUp;
            AssociatedObject.DropDownOpening -= DropDownOpening;
            AssociatedObject.LostFocus -= OnLostFocus;
        }

        _topLevel?.RemoveHandler(InputElement.PointerPressedEvent, OnGlobalPointerPressed);
        _topLevel = null;
        _popup = null;
        base.OnDetaching();
    }

    private void OnGlobalPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject is null || !AssociatedObject.IsDropDownOpen)
            return;

        if (e.Source is not Visual sourceVisual)
            return;

        if (sourceVisual == AssociatedObject || AssociatedObject.IsVisualAncestorOf(sourceVisual))
            return;

        if (_popup?.Child is Visual popupContent && popupContent.IsVisualAncestorOf(sourceVisual))
            return;

        AssociatedObject.SetCurrentValue(AutoCompleteBox.IsDropDownOpenProperty, false);
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject is null || !AssociatedObject.IsDropDownOpen)
            return;

        var focused = TopLevel.GetTopLevel(AssociatedObject)?.FocusManager?.GetFocusedElement() as Visual;
        if (focused is not null && _popup?.Child is Visual popupContent && popupContent.IsVisualAncestorOf(focused))
            return;

        AssociatedObject.SetCurrentValue(AutoCompleteBox.IsDropDownOpenProperty, false);
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if ((e.Key == Key.Down || e.Key == Key.F4))
        {
            if (string.IsNullOrEmpty(AssociatedObject?.Text))
            {
                ShowDropdown();
            }
        }
    }

    private void DropDownOpening(object? sender, CancelEventArgs e)
    {
        var prop = AssociatedObject!.GetType().GetProperty("TextBox", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var tb = (TextBox?)prop?.GetValue(AssociatedObject);
        if (tb is not null && tb.IsReadOnly)
        {
            e.Cancel = true;
        }
    }

    private void ShowDropdown()
    {
        if (AssociatedObject is not null && !AssociatedObject.IsDropDownOpen)
        {
            typeof(AutoCompleteBox).GetMethod("PopulateDropDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(AssociatedObject, new object[] { AssociatedObject, EventArgs.Empty });
            typeof(AutoCompleteBox).GetMethod("OpeningDropDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(AssociatedObject, new object[] { false });

            if (!AssociatedObject.IsDropDownOpen)
            {
                var ipc = typeof(AutoCompleteBox).GetField("_ignorePropertyChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var ignoreChange = ipc?.GetValue(AssociatedObject) as bool?;
                if (ignoreChange == false)
                    ipc?.SetValue(AssociatedObject, true);

                AssociatedObject.SetCurrentValue<bool>(AutoCompleteBox.IsDropDownOpenProperty, true);
            }
        }
    }
}
