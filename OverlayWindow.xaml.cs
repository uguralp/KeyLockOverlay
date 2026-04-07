using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace KeyLockOverlay;

public partial class OverlayWindow : Window
{
    private System.Windows.Threading.DispatcherTimer? _hideTimer;
    private bool _isAnimating = false;

    // Theme colors
    private static readonly Color CapsOnColor  = Color.FromRgb(0xF4, 0xA2, 0x61); // warm orange
    private static readonly Color CapsOffColor = Color.FromRgb(0x8E, 0x8E, 0x93); // grey
    private static readonly Color NumOnColor   = Color.FromRgb(0x4C, 0xC9, 0xF0); // sky blue
    private static readonly Color NumOffColor  = Color.FromRgb(0x8E, 0x8E, 0x93); // grey

    public OverlayWindow()
    {
        InitializeComponent();
        PositionWindow();
    }

    private void PositionWindow()
    {
        var screen = SystemParameters.WorkArea;
        Left = screen.Right - Width - 20;
        Top = screen.Top + 20;
    }

    public void ShowNotification(LockKey key, bool isOn)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateContent(key, isOn);
            PositionWindow();
            FadeIn();
            ResetHideTimer();
        });
    }

    private void UpdateContent(LockKey key, bool isOn)
    {
        Color accent;
        string label, status, icon;

        if (key == LockKey.CapsLock)
        {
            accent = isOn ? CapsOnColor : CapsOffColor;
            label  = "CAPS LOCK";
            status = isOn ? "ON" : "OFF";
            icon   = "⇪";
        }
        else
        {
            accent = isOn ? NumOnColor : NumOffColor;
            label  = "NUM LOCK";
            status = isOn ? "ON" : "OFF";
            icon   = "⇭";
        }

        var brush = new SolidColorBrush(accent);
        AccentBar.Background  = brush;
        StatusText.Foreground = brush;
        IconText.Foreground   = brush;
        LabelText.Text        = label;
        StatusText.Text       = status;
        IconText.Text         = icon;
    }

    private void FadeIn()
    {
        if (_isAnimating) return;
        _isAnimating = true;

        Show();

        var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(160))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        anim.Completed += (_, _) => _isAnimating = false;
        BeginAnimation(OpacityProperty, anim);
    }

    private void FadeOut()
    {
        var anim = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(300))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        anim.Completed += (_, _) => Hide();
        BeginAnimation(OpacityProperty, anim);
    }

    private void ResetHideTimer()
    {
        _hideTimer?.Stop();
        _hideTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1600)
        };
        _hideTimer.Tick += (_, _) =>
        {
            _hideTimer.Stop();
            FadeOut();
        };
        _hideTimer.Start();
    }
}
