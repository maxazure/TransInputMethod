using TransInputMethod.Data;
using TransInputMethod.Models;
using TransInputMethod.Services;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TransInputMethod.Forms
{
    public partial class FloatingTranslationForm : Form
    {
        // Windows API for getting caret position
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
[DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern bool GetCaretPos(out Point point);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const int KEYEVENTF_KEYUP = 0x02;
        private const int VK_CONTROL = 0x11;
        private const int VK_V = 0x56;

        private readonly TranslationService _translationService;
        private readonly ConfigService _configService;
        private readonly TranslationDbContext _dbContext;
        
        private TextBox _mainTextBox = null!;
        private Panel _controlPanel = null!;
        private Button _historyButton = null!;
        private ComboBox _scenarioComboBox = null!;
        private ProgressBar _loadingBar = null!;
        private Label _statusLabel = null!;
        private Button _translateButton = null!;
        private Button _settingsButton = null!;
        private Label _languageDetectionLabel = null!;

        private string _lastTranslatedText = string.Empty;
        private string _currentInputText = string.Empty;
        // Removed history navigation - now using separate history window only
        private bool _isTranslating = false;
        private bool _hasTranslated = false; // Track if we have translated once

        public FloatingTranslationForm(TranslationService translationService, ConfigService configService, TranslationDbContext dbContext)
        {
            _translationService = translationService;
            _configService = configService;
            _dbContext = dbContext;
            
            InitializeComponent();
            SetupForm();
            _ = LoadConfiguration(); // Fire and forget for constructor
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Size = new Size(600, 132);
            this.MinimumSize = new Size(600, 132);
            this.MaximumSize = new Size(600, 600);

            // Main text box with improved placeholder
            _mainTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.None,
                Font = new Font("Segoe UI", 15F, FontStyle.Regular),
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81), // Improved contrast #374151
                Margin = new Padding(24, 20, 24, 12),
                WordWrap = true,
                PlaceholderText = "输入/粘贴内容…（Ctrl+Enter 快捷翻译）"
            };
            _mainTextBox.TextChanged += MainTextBox_TextChanged;
            _mainTextBox.KeyDown += MainTextBox_KeyDown;
            _mainTextBox.MouseEnter += (s, e) => _mainTextBox.Cursor = Cursors.IBeam;
            _mainTextBox.MouseDown += TextBox_MouseDown;

            // Control panel with improved spacing and divider
            _controlPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(12)
            };
            
            // Add top border as divider
            _controlPanel.Paint += (s, e) => {
                using var pen = new Pen(Color.FromArgb(229, 231, 235), 1);
                e.Graphics.DrawLine(pen, 0, 0, _controlPanel.Width, 0);
            };

            // History button (small, left corner)
            _historyButton = new Button
            {
                Text = "📋",
                Size = new Size(32, 32),
                Location = new Point(12, 16),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 12F),
                Cursor = Cursors.Hand,
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _historyButton.FlatAppearance.BorderSize = 0;
            _historyButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(229, 231, 235);
            _historyButton.Click += HistoryButton_Click;
            
            // Settings button (small, next to history)
            _settingsButton = new Button
            {
                Text = "⚙",
                Size = new Size(32, 32),
                Location = new Point(52, 16),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 12F),
                Cursor = Cursors.Hand,
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _settingsButton.FlatAppearance.BorderSize = 0;
            _settingsButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(229, 231, 235);
            _settingsButton.Click += SettingsButton_Click;
            
            // Main translate button (right side)
            _translateButton = new Button
            {
                Text = "翻译",
                Size = new Size(80, 36),
                Location = new Point(508, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(59, 130, 246),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                ForeColor = Color.White
            };
            _translateButton.FlatAppearance.BorderSize = 0;
            _translateButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235);
            _translateButton.Click += TranslateButton_Click;

            // Removed previous and next buttons - using separate history window instead

            // Scenario combo box (center-right)
            _scenarioComboBox = new ComboBox
            {
                Size = new Size(100, 28),
                Location = new Point(400, 16),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(55, 65, 81),
                BackColor = Color.White,
                Cursor = Cursors.Default
            };
            _scenarioComboBox.SelectedIndexChanged += ScenarioComboBox_SelectedIndexChanged;

            // Loading bar (center area, shows during translation)
            _loadingBar = new ProgressBar
            {
                Size = new Size(200, 3),
                Location = new Point(200, 30),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false,
                Cursor = Cursors.Default,
                ForeColor = Color.FromArgb(59, 130, 246)
            };

            // Status label (center area)
            _statusLabel = new Label
            {
                Size = new Size(200, 20),
                Location = new Point(200, 16),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(107, 114, 128),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Default,
                Text = ""
            };
            
            // Language detection label (shows after translation, top-right)
            _languageDetectionLabel = new Label
            {
                Size = new Size(180, 20),
                Location = new Point(408, 52),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(107, 114, 128),
                TextAlign = ContentAlignment.TopRight,
                Cursor = Cursors.Default,
                Text = "",
                Visible = false
            };

            // Add controls to control panel
            _controlPanel.Controls.AddRange(new Control[] 
            { 
                _historyButton, _settingsButton, _statusLabel, _loadingBar, 
                _scenarioComboBox, _translateButton
            });


            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Setup form painting for custom border
            this.Paint += OnFormPaint;
            this.Resize += (s, e) => this.Invalidate();

            // Create a main container with padding to leave space for border
            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(1), // 1px padding for border space
                BackColor = Color.Transparent
            };
            
            // Add proper text box padding by using a container panel
            var textContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(24, 20, 24, 16)
            };
            textContainer.Controls.Add(_mainTextBox);
            textContainer.Controls.Add(_languageDetectionLabel);
            
            // Update control panel dock
            _controlPanel.Dock = DockStyle.Bottom;
            
            // Add controls to main container
            mainContainer.Controls.Add(textContainer);
            mainContainer.Controls.Add(_controlPanel);
            
            // Make form draggable - control panel
            _controlPanel.MouseDown += Form_MouseDown;
            _controlPanel.MouseMove += Form_MouseMove;
            _controlPanel.MouseUp += Form_MouseUp;
            _controlPanel.Cursor = Cursors.SizeAll;

            // Make form draggable - form itself (empty areas)
            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;
            
            // Set cursor for dragging on form but not on text box
            this.MouseEnter += (s, e) => {
                if (!_mainTextBox.ClientRectangle.Contains(_mainTextBox.PointToClient(Cursor.Position)))
                    this.Cursor = Cursors.SizeAll;
            };
            this.MouseLeave += (s, e) => this.Cursor = Cursors.Default;

            // Handle form deactivation - no longer hide on deactivate
            // Only hide when ESC is pressed
            
            // Update controls order
            this.Controls.Clear();
            this.Controls.Add(mainContainer);
        }

        private void OnFormPaint(object sender, PaintEventArgs e)
        {
            // Draw custom light gray border
            using var pen = new Pen(Color.FromArgb(209, 213, 219), 1);
            var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            e.Graphics.DrawRectangle(pen, rect);
        }


        private Point _lastMouseScreenPosition;
        private bool _isDragging = false;

        private void Form_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Convert to screen coordinates to avoid offset issues
                var control = sender as Control ?? this;
                _lastMouseScreenPosition = control.PointToScreen(e.Location);
                _isDragging = true;
            }
        }

        private void Form_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // Use screen coordinates for consistent calculation
                var control = sender as Control ?? this;
                var currentScreenPosition = control.PointToScreen(e.Location);
                
                var deltaX = currentScreenPosition.X - _lastMouseScreenPosition.X;
                var deltaY = currentScreenPosition.Y - _lastMouseScreenPosition.Y;
                
                this.Location = new Point(this.Location.X + deltaX, this.Location.Y + deltaY);
                _lastMouseScreenPosition = currentScreenPosition;
            }
        }

        private void Form_MouseUp(object? sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void TextBox_MouseDown(object? sender, MouseEventArgs e)
        {
            // Prevent form dragging when clicking on text box
            _isDragging = false;
        }

        private async Task LoadConfiguration()
        {
            try
            {
                var config = await _configService.GetConfigAsync();
                
                _scenarioComboBox.Items.Clear();
                foreach (var scenario in config.Scenarios)
                {
                    _scenarioComboBox.Items.Add(scenario.Name);
                }

                var defaultScenario = config.Scenarios.FirstOrDefault(s => s.IsDefault);
                if (defaultScenario != null)
                {
                    _scenarioComboBox.SelectedItem = defaultScenario.Name;
                }
                else if (_scenarioComboBox.Items.Count > 0)
                {
                    _scenarioComboBox.SelectedIndex = 0;
                }

                // Apply UI settings
                this.Opacity = config.Ui.WindowOpacity / 100.0;
                this.Width = config.Ui.WindowWidth;
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"配置加载失败: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
            }
        }

        private void MainTextBox_TextChanged(object? sender, EventArgs e)
        {
            _currentInputText = _mainTextBox.Text;
            AdjustFormHeight();
            
            // Clear language detection when text changes
            _languageDetectionLabel.Visible = false;
            _languageDetectionLabel.Text = "";
        }

        private void AdjustFormHeight()
        {
            using (var g = _mainTextBox.CreateGraphics())
            {
                // Calculate text size with proper padding consideration
                var availableWidth = _mainTextBox.Width - 48; // Account for text box padding
                var textSize = g.MeasureString(_mainTextBox.Text + "A", _mainTextBox.Font, availableWidth);
                var lines = Math.Max(1, (int)Math.Ceiling(textSize.Height / _mainTextBox.Font.Height));
                var baseHeight = 132; // Updated base height
                var lineHeight = (int)(_mainTextBox.Font.Height * 1.4f); // Better line spacing
                var newHeight = Math.Min(600, Math.Max(baseHeight, baseHeight + (lines - 1) * lineHeight));
                
                if (this.Height != newHeight)
                {
                    this.Height = newHeight;
                    this.Invalidate(); // Refresh painting when height changes
                }
            }
        }

        private async void MainTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                
                if (string.IsNullOrWhiteSpace(_currentInputText))
                    return;

                // If we have already translated, copy to clipboard, close, and paste
                if (_hasTranslated && !string.IsNullOrEmpty(_lastTranslatedText))
                {
                    _statusLabel.Text = "正在复制并粘贴...";
                    _statusLabel.ForeColor = Color.FromArgb(59, 130, 246);
                    
                    // Hide the form first
                    this.Hide();
                    
                    // Wait a moment for the form to hide
                    await Task.Delay(100);
                    
                    // Paste to active window
                    PasteToActiveWindow(_lastTranslatedText);
                    
                    return;
                }

                // Check if same text - if so, copy last translation
                if (_currentInputText.Trim() == _lastTranslatedText.Trim())
                {
                    var lastTranslation = await _dbContext.GetLastTranslationAsync();
                    if (lastTranslation != null)
                    {
                        Clipboard.SetText(lastTranslation.TranslatedText);
                        _statusLabel.Text = "译文已复制";
                        _statusLabel.ForeColor = Color.FromArgb(34, 197, 94);
                        _lastTranslatedText = lastTranslation.TranslatedText;
                        _hasTranslated = true;
                        return;
                    }
                }

                await TranslateCurrentText();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
            }
        }

        private async Task TranslateCurrentText()
        {
            if (_isTranslating)
                return;

            _isTranslating = true;
            ShowLoadingState();

            try
            {
                var selectedScenario = _scenarioComboBox.SelectedItem?.ToString();
                var result = await _translationService.TranslateAsync(_currentInputText, selectedScenario);

                if (result.Success)
                {
                    _mainTextBox.Text = result.TranslatedText;
                    _lastTranslatedText = result.TranslatedText;
                    _hasTranslated = true;
                    
                    // Save to database
                    var history = new TranslationHistory
                    {
                        SourceText = result.OriginalText,
                        TranslatedText = result.TranslatedText,
                        SourceLanguage = result.SourceLanguage,
                        TargetLanguage = result.TargetLanguage,
                        TranslationScenario = result.Scenario,
                        Timestamp = DateTime.Now
                    };

                    await _dbContext.AddTranslationAsync(history);
                    _statusLabel.Text = "翻译完成";
                    _statusLabel.ForeColor = Color.FromArgb(34, 197, 94);
                    
                    // Show language detection result and flash success
                    ShowLanguageDetection(result.SourceLanguage, result.TargetLanguage);
                    FlashButtonSuccess();

                    // History is now managed in separate window
                }
                else
                {
                    _statusLabel.Text = result.ErrorMessage ?? "翻译失败";
                    _statusLabel.ForeColor = Color.FromArgb(239, 68, 68);
                    
                    // Flash red border on translate button for error feedback
                    FlashButtonError();
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"翻译出错: {ex.Message}";
                _statusLabel.ForeColor = Color.FromArgb(239, 68, 68);
                
                // Flash red border on translate button for error feedback
                FlashButtonError();
            }
            finally
            {
                HideLoadingState();
                _isTranslating = false;
            }
        }

        private void ShowLoadingState()
        {
            _loadingBar.Visible = true;
            _translateButton.Text = "翻译中...";
            _translateButton.Enabled = false;
            _statusLabel.Text = "正在翻译...";
            _statusLabel.ForeColor = Color.FromArgb(59, 130, 246);
        }

        private void HideLoadingState()
        {
            _loadingBar.Visible = false;
            _translateButton.Text = "翻译";
            _translateButton.Enabled = true;
        }
        
        private void ShowLanguageDetection(string? sourceLanguage, string? targetLanguage)
        {
            if (!string.IsNullOrEmpty(sourceLanguage) && !string.IsNullOrEmpty(targetLanguage))
            {
                var sourceLang = sourceLanguage == "Chinese" ? "中文" : "英文";
                var targetLang = targetLanguage == "Chinese" ? "中文" : "英文";
                _languageDetectionLabel.Text = $"检测到：{sourceLang} → {targetLang}";
                _languageDetectionLabel.Visible = true;
            }
        }
        
        private async void FlashButtonSuccess()
        {
            var originalColor = _translateButton.BackColor;
            _translateButton.BackColor = Color.FromArgb(34, 197, 94); // Green success
            await Task.Delay(300);
            _translateButton.BackColor = originalColor;
        }
        
        private async void FlashButtonError()
        {
            var originalBorderColor = _translateButton.FlatAppearance.BorderColor;
            _translateButton.FlatAppearance.BorderSize = 2;
            _translateButton.FlatAppearance.BorderColor = Color.FromArgb(239, 68, 68); // Red error
            await Task.Delay(300);
            _translateButton.FlatAppearance.BorderSize = 0;
            _translateButton.FlatAppearance.BorderColor = originalBorderColor;
        }
        
        private async void TranslateButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentInputText))
                return;
                
            await TranslateCurrentText();
        }
        
        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var settingsForm = new SettingsForm(_configService);
                settingsForm.ShowDialog();
                
                // Reload configuration after settings are saved
                _ = LoadConfiguration();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"\u6253\u5f00\u8bbe\u7f6e\u5931\u8d25: {ex.Message}", "\u9519\u8bef", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Removed RefreshHistoryNavigation - using separate history window instead

        // Removed navigation button methods - using separate history window instead

        private void HistoryButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var historyForm = new HistoryForm(_dbContext);
                historyForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开历史记录失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ScenarioComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Update status when scenario changes
            if (_scenarioComboBox.SelectedItem != null)
            {
                _statusLabel.Text = $"当前场景: {_scenarioComboBox.SelectedItem}";
                _statusLabel.ForeColor = Color.FromArgb(107, 114, 128);
            }
        }

        private Point GetTextCursorPosition()
        {
            try
            {
                // 获取前台窗口
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                    return GetScreenCenterPosition();

                // 获取前台窗口的线程 ID
                uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindow, out _);
                
                // 使用 GUITHREADINFO 获取光标信息
                GUITHREADINFO guiInfo = new GUITHREADINFO();
                guiInfo.cbSize = (uint)Marshal.SizeOf(guiInfo);
                
                if (GetGUIThreadInfo(foregroundThreadId, ref guiInfo))
                {
                    // 检查是否有光标窗口
                    if (guiInfo.hwndCaret != IntPtr.Zero)
                    {
                        // 获取光标位置（相对于光标窗口）
                        Point caretPos = new Point(guiInfo.rcCaret.left, guiInfo.rcCaret.top);
                        
                        // 转换为屏幕坐标
                        ClientToScreen(guiInfo.hwndCaret, ref caretPos);
                        
                        // 应用 DPI 缩放修正（如果需要）
                        var correctedPos = ApplyDpiScaling(caretPos);
                        
                        // 添加调试信息
                        System.Diagnostics.Debug.WriteLine($"GUITHREADINFO光标位置: {caretPos}, 修正后位置: {correctedPos}");
                        
                        return correctedPos;
                    }
                }
                
                // 如果无法获取光标位置，尝试获取鼠标位置作为备用方案
                return GetMousePositionFallback();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取文本光标位置失败: {ex.Message}");
                return GetMousePositionFallback();
            }
        }

        private Point ApplyDpiScaling(Point originalPos)
        {
            try
            {
                // 获取 DPI awareness
                using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var dpiX = graphics.DpiX;
                    var dpiY = graphics.DpiY;
                    
                    // 如果应用程序是 DPI-aware 的，可能不需要额外的缩放
                    // 这取决于应用程序清单设置
                    
                    // 大多数情况下，ClientToScreen 已经返回了正确的屏幕坐标
                    System.Diagnostics.Debug.WriteLine($"DPI: {dpiX}x{dpiY}, 原始位置: {originalPos}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DPI缩放检查失败: {ex.Message}");
            }
            
            // 大多数情况下，直接返回原始位置即可
            return originalPos;
        }

        private Point GetMousePositionFallback()
        {
            if (GetCursorPos(out Point mousePos))
            {
                // 在鼠标位置稍微偏移，避免遮挡
                System.Diagnostics.Debug.WriteLine($"使用鼠标位置作为备用方案: {mousePos}");
                return new Point(mousePos.X + 10, mousePos.Y + 10);
            }
            
            return GetScreenCenterPosition();
        }

        private Point GetScreenCenterPosition()
        {
            var screen = Screen.PrimaryScreen;
            if (screen == null)
            {
                return new Point(300, 300); // fallback position
            }
            var centerX = screen.WorkingArea.Left + (screen.WorkingArea.Width - this.Width) / 2;
            var centerY = screen.WorkingArea.Top + (screen.WorkingArea.Height - this.Height) / 2;
            return new Point(centerX, centerY);
        }

        private async void PasteToActiveWindow(string text)
        {
            try
            {
                // Set clipboard content
                Clipboard.SetText(text);
                
                // Wait a moment to ensure clipboard is set
                await Task.Delay(50);
                
                // Simulate Ctrl+V to paste
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"粘贴到活动窗口失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 把窗口提到前台并把键盘焦点给 _mainTextBox
        /// </summary>
        private void ForceFocus()
        {
            IntPtr fgWin = GetForegroundWindow();
            uint fgThread = GetWindowThreadProcessId(fgWin, out _);
            uint thisThread = GetCurrentThreadId();

            // 把当前线程临时附加到前台窗口的输入队列
            AttachThreadInput(thisThread, fgThread, true);

            // 把本窗体变成前台窗口
            SetForegroundWindow(this.Handle);     // 激活窗体
            this.Activate();

            // 把焦点给文本框
            SetFocus(_mainTextBox.Handle);

            // 分离输入队列，恢复系统状态
            AttachThreadInput(thisThread, fgThread, false);
        }

        public async Task ShowAtCursor()
        {
            var cursorPos = GetTextCursorPosition();
            var screen = Screen.FromPoint(cursorPos);

            // 计算窗口位置，确保不超出屏幕边界
            var x = cursorPos.X;
            var y = cursorPos.Y + 25; // 在光标下方显示

            // 确保窗口完全在屏幕内
            if (x + this.Width > screen.WorkingArea.Right)
                x = screen.WorkingArea.Right - this.Width;
            if (x < screen.WorkingArea.Left)
                x = screen.WorkingArea.Left;

            if (y + this.Height > screen.WorkingArea.Bottom)
            {
                // 如果下方空间不足，显示在光标上方
                y = cursorPos.Y - this.Height - 10;
                if (y < screen.WorkingArea.Top)
                    y = screen.WorkingArea.Top;
            }

            this.Location = new Point(x, y);

            // 添加调试信息
            System.Diagnostics.Debug.WriteLine($"光标位置: {cursorPos}, 窗体位置: {this.Location}");

            // Reload configuration each time the form is shown
            await LoadConfiguration();

            this.Show();
            this.BringToFront();      // 可选
            this.TopMost = true;      // 如果需要置顶

            // 使用 BeginInvoke 确保在消息泵下一帧执行
            this.BeginInvoke(new Action(() =>
            {
                ForceFocus();         // ←←← 核心调用
            }));

            // 下面是你原来的状态复位逻辑
            if (!_isTranslating)
            {
                _mainTextBox.Clear();
                _statusLabel.Text = "";
                _statusLabel.ForeColor = Color.FromArgb(107, 114, 128);
                _hasTranslated = false;
                _lastTranslatedText = string.Empty;
                _languageDetectionLabel.Visible = false;
                _languageDetectionLabel.Text = "";
            }
        }

        // 移除 OnShown 的焦点处理逻辑

        /// <summary>
        /// 兜底确保文本框获得焦点
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!_mainTextBox.Focused)
                _mainTextBox.BeginInvoke(new Action(() => _mainTextBox.Focus()));
        }
    }
}