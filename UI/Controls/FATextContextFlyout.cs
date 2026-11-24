using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Lightweight Cut/Copy/Paste context flyout for <see cref="TextBox"/> and
/// <see cref="SelectableTextBlock"/>. Replaces FATextCommandBarFlyout so the
/// whole CommandBar/CommandBarFlyout stack doesn't need to be shipped.
/// </summary>
public class FATextContextFlyout : MenuFlyout
{
    protected override void OnOpening(System.ComponentModel.CancelEventArgs args)
    {
        Items.Clear();

        var hotkeys = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration;

        switch (Target)
        {
            case TextBox tb:
                bool editable = !tb.IsReadOnly;
                bool masked = tb.PasswordChar != default && !tb.RevealPassword;

                if (editable && !masked)
                    Add("Cut", FASymbol.Cut, hotkeys?.Cut, tb.CanCut, tb.Cut);
                if (!masked)
                    Add("Copy", FASymbol.Copy, hotkeys?.Copy, tb.CanCopy, tb.Copy);
                if (editable)
                    Add("Paste", FASymbol.Paste, hotkeys?.Paste, tb.CanPaste, tb.Paste);
                if (editable && (tb.CanUndo || tb.CanRedo))
                {
                    Items.Add(new Separator());
                    Add("Undo", FASymbol.Undo, hotkeys?.Undo, tb.CanUndo, tb.Undo);
                    Add("Redo", FASymbol.Redo, hotkeys?.Redo, tb.CanRedo, tb.Redo);
                }
                Items.Add(new Separator());
                Add("Select All", FASymbol.SelectAll, hotkeys?.SelectAll, !string.IsNullOrEmpty(tb.Text), tb.SelectAll);
                break;

            case SelectableTextBlock stb:
                Add("Copy", FASymbol.Copy, hotkeys?.Copy, stb.SelectionStart != stb.SelectionEnd, stb.Copy);
                Add("Select All", FASymbol.SelectAll, hotkeys?.SelectAll, true, stb.SelectAll);
                break;

            default:
                args.Cancel = true;
                return;
        }

        base.OnOpening(args);
    }

    private void Add(string header, FASymbol symbol, List<KeyGesture> gestures, bool enabled, Action execute)
    {
        var item = new MenuItem
        {
            Header = header,
            Icon = new FASymbolIcon { Symbol = symbol },
            InputGesture = gestures is { Count: > 0 } ? gestures[0] : null,
            IsEnabled = enabled,
        };
        item.Click += (_, _) => execute();
        Items.Add(item);
    }
}
