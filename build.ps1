# TransInputMethod 构建脚本 (PowerShell)
# 检查 .NET SDK 并构建发布版本

param(
    [switch]$CreateZip = $false,
    [string]$Configuration = "Release"
)

# 设置控制台编码
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TransInputMethod 构建脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查 .NET SDK
Write-Host "[1/6] 检查 .NET SDK 环境..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet 命令未找到"
    }
    Write-Host "✅ 找到 .NET SDK 版本: $dotnetVersion" -ForegroundColor Green
    
    # 检查版本是否为 8.0 或更高
    $majorVersion = [int]($dotnetVersion.Split('.')[0])
    if ($majorVersion -lt 8) {
        Write-Host "❌ .NET SDK 版本过低 (当前: $dotnetVersion)" -ForegroundColor Red
        Write-Host ""
        Write-Host "需要 .NET 8.0 或更高版本" -ForegroundColor Yellow
        Write-Host "下载地址: https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0" -ForegroundColor Cyan
        Write-Host ""
        Read-Host "按任意键退出"
        exit 1
    }
}
catch {
    Write-Host "❌ 未找到 .NET SDK" -ForegroundColor Red
    Write-Host ""
    Write-Host "请安装 .NET 8.0 SDK 或更高版本：" -ForegroundColor Yellow
    Write-Host "下载地址: https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "按任意键退出"
    exit 1
}

Write-Host ""

# 清理旧的构建文件
Write-Host "[2/6] 清理旧的构建文件..." -ForegroundColor Yellow
$itemsToRemove = @("bin", "obj", "TransInputMethod-Portable")
foreach ($item in $itemsToRemove) {
    if (Test-Path $item) {
        Remove-Item $item -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# 清理 ZIP 文件
Get-ChildItem -Path "." -Filter "TransInputMethod-*.zip" | Remove-Item -Force -ErrorAction SilentlyContinue

Write-Host "✅ 清理完成" -ForegroundColor Green
Write-Host ""

# 还原依赖项
Write-Host "[3/6] 还原 NuGet 依赖项..." -ForegroundColor Yellow
try {
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "还原失败"
    }
    Write-Host "✅ 依赖项还原完成" -ForegroundColor Green
}
catch {
    Write-Host "❌ 依赖项还原失败" -ForegroundColor Red
    Read-Host "按任意键退出"
    exit 1
}

Write-Host ""

# 构建项目
Write-Host "[4/6] 构建项目..." -ForegroundColor Yellow
try {
    dotnet build -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "构建失败"
    }
    Write-Host "✅ 项目构建完成" -ForegroundColor Green
}
catch {
    Write-Host "❌ 项目构建失败" -ForegroundColor Red
    Read-Host "按任意键退出"
    exit 1
}

Write-Host ""

# 发布自包含版本
Write-Host "[5/6] 发布自包含版本..." -ForegroundColor Yellow
try {
    dotnet publish -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=false --output "./TransInputMethod-Portable"
    if ($LASTEXITCODE -ne 0) {
        throw "发布失败"
    }
    Write-Host "✅ 发布完成" -ForegroundColor Green
}
catch {
    Write-Host "❌ 发布失败" -ForegroundColor Red
    Read-Host "按任意键退出"
    exit 1
}

Write-Host ""

# 创建便携版文件
Write-Host "[6/6] 创建便携版文件..." -ForegroundColor Yellow

# 获取版本号
$projectFile = "TransInputMethod.csproj"
if (Test-Path $projectFile) {
    $projectContent = Get-Content $projectFile
    $versionLine = $projectContent | Where-Object { $_ -match "<Version>" }
    if ($versionLine) {
        $appVersion = ($versionLine -split '<Version>|</Version>')[1]
    } else {
        $appVersion = "0.1.0"
    }
} else {
    $appVersion = "0.1.0"
}

# 创建 README.txt
$readmeContent = @"
TransInputMethod v$appVersion - Portable Version
==========================================

这是 TransInputMethod 的便携版，无需安装即可使用。

快速开始：
1. 双击运行 TransInputMethod.exe
2. 在设置中配置您的 API 密钥
3. 使用 Shift+Space 打开翻译窗口
4. 使用 Ctrl+Enter 进行翻译或复制

系统要求：
- Windows 10/11
- .NET 8.0 Runtime（已包含在此包中）

更多信息：
项目地址：https://github.com/maxazure/TransInputMethod

Copyright (c) 2024 maxazure
Licensed under MIT License
"@

