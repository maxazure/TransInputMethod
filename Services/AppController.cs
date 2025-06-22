using TransInputMethod.Data;
using TransInputMethod.Forms;
using TransInputMethod.Utils;
using System.Windows.Forms;

namespace TransInputMethod.Services
{
    public class AppController : ApplicationContext
    {
        private readonly ConfigService _configService;
        private readonly TranslationService _translationService;
        private readonly TranslationDbContext _dbContext;
        private readonly GlobalHotkey _globalHotkey;
        
        private FloatingTranslationForm? _floatingForm;
        private NotifyIcon? _notifyIcon;
        private Form _hiddenForm; // Hidden form to provide window handle
        
        private int _showWindowHotkeyId;
        private int _translateHotkeyId;

        public AppController()
        {
            _configService = new ConfigService();
            _translationService = new TranslationService(_configService);
            _dbContext = new TranslationDbContext();
            
            // Create hidden form to provide window handle for global hotkeys
            _hiddenForm = new Form
            {
                WindowState = FormWindowState.Minimized,
                ShowInTaskbar = false,
                Visible = false
            };
            _hiddenForm.CreateControl(); // Force handle creation
            
            _globalHotkey = new GlobalHotkey(_hiddenForm.Handle);

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                SetupSystemTray();
                await SetupGlobalHotkeys();
                CreateFloatingForm();
                
                _globalHotkey.HotkeyPressed += OnHotkeyPressed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitApplication();
            }
        }

        private void SetupSystemTray()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateApplicationIcon(),
                Text = "浮动翻译输入法",
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("显示翻译窗口", null, (s, e) => ShowFloatingForm());
            contextMenu.Items.Add("历史记录", null, (s, e) => ShowHistoryForm());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("设置", null, (s, e) => ShowSettingsForm());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("退出", null, (s, e) => ExitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => ShowFloatingForm();
        }

        private System.Drawing.Icon CreateApplicationIcon()
        {
            // Create a simple icon (you can replace this with a proper icon file)
            var bitmap = new System.Drawing.Bitmap(16, 16);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.Blue);
                g.FillEllipse(System.Drawing.Brushes.White, 2, 2, 12, 12);
                g.DrawString("T", new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold), 
                    System.Drawing.Brushes.Blue, 4, 2);
            }
            
            return System.Drawing.Icon.FromHandle(bitmap.GetHicon());
        }

        private async Task SetupGlobalHotkeys()
        {
            try
            {
                var config = await _configService.GetConfigAsync();
                
                // Parse and register show window hotkey
                var showModifiers = GlobalHotkey.ParseModifierKeys(config.Hotkeys.ShowWindowHotkey);
                var showKey = GlobalHotkey.ParseKey(config.Hotkeys.ShowWindowHotkey);
                
                if (showKey != Keys.None)
                {
                    _showWindowHotkeyId = _globalHotkey.RegisterHotkey(showModifiers, showKey, "Show Translation Window");
                    System.Diagnostics.Debug.WriteLine($"注册显示窗口热键: {config.Hotkeys.ShowWindowHotkey} (ID: {_showWindowHotkeyId})");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"显示窗口热键解析失败: {config.Hotkeys.ShowWindowHotkey}");
                }

                // Parse and register translate hotkey (this will be handled by the floating form)
                var translateModifiers = GlobalHotkey.ParseModifierKeys(config.Hotkeys.TranslateHotkey);
                var translateKey = GlobalHotkey.ParseKey(config.Hotkeys.TranslateHotkey);
                
                if (translateKey != Keys.None)
                {
                    _translateHotkeyId = _globalHotkey.RegisterHotkey(translateModifiers, translateKey, "Translate Text");
                    System.Diagnostics.Debug.WriteLine($"注册翻译热键: {config.Hotkeys.TranslateHotkey} (ID: {_translateHotkeyId})");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"翻译热键解析失败: {config.Hotkeys.TranslateHotkey}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"热键设置失败: {ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Diagnostics.Debug.WriteLine($"热键设置异常: {ex}");
            }
        }

        private void CreateFloatingForm()
        {
            _floatingForm = new FloatingTranslationForm(_translationService, _configService, _dbContext);
        }

        private void OnHotkeyPressed(object? sender, HotkeyPressedEventArgs e)
        {
            if (e.HotkeyId == _showWindowHotkeyId)
            {
                ShowFloatingForm();
            }
            // The translate hotkey is handled by the floating form itself
        }

        private async void ShowFloatingForm()
        {
            try
            {
                if (_floatingForm == null)
                {
                    CreateFloatingForm();
                }

                if (_floatingForm != null)
                {
                    await _floatingForm.ShowAtCursor();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"显示翻译窗口失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowHistoryForm()
        {
            try
            {
                var historyForm = new HistoryForm(_dbContext);
                historyForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"显示历史记录失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ShowSettingsForm()
        {
            try
            {
                var settingsForm = new SettingsForm(_configService);
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Unregister old hotkeys first
                    _globalHotkey.UnregisterHotkey(_showWindowHotkeyId);
                    _globalHotkey.UnregisterHotkey(_translateHotkeyId);
                    
                    // Reload hotkeys with new configuration
                    await SetupGlobalHotkeys();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"显示设置窗口失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExitApplication()
        {
            try
            {
                _notifyIcon?.Dispose();
                _globalHotkey?.Dispose();
                _floatingForm?.Dispose();
                _translationService?.Dispose();
                _dbContext?.Dispose();
                _hiddenForm?.Dispose();
                
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"退出应用程序时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ExitApplication();
            }
            base.Dispose(disposing);
        }
    }
}