using TransInputMethod.Data;
using TransInputMethod.Models;
using System.Drawing;
using System.Windows.Forms;

namespace TransInputMethod.Forms
{
    public partial class HistoryForm : Form
    {
        private readonly TranslationDbContext _dbContext;
        
        private Panel _searchPanel;
        private TextBox _searchTextBox;
        private Button _searchButton;
        private Panel _historyPanel;
        private FlowLayoutPanel _historyContainer;
        private Panel _navigationPanel;
        private Button _previousPageButton;
        private Button _nextPageButton;
        private Label _pageInfoLabel;
        
        private PagedResult<TranslationHistory> _currentResult = new PagedResult<TranslationHistory>();
        private int _currentPage = 1;
        private int _pageSize = 10;
        private string _currentSearchText = string.Empty;

        public HistoryForm(TranslationDbContext dbContext)
        {
            _dbContext = dbContext;
            InitializeComponent();
            LoadHistoryAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form settings
            this.Text = "翻译历史记录";
            this.Size = new Size(700, 600);
            this.MinimumSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei UI", 9F);

            // Search panel
            _searchPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(10)
            };

            _searchTextBox = new TextBox
            {
                Size = new Size(400, 25),
                Location = new Point(10, 12),
                PlaceholderText = "搜索翻译记录...",
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            _searchTextBox.KeyDown += SearchTextBox_KeyDown;

            _searchButton = new Button
            {
                Text = "搜索",
                Size = new Size(60, 25),
                Location = new Point(420, 12),
                FlatStyle = FlatStyle.System,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            _searchButton.Click += SearchButton_Click;

            _searchPanel.Controls.AddRange(new Control[] { _searchTextBox, _searchButton });

            // History panel (scrollable container)
            _historyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            _historyContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top
            };

            _historyPanel.Controls.Add(_historyContainer);

            // Navigation panel
            _navigationPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            _previousPageButton = new Button
            {
                Text = "上一页",
                Size = new Size(80, 30),
                Location = new Point(20, 10),
                Enabled = false,
                FlatStyle = FlatStyle.System,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            _previousPageButton.Click += PreviousPageButton_Click;

            _nextPageButton = new Button
            {
                Text = "下一页",
                Size = new Size(80, 30),
                Location = new Point(110, 10),
                Enabled = false,
                FlatStyle = FlatStyle.System,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            _nextPageButton.Click += NextPageButton_Click;

            _pageInfoLabel = new Label
            {
                Size = new Size(200, 30),
                Location = new Point(220, 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.Gray
            };

            _navigationPanel.Controls.AddRange(new Control[] 
            { 
                _previousPageButton, _nextPageButton, _pageInfoLabel 
            });

            // Add all panels to form
            this.Controls.Add(_historyPanel);
            this.Controls.Add(_searchPanel);
            this.Controls.Add(_navigationPanel);

            this.ResumeLayout(false);
        }

        private async void LoadHistoryAsync()
        {
            try
            {
                _currentResult = await _dbContext.GetTranslationHistoryAsync(_currentPage, _pageSize, _currentSearchText);
                UpdateHistoryDisplay();
                UpdateNavigationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载历史记录失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateHistoryDisplay()
        {
            _historyContainer.Controls.Clear();

            if (_currentResult.Data.Count == 0)
            {
                var noDataLabel = new Label
                {
                    Text = string.IsNullOrEmpty(_currentSearchText) ? "暂无翻译记录" : "未找到匹配的记录",
                    Font = new Font("Microsoft YaHei UI", 10F),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Margin = new Padding(0, 50, 0, 0)
                };
                _historyContainer.Controls.Add(noDataLabel);
                return;
            }

            foreach (var item in _currentResult.Data)
            {
                var itemPanel = CreateHistoryItemPanel(item);
                _historyContainer.Controls.Add(itemPanel);
            }
        }

        private Panel CreateHistoryItemPanel(TranslationHistory item)
        {
            var panel = new Panel
            {
                Width = _historyPanel.Width - 50,
                Height = 120,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(0, 5, 0, 5),
                Cursor = Cursors.Hand
            };

            // Source text label
            var sourceLabel = new Label
            {
                Text = $"原文: {item.SourceText}",
                Location = new Point(10, 10),
                Size = new Size(panel.Width - 100, 25),
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.Black,
                AutoEllipsis = true
            };

            // Translated text label
            var translatedLabel = new Label
            {
                Text = $"译文: {item.TranslatedText}",
                Location = new Point(10, 40),
                Size = new Size(panel.Width - 100, 25),
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.DarkBlue,
                AutoEllipsis = true
            };

            // Time and scenario info
            var infoLabel = new Label
            {
                Text = $"{item.Timestamp:yyyy-MM-dd HH:mm:ss}  |  {item.TranslationScenario ?? "默认"}",
                Location = new Point(10, 70),
                Size = new Size(panel.Width - 100, 20),
                Font = new Font("Microsoft YaHei UI", 8F),
                ForeColor = Color.Gray
            };

            // Copy buttons
            var copySourceButton = new Button
            {
                Text = "复制原文",
                Size = new Size(70, 25),
                Location = new Point(panel.Width - 85, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Microsoft YaHei UI", 8F),
                Cursor = Cursors.Hand
            };
            copySourceButton.FlatAppearance.BorderSize = 1;
            copySourceButton.Click += (s, e) => CopyToClipboard(item.SourceText, "原文已复制");

            var copyTranslatedButton = new Button
            {
                Text = "复制译文",
                Size = new Size(70, 25),
                Location = new Point(panel.Width - 85, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Microsoft YaHei UI", 8F),
                Cursor = Cursors.Hand
            };
            copyTranslatedButton.FlatAppearance.BorderSize = 1;
            copyTranslatedButton.Click += (s, e) => CopyToClipboard(item.TranslatedText, "译文已复制");

            var copyBothButton = new Button
            {
                Text = "复制全部",
                Size = new Size(70, 25),
                Location = new Point(panel.Width - 85, 70),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 240, 255),
                Font = new Font("Microsoft YaHei UI", 8F),
                Cursor = Cursors.Hand
            };
            copyBothButton.FlatAppearance.BorderSize = 1;
            copyBothButton.Click += (s, e) => CopyToClipboard($"原文: {item.SourceText}\n译文: {item.TranslatedText}", "全部内容已复制");

            // Add hover effects
            panel.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(250, 250, 255);
            panel.MouseLeave += (s, e) => panel.BackColor = Color.White;

            panel.Controls.AddRange(new Control[] 
            { 
                sourceLabel, translatedLabel, infoLabel, 
                copySourceButton, copyTranslatedButton, copyBothButton 
            });

            return panel;
        }

        private void CopyToClipboard(string text, string message)
        {
            try
            {
                Clipboard.SetText(text);
                
                // Show temporary success message
                var originalTitle = this.Text;
                this.Text = message;
                
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 1500; // 1.5 seconds
                timer.Tick += (s, e) =>
                {
                    this.Text = originalTitle;
                    timer.Dispose();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateNavigationButtons()
        {
            _previousPageButton.Enabled = _currentResult.HasPrevious;
            _nextPageButton.Enabled = _currentResult.HasNext;
            
            var searchInfo = string.IsNullOrEmpty(_currentSearchText) ? "" : $" (搜索: {_currentSearchText})";
            _pageInfoLabel.Text = $"第 {_currentResult.CurrentPage} 页，共 {_currentResult.TotalPages} 页，共 {_currentResult.TotalCount} 条记录{searchInfo}";
        }

        private async void SearchTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                await PerformSearch();
            }
        }

        private async void SearchButton_Click(object? sender, EventArgs e)
        {
            await PerformSearch();
        }

        private async Task PerformSearch()
        {
            _currentSearchText = _searchTextBox.Text.Trim();
            _currentPage = 1; // Reset to first page
            LoadHistoryAsync();
        }

        private async void PreviousPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentResult.HasPrevious)
            {
                _currentPage--;
                LoadHistoryAsync();
            }
        }

        private async void NextPageButton_Click(object? sender, EventArgs e)
        {
            if (_currentResult.HasNext)
            {
                _currentPage++;
                LoadHistoryAsync();
            }
        }

        private async void HistoryForm_Shown(object? sender, EventArgs e)
        {
            LoadHistoryAsync();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LoadHistoryAsync();
        }
    }
}