$readmeContent | Out-File -FilePath "TransInputMethod-Portable/README.txt" -Encoding UTF8

# 创建安装脚本
$setupScript = @'
@echo off
chcp 65001 >nul
echo 正在设置 TransInputMethod...
echo.

set CURRENT_DIR=%~dp0
set TARGET_PATH=%CURRENT_DIR%TransInputMethod.exe

if not exist "%TARGET_PATH%" (
    echo 错误：找不到 TransInputMethod.exe
    pause
    exit /b 1
)

echo 创建桌面快捷方式...
set DESKTOP=%USERPROFILE%\Desktop
powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%DESKTOP%\TransInputMethod.lnk'); $s.TargetPath = '%TARGET_PATH%'; $s.WorkingDirectory = '%CURRENT_DIR%'; $s.Description = '悬浮翻译输入法工具'; $s.Save()"

echo 创建开始菜单快捷方式...
set STARTMENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs
if not exist "%STARTMENU%\TransInputMethod" mkdir "%STARTMENU%\TransInputMethod"
powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%STARTMENU%\TransInputMethod\TransInputMethod.lnk'); $s.TargetPath = '%TARGET_PATH%'; $s.WorkingDirectory = '%CURRENT_DIR%'; $s.Description = '悬浮翻译输入法工具'; $s.Save()"

echo.
echo 安装完成！您现在可以通过以下方式运行 TransInputMethod：
echo - 桌面快捷方式
echo - 开始菜单
echo - 或直接运行此文件夹中的 TransInputMethod.exe
echo.
pause
'@

$setupScript | Out-File -FilePath "TransInputMethod-Portable/Setup.bat" -Encoding UTF8

# 创建卸载脚本
$uninstallScript = @'
@echo off
chcp 65001 >nul
echo 正在移除 TransInputMethod 快捷方式...
echo.

set DESKTOP=%USERPROFILE%\Desktop
if exist "%DESKTOP%\TransInputMethod.lnk" (
    del "%DESKTOP%\TransInputMethod.lnk"
    echo 已删除桌面快捷方式
)

set STARTMENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs\TransInputMethod
if exist "%STARTMENU%" (
    rmdir /s /q "%STARTMENU%"
    echo 已删除开始菜单快捷方式
)

echo.
echo 快捷方式已删除。
echo 如需完全删除，请手动删除整个文件夹。
pause
'@

$uninstallScript | Out-File -FilePath "TransInputMethod-Portable/Uninstall.bat" -Encoding UTF8

Write-Host "✅ 便携版文件创建完成" -ForegroundColor Green

# 创建 ZIP 文件（可选）
if ($CreateZip) {
    Write-Host ""
    Write-Host "[额外] 创建 ZIP 压缩包..." -ForegroundColor Yellow
    $zipFileName = "TransInputMethod-v$appVersion-Portable.zip"
    try {
        Compress-Archive -Path "TransInputMethod-Portable/*" -DestinationPath $zipFileName -Force
        $zipSize = [math]::Round((Get-Item $zipFileName).Length / 1MB, 2)
        Write-Host "✅ ZIP 文件创建完成: $zipFileName (${zipSize}MB)" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️ ZIP 文件创建失败: $_" -ForegroundColor Yellow
    }
}

# 显示构建结果
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🎉 构建完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📁 便携版位置: TransInputMethod-Portable\" -ForegroundColor White
Write-Host "📋 程序入口: TransInputMethod-Portable\TransInputMethod.exe" -ForegroundColor White
Write-Host "🔧 安装脚本: TransInputMethod-Portable\Setup.bat" -ForegroundColor White
Write-Host "📖 使用说明: TransInputMethod-Portable\README.txt" -ForegroundColor White

if ($CreateZip) {
    Write-Host "📦 ZIP 文件: $zipFileName" -ForegroundColor White
}

Write-Host ""
Write-Host "💡 提示：运行 Setup.bat 可以创建桌面和开始菜单快捷方式" -ForegroundColor Yellow
Write-Host "💡 提示：使用 -CreateZip 参数可以同时创建 ZIP 压缩包" -ForegroundColor Yellow
Write-Host ""

# 询问是否打开构建结果
$choice = Read-Host "是否打开构建结果文件夹？ (y/N)"
if ($choice -eq 'y' -or $choice -eq 'Y') {
    try {
        Invoke-Item "TransInputMethod-Portable"
    }
    catch {
        Write-Host "无法打开文件夹: $_" -ForegroundColor Yellow
    }
}