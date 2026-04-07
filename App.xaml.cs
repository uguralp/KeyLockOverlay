using System.Windows;

namespace KeyLockOverlay;

public partial class App : Application
{
    private OverlayWindow? _overlay;
    private KeyboardHook? _hook;
    private System.Windows.Forms.NotifyIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _overlay = new OverlayWindow();
        _hook = new KeyboardHook();
        _hook.KeyStateChanged += OnKeyStateChanged;
        _hook.Install();

        SetupTrayIcon();
    }

    private void OnKeyStateChanged(LockKey key, bool isOn)
    {
        _overlay?.ShowNotification(key, isOn);
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Text = "KeyLock Overlay",
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true
        };

        var menu = new System.Windows.Forms.ContextMenuStrip();
        var exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) =>
        {
            _trayIcon.Visible = false;
            _hook?.Uninstall();
            Shutdown();
        };
        menu.Items.Add(exitItem);
        _trayIcon.ContextMenuStrip = menu;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hook?.Uninstall();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
