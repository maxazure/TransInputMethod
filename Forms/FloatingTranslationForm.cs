using TransInputMethod.Data;
using TransInputMethod.Models;
using TransInputMethod.Services;
using System.Drawing;
using System.Windows.Forms;

namespace TransInputMethod.Forms
{
    public partial class FloatingTranslationForm : Form
    {
        private readonly TranslationService _translationService;
        private readonly ConfigService _configService;
        private readonly TranslationDbContext _dbContext;
        
        private TextBox _mainTextBox = null!;
        private Panel _controlPanel = null!;
        private Button _historyButton = null!;
        private Button _previousButton = null!;
        private Button _nextButton = null!;
        private ComboBox _scenarioComboBox = null!;
        private ProgressBar _loadingBar = null!;
        private Label _statusLabel = null!;

        private string _lastTranslatedText = string.Empty;
        private string _currentInputText = string.Empty;
        private List<TranslationHistory> _currentHistory = new List<TranslationHistory>();
        private int _historyIndex = -1;
        private bool _isTranslating = false;

        public FloatingTranslationForm(TranslationService translationService, ConfigService configService, TranslationDbContext dbContext)
        {
            _translationService = translationService;
            _configService = configService;
            _dbContext = dbContext;
            
            InitializeComponent();
            SetupForm();
            LoadConfiguration();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.White;
            this.Size = new Size(600, 120);
            this.MinimumSize = new Size(600, 120);
            this.MaximumSize = new Size(600, 600);

            // Main text box
            _mainTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Microsoft YaHei UI", 11F),
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Margin = new Padding(10)
            };
            _mainTextBox.TextChanged += MainTextBox_TextChanged;
            _mainTextBox.KeyDown += MainTextBox_KeyDown;

            // Control panel
            _controlPanel = new Panel
            {
                Height = 35,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // History button
            _historyButton = new Button
            {
                Text = "⌚",
                Size = new Size(30, 25),
                Location = new Point(10, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Emoji", 12F),
                Cursor = Cursors.Hand
            };
            _historyButton.FlatAppearance.BorderSize = 0;
            _historyButton.Click += HistoryButton_Click;

            // Previous button
            _previousButton = new Button
            {
                Text = "↑",
                Size = new Size(25, 25),
                Location = new Point(50, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Font = new Font("Microsoft YaHei UI", 12F),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _previousButton.FlatAppearance.BorderSize = 0;
            _previousButton.Click += PreviousButton_Click;

            // Next button
            _nextButton = new Button
            {
                Text = "↓",
                Size = new Size(25, 25),
                Location = new Point(80, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Font = new Font("Microsoft YaHei UI", 12F),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _nextButton.FlatAppearance.BorderSize = 0;
            _nextButton.Click += NextButton_Click;

            // Scenario combo box
            _scenarioComboBox = new ComboBox
            {
                Size = new Size(80, 25),
                Location = new Point(510, 5),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            _scenarioComboBox.SelectedIndexChanged += ScenarioComboBox_SelectedIndexChanged;

            // Loading bar
            _loadingBar = new ProgressBar
            {
                Size = new Size(400, 3),
                Location = new Point(110, 16),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };

            // Status label
            _statusLabel = new Label
            {
                Size = new Size(400, 25),
                Location = new Point(110, 5),
                Font = new Font("Microsoft YaHei UI", 8F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "输入文本后按 Ctrl+Enter 翻译"
            };

            // Add controls
            _controlPanel.Controls.AddRange(new Control[] 
            { 
                _historyButton, _previousButton, _nextButton, 
                _scenarioComboBox, _loadingBar, _statusLabel 
            });

            this.Controls.Add(_mainTextBox);
            this.Controls.Add(_controlPanel);

            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Add shadow effect
            this.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(200, 200, 200)), rect);
            };

            // Make form draggable
            _controlPanel.MouseDown += Form_MouseDown;
            _controlPanel.MouseMove += Form_MouseMove;
            _controlPanel.MouseUp += Form_MouseUp;

            // Handle form deactivation
            this.Deactivate += (s, e) => this.Hide();
        }

        private Point _lastMousePosition;
        private bool _isDragging = false;

        private void Form_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _lastMousePosition = e.Location;
            }
        }

        private void Form_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var deltaX = e.Location.X - _lastMousePosition.X;
                var deltaY = e.Location.Y - _lastMousePosition.Y;
                this.Location = new Point(this.Location.X + deltaX, this.Location.Y + deltaY);
            }
        }

