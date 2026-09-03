#nullable enable

using System.Collections;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace FluentAvalonia.UI.Controls;

public class AutoCompleteDropdownBehaviour : Behavior<AutoCompleteBox>
{
    private Popup? _popup;
    private SelectingItemsControl? _suggestionList;
    private TopLevel? _topLevel;
    private WindowBase? _window;

    private bool _pointerPressedOnControl;
    private bool _suppressNextAutoFocus;
    private bool _suppressDropDownOpening;

    protected override void OnAttached()
    {
        if (AssociatedObject is null)
            return;

        AssociatedObject.ApplyTemplate();
        AssociatedObject.TemplateApplied += OnTemplateApplied;
        AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += OnDetachedFromVisualTree;

        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnControlPointerPressed, RoutingStrategies.Tunnel);
        AssociatedObject.KeyUp += OnKeyUp;
        AssociatedObject.GotFocus += OnGotFocus;
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

        _suggestionList = e.NameScope.Find<SelectingItemsControl>("PART_SelectingItemsControl");
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _topLevel = TopLevel.GetTopLevel(AssociatedObject);
        if (_topLevel is not null)
        {
            _topLevel.AddHandler(InputElement.PointerPressedEvent, OnGlobalPointerPressed, RoutingStrategies.Tunnel);
        }

        if (_topLevel is WindowBase window)
        {
            _window = window;
            _window.Activated += OnWindowActivated;
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(InputElement.PointerPressedEvent, OnGlobalPointerPressed);
        _topLevel = null;

        if (_window is not null)
        {
            _window.Activated -= OnWindowActivated;
            _window = null;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.TemplateApplied -= OnTemplateApplied;
            AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
            AssociatedObject.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnControlPointerPressed);
            AssociatedObject.KeyUp -= OnKeyUp;
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.DropDownOpening -= DropDownOpening;
            AssociatedObject.LostFocus -= OnLostFocus;
        }

        _topLevel?.RemoveHandler(InputElement.PointerPressedEvent, OnGlobalPointerPressed);
        _topLevel = null;

        if (_window is not null)
        {
            _window.Activated -= OnWindowActivated;
            _window = null;
        }

        _popup = null;
        _suggestionList = null;
        base.OnDetaching();
    }

    private void OnControlPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _pointerPressedOnControl = true;
    }

    private void OnWindowActivated(object? sender, EventArgs e)
    {
        _suppressNextAutoFocus = true;
        _suppressDropDownOpening = true;

        Dispatcher.UIThread.Post(() =>
        {
            _suppressNextAutoFocus = false;
            _suppressDropDownOpening = false;
        }, DispatcherPriority.Input);
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
        _topLevel?.FocusManager?.Focus(null);
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

    private void OnGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (AssociatedObject is null)
            return;

        bool suppress = _suppressNextAutoFocus && !_pointerPressedOnControl;
        _suppressNextAutoFocus = false;
        _pointerPressedOnControl = false;

        if (suppress)
        {
            _topLevel?.FocusManager?.Focus(null);
            return;
        }

        if (AssociatedObject.IsDropDownOpen)
            return;

        ShowDropdown();
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down || e.Key == Key.F4)
        {
            if (string.IsNullOrEmpty(AssociatedObject?.Text))
            {
                ShowDropdown();
            }
        }
    }

    private void DropDownOpening(object? sender, CancelEventArgs e)
    {
        var prop = typeof(AutoCompleteBox).GetProperty("TextBox", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var tb = (TextBox?)prop?.GetValue(AssociatedObject);
        if (tb is not null && tb.IsReadOnly)
        {
            e.Cancel = true;
            return;
        }

        if (_suppressDropDownOpening)
        {
            e.Cancel = true;
            _suppressDropDownOpening = false;
            return;
        }

        if (!HasItems())
        {
            e.Cancel = true;
        }
    }

    private bool HasItems()
    {
        if (_suggestionList is not null && _suggestionList.ItemCount > 0)
            return true;

        if (AssociatedObject is not null)
        {
            var searchItemsProp = typeof(AutoCompleteBox).GetProperty("SearchItems", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (searchItemsProp?.GetValue(AssociatedObject) is IEnumerable searchItems)
            {
                return searchItems.Cast<object>().Any();
            }
        }

        return false;
    }

    private void ShowDropdown()
    {
        if (AssociatedObject is not null && !AssociatedObject.IsDropDownOpen)
        {
            typeof(AutoCompleteBox).GetMethod("PopulateDropDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(AssociatedObject, new object[] { AssociatedObject, EventArgs.Empty });
            typeof(AutoCompleteBox).GetMethod("OpeningDropDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(AssociatedObject, new object[] { false });

            if (!HasItems())
            {
                if (AssociatedObject.IsDropDownOpen)
                    AssociatedObject.SetCurrentValue(AutoCompleteBox.IsDropDownOpenProperty, false);

                return;
            }

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
