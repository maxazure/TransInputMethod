# TransInputMethod 部署指南

## 安装包类型

本项目提供多种安装包格式，适合不同的部署需求：

### 1. 便携版 (Portable Version) ✅ 已完成

**文件夹**: `TransInputMethod-Portable/`
**大小**: 约 164MB
**特点**: 
- 自包含，无需安装 .NET Runtime
- 包含所有依赖项
- 可直接运行，无需安装
- 包含安装脚本 (`Setup.bat`) 用于创建快捷方式

**使用方法**:
1. 解压到任意目录
2. 运行 `Setup.bat` 创建快捷方式（可选）
3. 直接运行 `TransInputMethod.exe`

**文件说明**:
- `README.txt`: 使用说明
- `Setup.bat`: 快捷方式安装脚本
- `Uninstall.bat`: 快捷方式卸载脚本
- `TransInputMethod.exe`: 主程序

### 2. MSI 安装包 (Windows Installer) 🚧 部分完成

**文件**: `TransInputMethodSetup.msi`
**工具**: WiX Toolset
**状态**: 脚本已准备，需要 WiX 3.11 或 4.x

**构建方法**:
```bash
# 使用 WiX 3.11
build-installer-simple.bat

# 使用 WiX 4.x  
build-installer.bat
```

**特点**:
- 标准 Windows 安装体验
- 集成到系统卸载程序
- 自动创建快捷方式
- 支持升级和卸载

### 3. NSIS 安装包 📝 脚本已准备

**文件**: `TransInputMethod-0.1.0-Setup.exe`
**工具**: NSIS (Nullsoft Scriptable Install System)
**脚本**: `installer.nsi`

**构建方法**:
```bash
# 需要安装 NSIS 后执行
makensis installer.nsi
```

**特点**:
- 现代化安装界面
- 中文界面支持
- 可选卸载用户数据
- 体积较小

## 构建流程

### 前置要求
- .NET 8.0 SDK
- Windows 10/11 开发环境

### 1. 构建应用程序
```bash
# 自包含版本（推荐）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false

# 依赖框架版本
dotnet publish -c Release -r win-x64 --self-contained false
```

### 2. 创建便携版
```bash
# 手动复制文件
cp -r bin/Release/net8.0-windows/win-x64/publish/* TransInputMethod-Portable/

# 或使用脚本（需要修复）
create-portable.bat
```

### 3. 打包安装器

#### WiX 安装包
需要安装 [WiX Toolset](https://wixtoolset.org/):
```bash
# 安装 WiX 全局工具
dotnet tool install --global wix

# 构建 MSI
cd Installer
dotnet build
```

#### NSIS 安装包
需要安装 [NSIS](https://nsis.sourceforge.io/):
```bash
# 编译安装器
makensis installer.nsi
```

## 文件清单

### 核心文件
- `TransInputMethod.exe` - 主程序
- `TransInputMethod.dll` - 主程序库
- `TransInputMethod.deps.json` - 依赖配置
- `TransInputMethod.runtimeconfig.json` - 运行时配置

### 依赖库
- `Microsoft.Data.Sqlite.dll` - SQLite 数据库
- `Newtonsoft.Json.dll` - JSON 序列化
- `e_sqlite3.dll` - SQLite 原生库

### 运行时文件
- .NET 8.0 运行时文件 (自包含版本)
- Windows Forms 相关 DLL
- 多语言资源文件

### 资源文件
- `icon.ico` - 应用程序图标 (内嵌)

## 部署选项

### 开发测试
推荐使用便携版，快速部署和测试。

### 正式发布
推荐顺序:
1. **便携版** - 适合技术用户和绿色软件爱好者
2. **NSIS 安装包** - 适合普通用户，安装体验好
3. **MSI 安装包** - 适合企业部署和系统管理员

### 自动化部署
可以配置 GitHub Actions 自动构建:
- 在 Release 时自动构建所有格式
- 上传到 GitHub Releases
- 生成校验和文件

## 故障排除

### 常见问题

1. **缺少 .NET Runtime**
   - 解决方案: 使用自包含版本 (`--self-contained true`)

2. **SQLite 错误**
   - 检查 `e_sqlite3.dll` 是否存在
   - 确保文件权限正确

3. **WiX 构建失败**
   - 检查 WiX 版本兼容性
   - 确保发布路径正确

4. **图标显示问题**
   - 检查 `Resources/icon.ico` 文件
   - 确保嵌入资源配置正确

## 版本管理

当前版本: **0.1.0**

版本号规则: `major.minor.patch`
- major: 重大功能变更
- minor: 新功能添加
- patch: 错误修复

每次发布需要更新:
- `TransInputMethod.csproj` 中的版本号
- `installer.nsi` 中的 `APPVERSION`
- `Product.wxs` 中的版本号
- `README.md` 中的版本信息

## 发布清单

发布新版本时需要:
- [ ] 更新版本号
- [ ] 构建便携版
- [ ] 测试便携版功能
- [ ] 构建安装包 (可选)
- [ ] 测试安装和卸载
- [ ] 更新文档
- [ ] 创建 Git tag
- [ ] 发布到 GitHub Releases