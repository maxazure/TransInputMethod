# 浮动翻译输入法工具

一款轻量级、高效的全局翻译工具，支持中英文互译，像输入法一样随时呼出使用。

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/maxazure/TransInputMethod/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://github.com/maxazure/TransInputMethod/releases)
[![GitHub release](https://img.shields.io/github/v/release/maxazure/TransInputMethod)](https://github.com/maxazure/TransInputMethod/releases)
[![GitHub stars](https://img.shields.io/github/stars/maxazure/TransInputMethod)](https://github.com/maxazure/TransInputMethod/stargazers)
[![GitHub issues](https://img.shields.io/github/issues/maxazure/TransInputMethod)](https://github.com/maxazure/TransInputMethod/issues)

## ✨ 特性

- 🚀 **全局快捷键**：随时随地按 `Shift+Space` 呼出翻译窗口
- 🎯 **智能翻译**：自动检测语言，中英文互译
- 📋 **一键复制**：翻译后再次按 `Ctrl+Enter` 直接复制译文
- 🎨 **浮动设计**：无边框窗体，跟随光标位置显示
- 📚 **场景切换**：支持IT办公口语、正式书面、IT技术交流等专业翻译场景
- 🗃️ **历史记录**：本地存储翻译历史，支持搜索和分页
- 🔗 **多API支持**：支持OpenAI、DeepSeek、Claude等多个AI服务商
- 🔐 **安全加密**：API密钥本地AES加密存储
- 🎛️ **个性化设置**：自定义透明度、窗口大小等界面选项

## 🖼️ 界面预览

### 浮动翻译窗体
轻量级无边框设计，智能跟随光标位置：

### 历史记录管理
完整的翻译历史，支持搜索和一键复制：

### 设置管理界面
简洁的标签式设置界面，功能全面：

## 🚀 快速开始

### 系统要求

- Windows 10/11
- .NET 8.0 Runtime

### 方式一：下载预编译版本（推荐）

1. **下载最新版本**
   - 访问 [Releases 页面](https://github.com/maxazure/TransInputMethod/releases)
   - 下载适合您系统的版本
   - 解压后直接运行 `TransInputMethod.exe`

### 方式二：从源码构建

1. **克隆项目**
   ```bash
   git clone https://github.com/maxazure/TransInputMethod.git
   cd TransInputMethod
   ```

2. **构建项目**
   
   **使用构建脚本（推荐）**：
   ```bash
   # Windows 批处理脚本（双击运行或命令行执行）
   build.bat
   
   # PowerShell 脚本
   .\build.ps1
   
   # PowerShell 脚本 + 创建ZIP压缩包
   .\build.ps1 -CreateZip
   ```
   
   构建脚本会自动：
   - ✅ 检查 .NET SDK 8.0+ 环境
   - 🧹 清理旧构建文件
   - 📦 还原依赖项并构建项目
   - 🚀 发布自包含版本到 `TransInputMethod-Portable/`
   - 📋 创建说明文件、安装脚本和卸载脚本
   
   **手动构建**：
   ```bash
   # 开发调试
   dotnet build
   dotnet run
   
   # 发布版本
   dotnet publish -c Release -r win-x64 --self-contained true --output "./TransInputMethod-Portable"
   ```

3. **运行应用**
   ```bash
   # 开发模式
   dotnet run
   
   # 或运行构建后的便携版
   .\TransInputMethod-Portable\TransInputMethod.exe
   ```

### 首次配置

1. 应用启动后，在系统托盘找到翻译工具图标
2. 右键图标选择"设置"
3. 在API设置页面配置：
   - **服务商**：选择API服务商（OpenAI、DeepSeek、Claude等）
   - **Base URL**：API服务地址（根据服务商自动填充）
   - **API Key**：对应服务商的API密钥
   - **Model**：选择使用的模型（推荐：`gpt-4.1-nano`）
4. 点击"测试连接"验证配置
5. 保存设置

## 📖 使用指南

### 基本翻译流程

1. **呼出翻译窗口**
   - 在任意应用中按 `Shift+Space`
   - 窗口会在光标附近显示

2. **进行翻译**
   - 输入要翻译的文本
   - 按 `Ctrl+Enter` 开始翻译
   - 翻译结果会替换输入框中的文本

3. **复制译文**
   - 在相同文本下再次按 `Ctrl+Enter`
   - 译文会自动复制到剪贴板

### 高级功能

#### 翻译场景切换
- 窗口中的下拉菜单可切换翻译场景
- **IT办公口语**：贴合新西兰职场/会议/邮件的自然表达
- **正式书面**：正式公文/报告/合同的书面规范
- **IT技术交流**：代码注释、技术博客、API文档等专业内容

#### 历史记录管理
- 点击窗口左下角的 📋 图标打开历史记录窗口
- 支持按原文或译文搜索
- 可分别复制原文、译文或全部内容
- 支持分页浏览，每页可显示10条记录

### 快捷键说明

| 快捷键 | 功能 |
|--------|------|
| `Shift+Space` | 呼出翻译窗口 |
| `Ctrl+Enter` | 翻译文本/复制译文 |
| `Esc` | 隐藏翻译窗口 |

## ⚙️ 配置选项

### API设置
- **服务商选择**：内置OpenAI、DeepSeek、Claude等主流服务商
- **Base URL**：支持自定义API服务地址，兼容OpenAI协议的服务
- **API Key**：加密存储，支持显示/隐藏切换
- **Model**：根据服务商显示对应的可用模型
- **组织ID**：支持OpenAI企业账户的组织ID配置
- **Timeout**：请求超时时间设置（5-120秒）

### 界面设置
- **窗口宽度**：调整浮动窗口宽度（400-1000px）
- **透明度**：设置窗口透明度（50-100%）
- **历史页大小**：设置历史记录每页显示数量

### 场景管理
- 内置专业场景：IT办公口语、正式书面、IT技术交流等
- 支持自定义翻译场景和提示词
- 可编辑场景名称和对应的提示词
- 支持设置默认场景

## 🗂️ 项目结构

```
TransInputMethod/
├── Forms/                     # 窗体文件
│   ├── FloatingTranslationForm.cs    # 主浮动翻译窗体
│   ├── HistoryForm.cs                # 历史记录窗体
│   └── SettingsForm.cs               # 设置窗体
├── Models/                    # 数据模型
│   ├── AppConfig.cs                  # 应用配置模型
│   └── TranslationHistory.cs         # 翻译历史模型
├── Services/                  # 业务服务
│   ├── AppController.cs              # 应用控制器
│   ├── ConfigService.cs              # 配置管理服务
│   └── TranslationService.cs         # 翻译服务
├── Data/                      # 数据访问
│   └── TranslationDbContext.cs       # 数据库上下文
├── Utils/                     # 工具类
│   └── GlobalHotkey.cs               # 全局热键管理
└── Program.cs                 # 程序入口
```

## 🔧 技术栈

- **框架**：.NET 8.0 WinForms
- **数据库**：SQLite（Microsoft.Data.Sqlite）
- **序列化**：Newtonsoft.Json
- **加密**：System.Security.Cryptography（AES）
- **API**：OpenAI ChatGPT API

## 🛡️ 安全特性

- **本地存储**：所有数据存储在本地，不上传到第三方服务器
- **加密保护**：API密钥和敏感配置使用AES加密存储
- **隐私保护**：翻译历史仅存储在本地SQLite数据库中
- **权限最小化**：仅申请必要的系统权限

## 📊 数据存储

### 配置文件
- `config.json`：加密的应用配置文件
- `config.key`：AES加密密钥文件（隐藏属性）

### 数据库
- `translations.db`：SQLite数据库文件
- 包含翻译历史、时间戳、语言信息等

### 数据库结构
```sql
CREATE TABLE history (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    source_text TEXT NOT NULL,
    translated_text TEXT NOT NULL,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    source_language TEXT,
    target_language TEXT,
    translation_scenario TEXT
);
```

## 🚀 发布部署

### 开发环境运行
```bash
dotnet run
```

### 发布独立可执行文件
```bash
# 发布为单文件可执行程序
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# 发布依赖框架的版本（需要安装.NET 8 Runtime）
dotnet publish -c Release -r win-x64 --self-contained false
```

## 🤝 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork [本项目](https://github.com/maxazure/TransInputMethod)
2. 创建功能分支：`git checkout -b feature/AmazingFeature`
3. 提交更改：`git commit -m 'Add some AmazingFeature'`
4. 推送分支：`git push origin feature/AmazingFeature`
5. 提交 Pull Request

## 📝 更新日志

### v0.1.0 (2024-12-22)
- ✨ 首次发布
- 🚀 支持全局快捷键呼出翻译窗口（默认：Shift+Space）
- 🎯 实现中英文智能互译
- 📋 添加一键复制功能
- 🗃️ 完整的历史记录管理
- 🔗 多API服务商支持（OpenAI、DeepSeek、Claude等）
- 🎨 现代化UI设计，优化用户体验
- ⚙️ 丰富的个性化设置选项
- 🔐 安全的配置信息加密存储

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详细信息。

## 💬 联系支持

如果您遇到问题或有功能建议，请通过以下方式联系：

- 📧 Email: maxazure@gmail.com
- 🐛 Issues: [GitHub Issues](https://github.com/maxazure/TransInputMethod/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/maxazure/TransInputMethod/discussions)

## 🙏 致谢

- 感谢 OpenAI 提供强大的翻译API服务
- 感谢 .NET 团队提供优秀的开发框架
- 感谢所有贡献者和用户的支持

---

<div align="center">

**如果这个项目对您有帮助，请考虑给它一个 ⭐ Star！**

Made with ❤️ by [maxazure](https://github.com/maxazure)

</div>