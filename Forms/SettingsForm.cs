using TransInputMethod.Models;
using TransInputMethod.Services;
using TransInputMethod.Utils;
using System.Drawing;
using System.Windows.Forms;

namespace TransInputMethod.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ConfigService _configService;
        private AppConfig _config = new AppConfig();

        // Tab control and pages
        private TabControl _tabControl;
        private TabPage _apiTabPage;
        private TabPage _hotkeyTabPage;
        private TabPage _scenarioTabPage;
        private TabPage _uiTabPage;

        // API settings controls
        private TextBox _baseUrlTextBox;
        private TextBox _apiKeyTextBox;
        private CheckBox _showApiKeyCheckBox;
        private ComboBox _modelComboBox;
        private NumericUpDown _timeoutNumericUpDown;

        // Hotkey settings controls
        private HotkeyTextBox _showWindowHotkeyTextBox;
        private HotkeyTextBox _translateHotkeyTextBox;

        // Scenario settings controls
        private ListBox _scenarioListBox;
        private TextBox _scenarioNameTextBox;
        private TextBox _scenarioPromptTextBox;
        private Button _addScenarioButton;
        private Button _updateScenarioButton;
        private Button _deleteScenarioButton;
        private CheckBox _defaultScenarioCheckBox;

        // UI settings controls
        private NumericUpDown _windowWidthNumericUpDown;
        private TrackBar _opacityTrackBar;
        private Label _opacityValueLabel;
        private CheckBox _darkModeCheckBox;
        private NumericUpDown _historyPageSizeNumericUpDown;

        // Action buttons
        private Button _saveButton;
        private Button _cancelButton;
        private Button _testConnectionButton;

        public SettingsForm(ConfigService configService)
        {
            _configService = configService;
            InitializeComponent();
            LoadConfiguration();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.Text = "设置";
            this.Size = new Size(650, 500);
            this.MinimumSize = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Microsoft YaHei UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            CreateTabControl();
            CreateApiTab();
            CreateHotkeyTab();
            CreateScenarioTab();
            CreateUiTab();
            CreateActionButtons();

            this.Controls.Add(_tabControl);
            this.Controls.AddRange(new Control[] { _saveButton, _cancelButton, _testConnectionButton });

            this.ResumeLayout(false);
        }

        private void CreateTabControl()
        {
            _tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(620, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            _apiTabPage = new TabPage("API 设置");
            _hotkeyTabPage = new TabPage("热键配置");
            _scenarioTabPage = new TabPage("场景管理");
            _uiTabPage = new TabPage("界面设置");

            _tabControl.TabPages.AddRange(new TabPage[] 
            { 
                _apiTabPage, _hotkeyTabPage, _scenarioTabPage, _uiTabPage 
            });
        }

        private void CreateApiTab()
        {
            var y = 20;

            // Base URL
            var baseUrlLabel = new Label
            {
                Text = "Base URL:",
                Location = new Point(20, y),
                Size = new Size(100, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _baseUrlTextBox = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(450, 23),
                Text = "https://api.openai.com/v1"
            };
            
            y += 35;

            // API Key
            var apiKeyLabel = new Label
            {
                Text = "API Key:",
                Location = new Point(20, y),
                Size = new Size(100, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _apiKeyTextBox = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(350, 23),
                UseSystemPasswordChar = true
            };
            
            _showApiKeyCheckBox = new CheckBox
            {
                Text = "显示",
                Location = new Point(490, y),
                Size = new Size(60, 23)
            };
            _showApiKeyCheckBox.CheckedChanged += ShowApiKeyCheckBox_CheckedChanged;
            
            y += 35;

            // Model
            var modelLabel = new Label
            {
                Text = "模型:",
                Location = new Point(20, y),
                Size = new Size(100, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _modelComboBox = new ComboBox
            {
                Location = new Point(130, y),
                Size = new Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            _modelComboBox.Items.AddRange(new string[] 
            { 
                "gpt-4o", "gpt-4o-mini", "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" 
            });
            
            y += 35;

            // Timeout
            var timeoutLabel = new Label
            {
                Text = "超时 (秒):",
                Location = new Point(20, y),
                Size = new Size(100, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _timeoutNumericUpDown = new NumericUpDown
            {
                Location = new Point(130, y),
                Size = new Size(100, 23),
                Minimum = 5,
                Maximum = 120,
                Value = 30
            };

            _apiTabPage.Controls.AddRange(new Control[] 
            { 
                baseUrlLabel, _baseUrlTextBox, 
                apiKeyLabel, _apiKeyTextBox, _showApiKeyCheckBox,
                modelLabel, _modelComboBox,
                timeoutLabel, _timeoutNumericUpDown
            });
        }

        private void CreateHotkeyTab()
        {
            var y = 20;

            // Show window hotkey
            var showHotkeyLabel = new Label
            {
                Text = "呼出快捷键:",
                Location = new Point(20, y),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _showWindowHotkeyTextBox = new HotkeyTextBox
            {
                Location = new Point(150, y),
                Size = new Size(200, 23)
            };
            _showWindowHotkeyTextBox.HotkeyChanged += (s, e) => {
                // Update configuration when hotkey changes
            };
            
            y += 35;

            // Translate hotkey
            var translateHotkeyLabel = new Label
            {
                Text = "翻译快捷键:",
                Location = new Point(20, y),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _translateHotkeyTextBox = new HotkeyTextBox
            {
                Location = new Point(150, y),
                Size = new Size(200, 23)
            };
            _translateHotkeyTextBox.HotkeyChanged += (s, e) => {
                // Update configuration when hotkey changes
            };
            
            y += 50;

            var hotkeyNote = new Label
            {
                Text = "提示: 点击输入框后按下组合键即可设置快捷键",
                Location = new Point(20, y),
                Size = new Size(550, 40),
                ForeColor = Color.Gray,
                Font = new Font("Microsoft YaHei UI", 8F)
            };

            _hotkeyTabPage.Controls.AddRange(new Control[] 
            { 
                showHotkeyLabel, _showWindowHotkeyTextBox,
                translateHotkeyLabel, _translateHotkeyTextBox,
                hotkeyNote
            });
        }

        private void CreateScenarioTab()
        {
            // Scenario list
            var listLabel = new Label
            {
                Text = "翻译场景:",
                Location = new Point(20, 20),
                Size = new Size(100, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _scenarioListBox = new ListBox
            {
                Location = new Point(20, 45),
                Size = new Size(250, 200),
                DisplayMember = "Name"
            };
            _scenarioListBox.SelectedIndexChanged += ScenarioListBox_SelectedIndexChanged;

            // Scenario details
            var nameLabel = new Label
            {
                Text = "场景名称:",
                Location = new Point(290, 20),
                Size = new Size(80, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _scenarioNameTextBox = new TextBox
            {
                Location = new Point(290, 45),
                Size = new Size(200, 23)
            };

            var promptLabel = new Label
            {
                Text = "提示词:",
                Location = new Point(290, 80),
                Size = new Size(80, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _scenarioPromptTextBox = new TextBox
            {
                Location = new Point(290, 105),
                Size = new Size(290, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            _defaultScenarioCheckBox = new CheckBox
            {
                Text = "设为默认场景",
                Location = new Point(290, 215),
                Size = new Size(150, 23)
            };

            // Buttons
            _addScenarioButton = new Button
            {
                Text = "新增",
                Location = new Point(290, 250),
                Size = new Size(60, 30),
                FlatStyle = FlatStyle.System
            };
            _addScenarioButton.Click += AddScenarioButton_Click;

            _updateScenarioButton = new Button
            {
                Text = "更新",
                Location = new Point(360, 250),
                Size = new Size(60, 30),
                FlatStyle = FlatStyle.System,
                Enabled = false
            };
            _updateScenarioButton.Click += UpdateScenarioButton_Click;

            _deleteScenarioButton = new Button
            {
                Text = "删除",
                Location = new Point(430, 250),
                Size = new Size(60, 30),
                FlatStyle = FlatStyle.System,
                Enabled = false
            };
            _deleteScenarioButton.Click += DeleteScenarioButton_Click;

            _scenarioTabPage.Controls.AddRange(new Control[] 
            { 
                listLabel, _scenarioListBox,
                nameLabel, _scenarioNameTextBox,
                promptLabel, _scenarioPromptTextBox,
                _defaultScenarioCheckBox,
                _addScenarioButton, _updateScenarioButton, _deleteScenarioButton
            });
        }

        private void CreateUiTab()
        {
            var y = 20;

            // Window width
            var widthLabel = new Label
            {
                Text = "窗口宽度:",
                Location = new Point(20, y),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _windowWidthNumericUpDown = new NumericUpDown
            {
                Location = new Point(150, y),
                Size = new Size(100, 23),
                Minimum = 400,
                Maximum = 1000,
                Value = 600
            };
            
            y += 35;

            // Opacity
            var opacityLabel = new Label
            {
                Text = "窗口透明度:",
                Location = new Point(20, y),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _opacityTrackBar = new TrackBar
            {
                Location = new Point(150, y),
                Size = new Size(200, 45),
                Minimum = 50,
                Maximum = 100,
                Value = 95,
                TickFrequency = 10
            };
            _opacityTrackBar.ValueChanged += OpacityTrackBar_ValueChanged;
            
            _opacityValueLabel = new Label
            {
                Text = "95%",
                Location = new Point(360, y + 10),
                Size = new Size(50, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            y += 50;

            // Dark mode
            _darkModeCheckBox = new CheckBox
            {
                Text = "深色模式 (暂不支持)",
                Location = new Point(20, y),
                Size = new Size(200, 23),
                Enabled = false
            };
            
            y += 35;

            // History page size
            var pageSizeLabel = new Label
            {
                Text = "历史记录页大小:",
                Location = new Point(20, y),
                Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            _historyPageSizeNumericUpDown = new NumericUpDown
            {
                Location = new Point(150, y),
                Size = new Size(100, 23),
                Minimum = 5,
                Maximum = 50,
                Value = 10
            };

            _uiTabPage.Controls.AddRange(new Control[] 
            { 
                widthLabel, _windowWidthNumericUpDown,
                opacityLabel, _opacityTrackBar, _opacityValueLabel,
                _darkModeCheckBox,
                pageSizeLabel, _historyPageSizeNumericUpDown
            });
        }

        private void CreateActionButtons()
        {
            _saveButton = new Button
            {
                Text = "保存",
                Location = new Point(370, 420),
                Size = new Size(80, 35),
                FlatStyle = FlatStyle.System,
                DialogResult = DialogResult.OK,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
            };
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new Button
            {
                Text = "取消",
                Location = new Point(460, 420),
                Size = new Size(80, 35),
                FlatStyle = FlatStyle.System,
                DialogResult = DialogResult.Cancel,
                Font = new Font("Microsoft YaHei UI", 9F)
            };

            _testConnectionButton = new Button
            {
                Text = "测试连接",
                Location = new Point(550, 420),
                Size = new Size(80, 35),
                FlatStyle = FlatStyle.System,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            _testConnectionButton.Click += TestConnectionButton_Click;

            this.AcceptButton = _saveButton;
            this.CancelButton = _cancelButton;
        }

        private async void LoadConfiguration()
        {
            try
            {
                _config = await _configService.GetConfigAsync();
                
                // Load API settings
                _baseUrlTextBox.Text = _config.Api.BaseUrl;
                _apiKeyTextBox.Text = _config.Api.ApiKey;
                _modelComboBox.Text = _config.Api.Model;
                _timeoutNumericUpDown.Value = _config.Api.Timeout;

                // Load hotkey settings
                LoadHotkeySettings();

                // Load UI settings
                _windowWidthNumericUpDown.Value = _config.Ui.WindowWidth;
                _opacityTrackBar.Value = _config.Ui.WindowOpacity;
                _opacityValueLabel.Text = $"{_config.Ui.WindowOpacity}%";
                _darkModeCheckBox.Checked = _config.Ui.DarkMode;
                _historyPageSizeNumericUpDown.Value = _config.Ui.HistoryPageSize;

                // Load scenarios
                LoadScenarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadScenarios()
        {
            _scenarioListBox.Items.Clear();
            foreach (var scenario in _config.Scenarios)
            {
                _scenarioListBox.Items.Add(scenario);
            }
            
            if (_scenarioListBox.Items.Count > 0)
            {
                _scenarioListBox.SelectedIndex = 0;
            }
        }

        private void LoadHotkeySettings()
        {
            // Parse show window hotkey
            var showModifiers = GlobalHotkey.ParseModifierKeys(_config.Hotkeys.ShowWindowHotkey);
            var showKey = GlobalHotkey.ParseKey(_config.Hotkeys.ShowWindowHotkey);
            _showWindowHotkeyTextBox.SetHotkey(showModifiers, showKey);

            // Parse translate hotkey
            var translateModifiers = GlobalHotkey.ParseModifierKeys(_config.Hotkeys.TranslateHotkey);
            var translateKey = GlobalHotkey.ParseKey(_config.Hotkeys.TranslateHotkey);
            _translateHotkeyTextBox.SetHotkey(translateModifiers, translateKey);
        }

        private string FormatHotkey(GlobalHotkey.ModifierKeys modifiers, Keys key)
        {
            var parts = new List<string>();

            if ((modifiers & GlobalHotkey.ModifierKeys.Control) != 0)
                parts.Add("Ctrl");
            if ((modifiers & GlobalHotkey.ModifierKeys.Alt) != 0)
                parts.Add("Alt");
            if ((modifiers & GlobalHotkey.ModifierKeys.Shift) != 0)
                parts.Add("Shift");
            if ((modifiers & GlobalHotkey.ModifierKeys.Win) != 0)
                parts.Add("Win");

            if (key != Keys.None)
            {
                var keyName = HotkeyCapture.GetKeyDisplayName(key);
                if (!string.IsNullOrEmpty(keyName))
                    parts.Add(keyName);
            }

            return string.Join("+", parts);
        }

        private void ShowApiKeyCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            _apiKeyTextBox.UseSystemPasswordChar = !_showApiKeyCheckBox.Checked;
        }

        private void OpacityTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            _opacityValueLabel.Text = $"{_opacityTrackBar.Value}%";
        }

        private void ScenarioListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_scenarioListBox.SelectedItem is TranslationScenario scenario)
            {
                _scenarioNameTextBox.Text = scenario.Name;
                _scenarioPromptTextBox.Text = scenario.Prompt;
                _defaultScenarioCheckBox.Checked = scenario.IsDefault;
                
                _updateScenarioButton.Enabled = true;
                _deleteScenarioButton.Enabled = _config.Scenarios.Count > 1; // Keep at least one scenario
            }
            else
            {
                _scenarioNameTextBox.Clear();
                _scenarioPromptTextBox.Clear();
                _defaultScenarioCheckBox.Checked = false;
                
                _updateScenarioButton.Enabled = false;
                _deleteScenarioButton.Enabled = false;
            }
        }

        private void AddScenarioButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_scenarioNameTextBox.Text))
            {
                MessageBox.Show("请输入场景名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newScenario = new TranslationScenario
            {
                Name = _scenarioNameTextBox.Text.Trim(),
                Prompt = _scenarioPromptTextBox.Text.Trim(),
                IsDefault = _defaultScenarioCheckBox.Checked
            };

            if (_defaultScenarioCheckBox.Checked)
            {
                // Remove default from other scenarios
                foreach (var scenario in _config.Scenarios)
                {
                    scenario.IsDefault = false;
                }
            }

            _config.Scenarios.Add(newScenario);
            LoadScenarios();
            
            // Select the new scenario
            _scenarioListBox.SelectedItem = newScenario;
        }

        private void UpdateScenarioButton_Click(object? sender, EventArgs e)
        {
            if (_scenarioListBox.SelectedItem is not TranslationScenario scenario)
                return;

            if (string.IsNullOrWhiteSpace(_scenarioNameTextBox.Text))
            {
                MessageBox.Show("请输入场景名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            scenario.Name = _scenarioNameTextBox.Text.Trim();
            scenario.Prompt = _scenarioPromptTextBox.Text.Trim();
            
            if (_defaultScenarioCheckBox.Checked)
            {
                // Remove default from other scenarios
                foreach (var s in _config.Scenarios)
                {
                    s.IsDefault = false;
                }
                scenario.IsDefault = true;
            }
            else
            {
                scenario.IsDefault = false;
            }

            LoadScenarios();
            _scenarioListBox.SelectedItem = scenario;
        }

        private void DeleteScenarioButton_Click(object? sender, EventArgs e)
        {
            if (_scenarioListBox.SelectedItem is not TranslationScenario scenario)
                return;

            if (_config.Scenarios.Count <= 1)
            {
                MessageBox.Show("至少需要保留一个翻译场景", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"确定要删除场景 '{scenario.Name}' 吗？", "确认删除", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                _config.Scenarios.Remove(scenario);
                
                // If deleted scenario was default, make the first one default
                if (scenario.IsDefault && _config.Scenarios.Count > 0)
                {
                    _config.Scenarios[0].IsDefault = true;
                }
                
                LoadScenarios();
            }
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(_baseUrlTextBox.Text))
                {
                    MessageBox.Show("请输入Base URL", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _tabControl.SelectedTab = _apiTabPage;
                    _baseUrlTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(_apiKeyTextBox.Text))
                {
                    MessageBox.Show("请输入API Key", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _tabControl.SelectedTab = _apiTabPage;
                    _apiKeyTextBox.Focus();
                    return;
                }

                // Update configuration
                _config.Api.BaseUrl = _baseUrlTextBox.Text.Trim();
                _config.Api.ApiKey = _apiKeyTextBox.Text.Trim();
                _config.Api.Model = _modelComboBox.Text;
                _config.Api.Timeout = (int)_timeoutNumericUpDown.Value;

                // Update hotkey settings
                var (showModifiers, showKey) = _showWindowHotkeyTextBox.GetHotkey();
                _config.Hotkeys.ShowWindowHotkey = FormatHotkey(showModifiers, showKey);
                
                var (translateModifiers, translateKey) = _translateHotkeyTextBox.GetHotkey();
                _config.Hotkeys.TranslateHotkey = FormatHotkey(translateModifiers, translateKey);

                _config.Ui.WindowWidth = (int)_windowWidthNumericUpDown.Value;
                _config.Ui.WindowOpacity = _opacityTrackBar.Value;
                _config.Ui.DarkMode = _darkModeCheckBox.Checked;
                _config.Ui.HistoryPageSize = (int)_historyPageSizeNumericUpDown.Value;

                // Ensure at least one default scenario exists
                if (!_config.Scenarios.Any(s => s.IsDefault) && _config.Scenarios.Count > 0)
                {
                    _config.Scenarios[0].IsDefault = true;
                }

                await _configService.SaveConfigAsync(_config);
                
                MessageBox.Show("配置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void TestConnectionButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _testConnectionButton.Enabled = false;
                _testConnectionButton.Text = "测试中...";

                // Create a temporary config for testing
                var testConfig = new AppConfig();
                testConfig.Api.BaseUrl = _baseUrlTextBox.Text.Trim();
                testConfig.Api.ApiKey = _apiKeyTextBox.Text.Trim();
                testConfig.Api.Model = _modelComboBox.Text;
                testConfig.Api.Timeout = (int)_timeoutNumericUpDown.Value;

                var tempConfigService = new ConfigService();
                await tempConfigService.SaveConfigAsync(testConfig);

                var translationService = new TranslationService(tempConfigService);
                var result = await translationService.TranslateAsync("Hello");

                if (result.Success)
                {
                    MessageBox.Show("API连接测试成功！", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"API连接测试失败: {result.ErrorMessage}", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"测试过程中出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _testConnectionButton.Enabled = true;
                _testConnectionButton.Text = "测试连接";
            }
        }
    }
}