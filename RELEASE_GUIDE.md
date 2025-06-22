# 🚀 TransInputMethod 发布指南

## 📦 发布策略总结

### ✅ 推荐方案：GitHub Releases + GitHub Actions

**优势：**
- 🎯 自动化构建和发布
- 📊 下载统计和版本管理
- 🔄 支持多种安装包格式
- 💾 不占用仓库存储空间
- 🏷️ 清晰的版本标签管理

## 🛠️ 自动化 CI/CD 流程

### 1. 代码提交触发构建
```bash
# 每次推送到 main 分支会触发构建测试
git push origin main
```

### 2. 发布新版本
```bash
# 创建并推送版本标签，触发自动发布
git tag v0.1.0
git push origin v0.1.0
```

### 3. 自动化流程
GitHub Actions 会自动：
1. ✅ 构建应用程序
2. 📦 创建便携版和依赖版本
3. 🗜️ 生成 ZIP 压缩包
4. 🔐 生成 SHA256 校验和
5. 📝 创建 GitHub Release
6. ⬆️ 上传所有安装包

## 📁 文件放置位置

### ❌ 不要放在仓库中：
```
TransInputMethod/
├── releases/              # ❌ 不要创建这个文件夹
├── dist/                  # ❌ 不要放编译产物
├── build/                 # ❌ 不要提交构建文件
└── *.zip                  # ❌ 不要提交安装包
```

### ✅ 正确的做法：
- 📤 **GitHub Releases**: 自动上传到 `https://github.com/maxazure/TransInputMethod/releases`
- 🏷️ **版本标签**: 使用 Git tags (`v0.1.0`, `v0.2.0` 等)
- 📋 **发布说明**: 自动生成详细的 Release Notes

## 🎯 发布类型和用途

| 安装包类型 | 文件名 | 大小 | 用途 |
|------------|--------|------|------|
| **便携版** | `TransInputMethod-v0.1.0-Portable.zip` | ~150MB | 普通用户推荐，解压即用 |
| **依赖版** | `TransInputMethod-v0.1.0-Framework.zip` | ~10MB | 已安装.NET的用户 |
| **校验和** | `checksums.txt` | <1KB | 文件完整性验证 |

## 📝 发布步骤详解

### 第一次设置（仅需一次）
1. 提交 GitHub Actions 配置文件到仓库
2. 确保 `.gitignore` 排除编译产物
3. 验证 GitHub 仓库权限

### 日常发布流程

#### 1. 准备发布
```bash
# 1. 确保代码已提交
git add .
git commit -m "准备发布 v0.1.0"
git push origin main

# 2. 更新版本号（在项目文件中）
# 编辑 TransInputMethod.csproj 中的版本号
```

#### 2. 创建发布
```bash
# 3. 创建版本标签
git tag -a v0.1.0 -m "Release version 0.1.0"

# 4. 推送标签触发自动发布
git push origin v0.1.0
```

#### 3. 验证发布
1. 🔍 访问 [Actions 页面](https://github.com/maxazure/TransInputMethod/actions) 查看构建状态
2. ✅ 等待构建完成（约5-10分钟）
3. 📦 检查 [Releases 页面](https://github.com/maxazure/TransInputMethod/releases) 的新版本

## 🔧 高级配置

### 自定义发布说明
编辑 `.github/workflows/release.yml` 中的 `body` 部分：
```yaml
body: |
  ## 🎉 新功能
  - 添加了新的翻译场景
  - 优化了界面响应速度
  
  ## 🐛 修复
  - 修复了快捷键冲突问题
  - 解决了内存泄漏
```

### 预发布版本
```bash
# 创建预发布标签
git tag -a v0.1.0-beta -m "Beta release"
git push origin v0.1.0-beta
```

### 手动发布（备用方案）
如果自动化失败，可以手动操作：
1. 本地运行 `dotnet publish`
2. 手动创建 ZIP 文件
3. 在 GitHub 创建 Release 并上传

## 📊 版本管理策略

### 版本号规则
- `v0.1.0` - 首个正式版本
- `v0.1.1` - 修复版本
- `v0.2.0` - 新功能版本
- `v1.0.0` - 重大版本

### 分支策略
```
main          ├─────●─────●─────● (稳定版本)
              │     │     │
develop       ├─●─●─┴─●─●─┴─●─●─● (开发版本)
              │
feature/*     └─●─●─┘           (功能分支)
```

## 🎯 用户下载体验

用户访问：`https://github.com/maxazure/TransInputMethod/releases/latest`

看到：
```
TransInputMethod v0.1.0
发布时间：2024-12-22

📦 下载选项：
□ TransInputMethod-v0.1.0-Portable.zip (150MB) - 推荐
□ TransInputMethod-v0.1.0-Framework.zip (10MB) - 需要.NET 8.0
□ checksums.txt - 校验和文件

🚀 快速开始：下载便携版 → 解压 → 运行 Setup.bat → 开始使用
```

## 🔍 监控和分析

GitHub 提供的数据：
- 📈 下载次数统计
- 🌍 地理分布
- 📱 平台分析
- ⭐ Star 和 Fork 趋势

## ❓ 常见问题

**Q: GitHub Actions 构建失败怎么办？**
A: 检查 Actions 页面的错误日志，通常是依赖或权限问题。

**Q: 可以修改已发布的版本吗？**
A: 可以编辑 Release 说明，但不建议替换已下载的文件。

**Q: 如何撤回错误的发布？**
A: 可以删除 Release 和对应的 Git tag。

---

🎉 **恭喜！现在你有了一个完全自动化的发布系统！**