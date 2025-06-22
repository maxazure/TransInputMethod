using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TransInputMethod.Utils
{
    public class GlobalHotkey : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [Flags]
        public enum ModifierKeys : uint
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }

        private readonly Dictionary<int, HotkeyInfo> _hotkeys = new Dictionary<int, HotkeyInfo>();
        private readonly IntPtr _windowHandle;
        private int _currentId = 0;
        private bool _disposed = false;

        // Low-level keyboard hook
        private LowLevelKeyboardProc _keyboardProc;
        private IntPtr _keyboardHook = IntPtr.Zero;
        private readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>();

        public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

        public GlobalHotkey(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _keyboardProc = HookCallback;
            SetKeyboardHook();
        }

        private void SetKeyboardHook()
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                if (curModule != null)
                {
                    _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var isKeyDown = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;
                var isKeyUp = wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP;

                if (isKeyDown || isKeyUp)
                {
                    var vkCode = (Keys)Marshal.ReadInt32(lParam);
                    
                    if (isKeyDown)
                    {
                        _pressedKeys.Add(vkCode);
                        CheckHotkeyPressed();
                    }
                    else if (isKeyUp)
                    {
                        _pressedKeys.Remove(vkCode);
                    }
                }
            }

            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        private void CheckHotkeyPressed()
        {
            foreach (var hotkey in _hotkeys.Values)
            {
                if (IsHotkeyPressed(hotkey))
                {
                    HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(hotkey.Id, hotkey.Key));
                    break; // Only trigger one hotkey at a time
                }
            }
        }

        private bool IsHotkeyPressed(HotkeyInfo hotkey)
        {
            // Check if all required modifier keys are pressed
            var requiredModifiers = new List<Keys>();
            
            if ((hotkey.Modifiers & ModifierKeys.Control) != 0)
                requiredModifiers.Add(Keys.ControlKey);
            if ((hotkey.Modifiers & ModifierKeys.Shift) != 0)
                requiredModifiers.Add(Keys.ShiftKey);
            if ((hotkey.Modifiers & ModifierKeys.Alt) != 0)
                requiredModifiers.Add(Keys.Menu);
            if ((hotkey.Modifiers & ModifierKeys.Win) != 0)
                requiredModifiers.Add(Keys.LWin);

            // Check if the main key and all modifiers are pressed
            var allPressed = _pressedKeys.Contains(hotkey.Key) && 
                           requiredModifiers.All(key => _pressedKeys.Contains(key));

            // Also check if no extra modifier keys are pressed
            var extraModifiers = _pressedKeys.Where(key => 
                key == Keys.ControlKey || key == Keys.ShiftKey || 
                key == Keys.Menu || key == Keys.LWin || key == Keys.RWin)
                .Except(requiredModifiers);

            return allPressed && !extraModifiers.Any();
        }

        public int RegisterHotkey(ModifierKeys modifiers, Keys key, string description = "")
        {
            var id = ++_currentId;
            var hotkeyInfo = new HotkeyInfo(id, modifiers, key, description);
            _hotkeys[id] = hotkeyInfo;
            return id;
        }

        public bool UnregisterHotkey(int id)
        {
            if (_hotkeys.ContainsKey(id))
            {
                _hotkeys.Remove(id);
                return true;
            }
            return false;
        }

        public static ModifierKeys ParseModifierKeys(string modifierString)
        {
            var result = ModifierKeys.None;
            var parts = modifierString.Split('+', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                switch (part.Trim().ToLower())
                {
                    case "ctrl":
                    case "control":
                        result |= ModifierKeys.Control;
                        break;
                    case "shift":
                        result |= ModifierKeys.Shift;
                        break;
                    case "alt":
                        result |= ModifierKeys.Alt;
                        break;
                    case "win":
                    case "windows":
                        result |= ModifierKeys.Win;
                        break;
                }
            }
            
            return result;
        }

        public static Keys ParseKey(string keyString)
        {
            var parts = keyString.Split('+', StringSplitOptions.RemoveEmptyEntries);
            var keyPart = parts.LastOrDefault()?.Trim();
            
            if (string.IsNullOrEmpty(keyPart))
                return Keys.None;

            // Handle special cases
            switch (keyPart.ToLower())
            {
                case "enter":
                case "return":
                    return Keys.Enter;
                case "space":
                    return Keys.Space;
                case "tab":
                    return Keys.Tab;
                case "esc":
                case "escape":
                    return Keys.Escape;
                default:
                    if (Enum.TryParse<Keys>(keyPart, true, out var result))
                        return result;
                    break;
            }

            return Keys.None;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _hotkeys.Clear();
                
                if (_keyboardHook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_keyboardHook);
                    _keyboardHook = IntPtr.Zero;
                }
                
                _disposed = true;
            }
        }
    }

    public class HotkeyInfo
    {
        public int Id { get; }
        public GlobalHotkey.ModifierKeys Modifiers { get; }
        public Keys Key { get; }
        public string Description { get; }

        public HotkeyInfo(int id, GlobalHotkey.ModifierKeys modifiers, Keys key, string description)
        {
            Id = id;
            Modifiers = modifiers;
            Key = key;
            Description = description;
        }
    }

    public class HotkeyPressedEventArgs : EventArgs
    {
        public int HotkeyId { get; }
        public Keys Key { get; }

        public HotkeyPressedEventArgs(int hotkeyId, Keys key)
        {
            HotkeyId = hotkeyId;
            Key = key;
        }
    }
}