        private void Form_MouseUp(object? sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private async void LoadConfiguration()
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
            
            if (string.IsNullOrWhiteSpace(_currentInputText))
            {
                _statusLabel.Text = "输入文本后按 Ctrl+Enter 翻译";
                _statusLabel.ForeColor = Color.Gray;
            }
        }

        private void AdjustFormHeight()
        {
            using (var g = _mainTextBox.CreateGraphics())
            {
                var textSize = g.MeasureString(_mainTextBox.Text + "A", _mainTextBox.Font, _mainTextBox.Width);
                var lines = Math.Max(1, (int)Math.Ceiling(textSize.Height / _mainTextBox.Font.Height));
                var newHeight = Math.Min(600, Math.Max(120, lines * (int)_mainTextBox.Font.Height + 50));
                
                if (this.Height != newHeight)
                {
                    this.Height = newHeight;
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

                // Check if same text - if so, copy last translation
                if (_currentInputText.Trim() == _lastTranslatedText.Trim())
                {
                    var lastTranslation = await _dbContext.GetLastTranslationAsync();
                    if (lastTranslation != null)
                    {
                        Clipboard.SetText(lastTranslation.TranslatedText);
                        _statusLabel.Text = "译文已复制到剪贴板";
                        _statusLabel.ForeColor = Color.Green;
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
                    _statusLabel.Text = "翻译完成，再次按 Ctrl+Enter 复制";
                    _statusLabel.ForeColor = Color.Green;

                    // Refresh history navigation
                    await RefreshHistoryNavigation();
                }
                else
                {
                    _statusLabel.Text = result.ErrorMessage ?? "翻译失败";
                    _statusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"翻译出错: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
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
            _statusLabel.Text = "正在翻译...";
            _statusLabel.ForeColor = Color.Blue;
        }

        private void HideLoadingState()
        {
            _loadingBar.Visible = false;
        }

        private async Task RefreshHistoryNavigation()
        {
            try
            {
                var historyResult = await _dbContext.GetTranslationHistoryAsync(1, 20);
                _currentHistory = historyResult.Data;
                _historyIndex = -1;
                
                _previousButton.Enabled = _currentHistory.Count > 0;
                _nextButton.Enabled = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"刷新历史导航失败: {ex.Message}");
            }
        }

        private void PreviousButton_Click(object? sender, EventArgs e)
        {
            if (_currentHistory.Count == 0)
                return;

            _historyIndex++;
            if (_historyIndex >= _currentHistory.Count)
                _historyIndex = _currentHistory.Count - 1;

            LoadHistoryItem();
            UpdateNavigationButtons();
        }

        private void NextButton_Click(object? sender, EventArgs e)
        {
            if (_historyIndex <= 0)
                return;

            _historyIndex--;
            LoadHistoryItem();
            UpdateNavigationButtons();
        }

        private void LoadHistoryItem()
        {
            if (_historyIndex >= 0 && _historyIndex < _currentHistory.Count)
            {
                var item = _currentHistory[_historyIndex];
                _mainTextBox.Text = $"{item.SourceText}\n---\n{item.TranslatedText}";
                _statusLabel.Text = $"历史记录 ({item.Timestamp:MM-dd HH:mm})";
                _statusLabel.ForeColor = Color.Gray;
            }
        }

        private void UpdateNavigationButtons()
        {
            _previousButton.Enabled = _historyIndex < _currentHistory.Count - 1;
            _nextButton.Enabled = _historyIndex > 0;
        }

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
                _statusLabel.ForeColor = Color.Gray;
            }
        }

        public void ShowAtCursor()
        {
            var cursorPos = Cursor.Position;
            var screen = Screen.FromPoint(cursorPos);
            
            // Position the form near cursor but ensure it's fully visible
            var x = Math.Min(cursorPos.X, screen.WorkingArea.Right - this.Width);
            var y = Math.Min(cursorPos.Y + 20, screen.WorkingArea.Bottom - this.Height);
            
            this.Location = new Point(x, y);
            this.Show();
            this.Activate();
            _mainTextBox.Focus();
            
            // Clear previous content
            if (!_isTranslating)
            {
                _mainTextBox.Clear();
                _statusLabel.Text = "输入文本后按 Ctrl+Enter 翻译";
                _statusLabel.ForeColor = Color.Gray;
            }
        }
    }
}