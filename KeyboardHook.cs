using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace KeyLockOverlay;

public enum LockKey { CapsLock, NumLock }

public class KeyboardHook
{
    public event Action<LockKey, bool>? KeyStateChanged;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYUP       = 0x0101;
    private const int WM_SYSKEYUP    = 0x0105;
    private const int VK_CAPITAL     = 0x14;
    private const int VK_NUMLOCK     = 0x90;

    private IntPtr _hookHandle = IntPtr.Zero;
    private readonly LowLevelKeyboardProc _proc;

    public KeyboardHook()
    {
        _proc = HookCallback; // keep reference alive
    }

    public void Install()
    {
        using var module = System.Diagnostics.Process.GetCurrentProcess().MainModule!;
        _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
            GetModuleHandle(module.ModuleName!), 0);
    }

    public void Uninstall()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
        {
            var vkCode = Marshal.ReadInt32(lParam);

            if (vkCode == VK_CAPITAL)
            {
                // State is toggled BEFORE keyup in Windows — read current state
                bool isOn = Keyboard.IsKeyToggled(Key.CapsLock);
                KeyStateChanged?.Invoke(LockKey.CapsLock, isOn);
            }
            else if (vkCode == VK_NUMLOCK)
            {
                bool isOn = Keyboard.IsKeyToggled(Key.NumLock);
                KeyStateChanged?.Invoke(LockKey.NumLock, isOn);
            }
        }
        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string lpModuleName);
}
