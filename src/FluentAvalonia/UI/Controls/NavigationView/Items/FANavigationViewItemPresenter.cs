using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls.Primitives;

/// <summary>
/// Represents the visual elements of a NavigationViewItem.
/// </summary>
public partial class FANavigationViewItemPresenter : ContentControl
{
    private Viewbox _iconBox;
    private Viewbox _customIconBox;

    public FANavigationViewItemPresenter()
    {
        TemplateSettings = new FANavigationViewItemPresenterTemplateSettings();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // HACK: Bug in Viewbox doesn't disconnect its child when its removed from the tree
        //       causing a crash when switching pane mode. Force disconnect the icon and 
        //       reapply it after applying the template.
        var ts = TemplateSettings;
        var icoSrc = IconSource;
        if (icoSrc != null)
        {
            ts.Icon = null;
        }

        base.OnApplyTemplate(e);

        _selectionIndicator = e.NameScope.Find<Border>(s_tpSelectionIndicator);
        _contentGrid = e.NameScope.Find<Panel>(s_tpPresenterContentRootGrid);
        _infoBadgePresenter = e.NameScope.Find<ContentPresenter>(s_tpInfoBadgePresenter);

        _iconBox = e.NameScope.Find<Viewbox>("IconBox");
        _customIconBox = e.NameScope.Find<Viewbox>("CustomIconBox");   // ← find the Viewbox

        UpdateCustomIconVisibility();

        var nvi = GetNVI;
        if (nvi != null)
        {
            _expandCollapseChevron = e.NameScope.Find<Panel>(s_tpExpandCollapseChevron);
            if (_expandCollapseChevron != null)
            {
                _expandCollapseChevron.Tapped += nvi.OnExpandCollapseChevronTapped;
            }
            nvi.UpdateVisualState();

            var navView = nvi.GetNavigationView;
            if (navView != null && navView.PaneDisplayMode != FANavigationViewPaneDisplayMode.Top)
            {
                UpdateCompactPaneLength(_compactPaneLengthValue, true);
            }
        }

        UpdateMargin();

        if (icoSrc != null)
        {
            ts.Icon = FAIconHelpers.CreateFromUnknown(icoSrc);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconSourceProperty)
        {
            TemplateSettings.Icon = FAIconHelpers.CreateFromUnknown(change.GetNewValue<FAIconSource>());
        }
        else if (change.Property == IconProperty)
        {
            UpdateCustomIconVisibility();
        }
    }

    private void UpdateCustomIconVisibility()
    {
        bool hasCustomIcon = Icon != null;
        if (_iconBox != null)
            _iconBox.IsVisible = !hasCustomIcon;
        if (_customIconBox != null)
            _customIconBox.IsVisible = hasCustomIcon;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcPressed, true);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased
            && e.InitialPressMouseButton == MouseButton.Left)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcPressed, false);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        PseudoClasses.Set(FASharedPseudoclasses.s_pcPressed, false);
    }

    internal void RotateExpandCollapseChevron(bool isExpanded)
    {
        PseudoClasses.Set(s_pcExpanded, isExpanded);
    }

    internal void UpdateContentLeftIndentation(double leftIndent)
    {
        _leftIndentation = leftIndent;
        UpdateMargin();
    }

    private void UpdateMargin()
    {
        if (_contentGrid != null)
        {
            var oldMargin = _contentGrid.Margin;
            _contentGrid.Margin = new Thickness(_leftIndentation, oldMargin.Top, oldMargin.Right, oldMargin.Bottom);
        }
    }

    internal void UpdateCompactPaneLength(double len, bool update)
    {
        _compactPaneLengthValue = len;

        if (update)
        {
            TemplateSettings.IconWidth = len;
            TemplateSettings.SmallerIconWidth = len - 8;
        }
    }

    internal void UpdateClosedCompactVisualState(bool topLevel, bool isClosedCompact)
    {
        // We increased the ContentPresenter margin to align it visually with the expand/collapse chevron. This updated margin is even applied when the
        // NavigationView is in a visual state where no expand/collapse chevrons are shown, leading to more content being cut off than necessary.
        // This is the case for top-level items when the NavigationView is in a compact mode and the NavigationView pane is closed. To keep the original
        // cutoff visual experience intact, we restore  the original ContentPresenter margin for such top-level items only (children shown in a flyout
        // will use the updated margin).

        //states :closedcompacttop, :notclosedcompacttop

        PseudoClasses.Set(s_pcClosedCompactTop, isClosedCompact && topLevel);
        PseudoClasses.Set(s_pcNotClosedCompactTop, !isClosedCompact && topLevel);
    }

    private Panel _contentGrid;
    private Panel _expandCollapseChevron;
    private Control _selectionIndicator;
    private ContentPresenter _infoBadgePresenter;
    private double _compactPaneLengthValue = 40;
    private double _leftIndentation;
}
