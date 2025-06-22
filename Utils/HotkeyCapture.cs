using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TransInputMethod.Utils
{
    public class HotkeyCapture
    {
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        private static extern int GetKeyNameText(int lParam, StringBuilder lpString, int nSize);

        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(int uCode, int uMapType);

        public static string GetHotkeyString(Keys key)
        {
            var parts = new List<string>();

            // Check modifier keys
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                parts.Add("Ctrl");
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                parts.Add("Alt");
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                parts.Add("Shift");

            // Get the main key name
            var keyName = GetKeyDisplayName(key);
            if (!string.IsNullOrEmpty(keyName))
                parts.Add(keyName);

            return string.Join("+", parts);
        }

        public static string GetKeyDisplayName(Keys key)
        {
            // Handle special keys first
            switch (key)
            {
                case Keys.Enter:
                    return "Enter";
                case Keys.Space:
                    return "Space";
                case Keys.Tab:
                    return "Tab";
                case Keys.Escape:
                    return "Esc";
                case Keys.Back:
                    return "Backspace";
                case Keys.Delete:
                    return "Delete";
                case Keys.Insert:
                    return "Insert";
                case Keys.Home:
                    return "Home";
                case Keys.End:
                    return "End";
                case Keys.PageUp:
                    return "PageUp";
                case Keys.PageDown:
                    return "PageDown";
                case Keys.Up:
                    return "↑";
                case Keys.Down:
                    return "↓";
                case Keys.Left:
                    return "←";
                case Keys.Right:
                    return "→";
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                    return key.ToString();
                case Keys.None:
                    return "";
                default:
                    // For other keys, try to get the display name
                    var keyCode = (int)key;
                    var scanCode = MapVirtualKey(keyCode, 0);
                    var sb = new StringBuilder(256);
                    
                    if (GetKeyNameText(scanCode << 16, sb, 256) > 0)
                    {
                        return sb.ToString();
                    }
                    
                    // Fallback to the enum name
                    return key.ToString();
            }
        }

        public static GlobalHotkey.ModifierKeys GetCurrentModifiers()
        {
            var modifiers = GlobalHotkey.ModifierKeys.None;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                modifiers |= GlobalHotkey.ModifierKeys.Control;
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                modifiers |= GlobalHotkey.ModifierKeys.Alt;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                modifiers |= GlobalHotkey.ModifierKeys.Shift;

            return modifiers;
        }

        public static bool IsModifierKey(Keys key)
        {
            return key == Keys.ControlKey || key == Keys.ShiftKey || 
                   key == Keys.Menu || key == Keys.LWin || key == Keys.RWin ||
                   key == Keys.LControlKey || key == Keys.RControlKey ||
                   key == Keys.LShiftKey || key == Keys.RShiftKey ||
                   key == Keys.LMenu || key == Keys.RMenu;
        }
    }

    public class HotkeyTextBox : TextBox
    {
        public event EventHandler<HotkeyChangedEventArgs>? HotkeyChanged;

        private GlobalHotkey.ModifierKeys _modifiers = GlobalHotkey.ModifierKeys.None;
        private Keys _key = Keys.None;

        public HotkeyTextBox()
        {
            this.ReadOnly = true;
            this.BackColor = Color.White;
            this.KeyDown += HotkeyTextBox_KeyDown;
            this.KeyUp += HotkeyTextBox_KeyUp;
        }

        public void SetHotkey(GlobalHotkey.ModifierKeys modifiers, Keys key)
        {
            _modifiers = modifiers;
            _key = key;
            UpdateDisplay();
        }

        public (GlobalHotkey.ModifierKeys Modifiers, Keys Key) GetHotkey()
        {
            return (_modifiers, _key);
        }

        private void HotkeyTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;

            // Don't capture modifier keys alone
            if (HotkeyCapture.IsModifierKey(e.KeyCode))
                return;

            _modifiers = HotkeyCapture.GetCurrentModifiers();
            _key = e.KeyCode;

            UpdateDisplay();
            HotkeyChanged?.Invoke(this, new HotkeyChangedEventArgs(_modifiers, _key));
        }

        private void HotkeyTextBox_KeyUp(object? sender, KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void UpdateDisplay()
        {
            var parts = new List<string>();

            if ((_modifiers & GlobalHotkey.ModifierKeys.Control) != 0)
                parts.Add("Ctrl");
            if ((_modifiers & GlobalHotkey.ModifierKeys.Alt) != 0)
                parts.Add("Alt");
            if ((_modifiers & GlobalHotkey.ModifierKeys.Shift) != 0)
                parts.Add("Shift");
            if ((_modifiers & GlobalHotkey.ModifierKeys.Win) != 0)
                parts.Add("Win");

            if (_key != Keys.None)
            {
                var keyName = HotkeyCapture.GetKeyDisplayName(_key);
                if (!string.IsNullOrEmpty(keyName))
                    parts.Add(keyName);
            }

            this.Text = string.Join("+", parts);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.BackColor = Color.LightYellow;
            this.Text = "请按下组合键...";
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.BackColor = Color.White;
            UpdateDisplay();
        }
    }

    public class HotkeyChangedEventArgs : EventArgs
    {
        public GlobalHotkey.ModifierKeys Modifiers { get; }
        public Keys Key { get; }

        public HotkeyChangedEventArgs(GlobalHotkey.ModifierKeys modifiers, Keys key)
        {
            Modifiers = modifiers;
            Key = key;
        }
    }
}