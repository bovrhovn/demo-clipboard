using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ClipboardDemoApp;

/// <summary>
///  clipboard native methods
/// </summary>
internal static class ClipboardNativeMethods
{
    /// <summary>
    /// notification about when content of the clipboards have changed
    /// </summary>
    /// <example>
    ///    #define WM_CLIPBOARDUPDATE              0x031D     
    /// </example>
    /// <seealso cref="https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-clipboardupdate"/>
    public const int WM_CLIPBOARDUPDATE = 0x031D;

    /// <summary>
    /// message only windows 
    /// </summary>
    /// <see cref="https://docs.microsoft.com/en-us/windows/win32/winmsg/window-features#message-only-windows"/>
    public static IntPtr HWND_MESSAGE = new(-3);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AddClipboardFormatListener(IntPtr hwnd);
    // [DllImport("User32.dll", SetLastError = true)]
    // static extern IntPtr GetClipboardData(uint uFormat);
    // [DllImport("user32.dll", SetLastError = true)]
    // [return: MarshalAs(UnmanagedType.Bool)]
    // static extern bool OpenClipboard(IntPtr hWndNewOwner);
    // [DllImport("User32")]
    // internal static extern bool CloseClipboard();
    // [DllImport("User32")]
    // internal static extern bool EmptyClipboard();
    // [DllImport("User32")]
    // internal static extern bool IsClipboardFormatAvailable(int format);
    // [DllImport("User32")]
    // internal static extern IntPtr GetClipboardData(int uFormat);
    // [DllImport("User32", CharSet = CharSet.Unicode)]
    // internal static extern IntPtr SetClipboardData(int uFormat, IntPtr hMem);
}

/// <summary>
/// code to help with clipboard actions
/// </summary>
///<remarks>
///   inspired by https://stackoverflow.com/questions/621577/how-do-i-monitor-clipboard-changes-in-c
/// </remarks>
/// <seealso cref="https://stackoverflow.com/questions/621577/how-do-i-monitor-clipboard-changes-in-c"/>
public class ClipboardHelper
{
    public event EventHandler? ClipboardChanged;
    public event EventHandler<string>? TextAdded;
    private readonly List<string> items = new();

    public void RegisterForUpdatesFromWindow(Window windowSource)
    {
        if (PresentationSource.FromVisual(windowSource) is not HwndSource source)
            throw new ArgumentException("Windows is not initialized", nameof(windowSource));

        source.AddHook(ReactOnClipboardChangeNotificationEvent);

        // get window handle for interop
        var windowHandle = new WindowInteropHelper(windowSource).Handle;

        // register for clipboard events
        ClipboardNativeMethods.AddClipboardFormatListener(windowHandle);

        if (Clipboard.ContainsText())
            items.Add(Clipboard.GetText());
    }

    private void OnClipboardChanged()
    {
        if (Clipboard.ContainsText())
        {
            var text = Clipboard.GetText();
            TextAdded?.Invoke(this, text);
            items.Add(text);
        }

        ClipboardChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<string> Data => items;

    private static readonly IntPtr WndProcSuccess = IntPtr.Zero;

    private IntPtr ReactOnClipboardChangeNotificationEvent(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam,
        ref bool handled)
    {
        if (msg == ClipboardNativeMethods.WM_CLIPBOARDUPDATE)
        {
            OnClipboardChanged();
            handled = true;
        }

        return WndProcSuccess;
    }
}