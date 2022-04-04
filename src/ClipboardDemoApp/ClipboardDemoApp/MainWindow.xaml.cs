using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ClipboardDemoApp;

public partial class MainWindow : Window
{
    private ClipboardHelper windowClipboardManager;

    public MainWindow()
    {
        InitializeComponent();
        windowClipboardManager = new ClipboardHelper();
        Loaded += LoadWin;
    }

    private void LoadWin(object sender, RoutedEventArgs e)
    {
        windowClipboardManager.RegisterForUpdatesFromWindow(this);
        windowClipboardManager.ClipboardChanged += ClipboardChanged;
        windowClipboardManager.TextAdded += TextAddedEvent;
    }

    private void TextAddedEvent(object? sender, string e) => lbData.Items.Add(e);

    private void ClipboardChanged(object? sender, EventArgs e)
    {
        if (Clipboard.ContainsText()) 
            Debug.WriteLine("Called from clipboard changed: " + Clipboard.GetText());
    }

    private void btnClipboard_Click(object sender, RoutedEventArgs e)
    {
        var text = Clipboard.GetText();
        lbData.Items.Add(text);
    }

    private void btnItemCleared_Click(object sender, RoutedEventArgs e) => lbData.Items.Clear();

    private void TbQuery_OnKeyDown(object sender, KeyEventArgs e)
    {
        var items = windowClipboardManager.Data;

        var list = items.Where(currentItem =>
            currentItem.Contains(tbQuery.Text)).ToList();

        if (string.IsNullOrEmpty(tbQuery.Text)) list = items;

        lbData.Items.Clear();
        foreach (var currentItem in list)
        {
            lbData.Items.Add(currentItem);
        }
    }
}