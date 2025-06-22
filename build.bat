@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo TransInputMethod 构建脚本
echo ========================================
echo.

:: 检查 .NET SDK
echo [1/6] 检查 .NET SDK 环境...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 未找到 .NET SDK
    echo.
    echo 请安装 .NET 8.0 SDK 或更高版本：
    echo 下载地址: https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

:: 获取 .NET 版本
for /f "tokens=*" %%i in ('dotnet --version 2^>nul') do set DOTNET_VERSION=%%i
echo ✅ 找到 .NET SDK 版本: %DOTNET_VERSION%

:: 检查版本是否为 8.0 或更高
for /f "tokens=1 delims=." %%a in ("%DOTNET_VERSION%") do set MAJOR_VERSION=%%a
for /f "tokens=2 delims=." %%b in ("%DOTNET_VERSION%") do set MINOR_VERSION=%%b

if %MAJOR_VERSION% LSS 8 (
    echo ❌ .NET SDK 版本过低 (当前: %DOTNET_VERSION%)
    echo.
    echo 需要 .NET 8.0 或更高版本
    echo 下载地址: https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo.

:: 清理旧的构建文件
echo [2/6] 清理旧的构建文件...
if exist "bin" rmdir /s /q "bin" >nul 2>&1
if exist "obj" rmdir /s /q "obj" >nul 2>&1
if exist "TransInputMethod-Portable" rmdir /s /q "TransInputMethod-Portable" >nul 2>&1
if exist "TransInputMethod-*.zip" del /q "TransInputMethod-*.zip" >nul 2>&1
echo ✅ 清理完成

echo.

:: 还原依赖项
echo [3/6] 还原 NuGet 依赖项...
dotnet restore
if errorlevel 1 (
    echo ❌ 依赖项还原失败
    pause
    exit /b 1
)
echo ✅ 依赖项还原完成

echo.

:: 构建项目
echo [4/6] 构建项目...
dotnet build -c Release --no-restore
if errorlevel 1 (
    echo ❌ 项目构建失败
    pause
    exit /b 1
)
echo ✅ 项目构建完成

echo.

:: 发布自包含版本
echo [5/6] 发布自包含版本...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false --output "./TransInputMethod-Portable"
if errorlevel 1 (
    echo ❌ 发布失败
    pause
    exit /b 1
)
echo ✅ 发布完成

echo.

:: 创建便携版说明文件
echo [6/6] 创建便携版文件...

:: 获取版本号
for /f "tokens=*" %%i in ('findstr "<Version>" TransInputMethod.csproj') do set VERSION_LINE=%%i
for /f "tokens=2 delims=<>" %%j in ("!VERSION_LINE!") do set APP_VERSION=%%j

:: 创建 README.txt
echo TransInputMethod v%APP_VERSION% - Portable Version > "TransInputMethod-Portable\README.txt"
echo ========================================== >> "TransInputMethod-Portable\README.txt"
echo. >> "TransInputMethod-Portable\README.txt"
echo 这是 TransInputMethod 的便携版，无需安装即可使用。 >> "TransInputMethod-Portable\README.txt"
echo. >> "TransInputMethod-Portable\README.txt"
echo 快速开始： >> "TransInputMethod-Portable\README.txt"
echo 1. 双击运行 TransInputMethod.exe >> "TransInputMethod-Portable\README.txt"
echo 2. 在设置中配置您的 API 密钥 >> "TransInputMethod-Portable\README.txt"
echo 3. 使用 Shift+Space 打开翻译窗口 >> "TransInputMethod-Portable\README.txt"
echo 4. 使用 Ctrl+Enter 进行翻译或复制 >> "TransInputMethod-Portable\README.txt"
echo. >> "TransInputMethod-Portable\README.txt"
echo 系统要求： >> "TransInputMethod-Portable\README.txt"
echo - Windows 10/11 >> "TransInputMethod-Portable\README.txt"
echo - .NET 8.0 Runtime（已包含在此包中） >> "TransInputMethod-Portable\README.txt"
echo. >> "TransInputMethod-Portable\README.txt"
echo 更多信息： >> "TransInputMethod-Portable\README.txt"
echo 项目地址：https://github.com/maxazure/TransInputMethod >> "TransInputMethod-Portable\README.txt"
echo. >> "TransInputMethod-Portable\README.txt"
echo Copyright (c) 2024 maxazure >> "TransInputMethod-Portable\README.txt"
echo Licensed under MIT License >> "TransInputMethod-Portable\README.txt"

:: 创建安装脚本
echo @echo off > "TransInputMethod-Portable\Setup.bat"
echo chcp 65001 ^>nul >> "TransInputMethod-Portable\Setup.bat"
echo echo 正在设置 TransInputMethod... >> "TransInputMethod-Portable\Setup.bat"
echo echo. >> "TransInputMethod-Portable\Setup.bat"
echo. >> "TransInputMethod-Portable\Setup.bat"
echo set CURRENT_DIR=%%~dp0 >> "TransInputMethod-Portable\Setup.bat"
echo set TARGET_PATH=%%CURRENT_DIR%%TransInputMethod.exe >> "TransInputMethod-Portable\Setup.bat"
echo. >> "TransInputMethod-Portable\Setup.bat"
echo if not exist "%%TARGET_PATH%%" ^( >> "TransInputMethod-Portable\Setup.bat"
echo     echo 错误：找不到 TransInputMethod.exe >> "TransInputMethod-Portable\Setup.bat"
echo     pause >> "TransInputMethod-Portable\Setup.bat"
echo     exit /b 1 >> "TransInputMethod-Portable\Setup.bat"
echo ^) >> "TransInputMethod-Portable\Setup.bat"
echo. >> "TransInputMethod-Portable\Setup.bat"
echo echo 创建桌面快捷方式... >> "TransInputMethod-Portable\Setup.bat"
echo set DESKTOP=%%USERPROFILE%%\Desktop >> "TransInputMethod-Portable\Setup.bat"
echo powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%%DESKTOP%%\TransInputMethod.lnk'); $s.TargetPath = '%%TARGET_PATH%%'; $s.WorkingDirectory = '%%CURRENT_DIR%%'; $s.Description = '悬浮翻译输入法工具'; $s.Save()" >> "TransInputMethod-Portable\Setup.bat"
echo. >> "TransInputMethod-Portable\Setup.bat"
echo echo 创建开始菜单快捷方式... >> "TransInputMethod-Portable\Setup.bat"
echo set STARTMENU=%%APPDATA%%\Microsoft\Windows\Start Menu\Programs >> "TransInputMethod-Portable\Setup.bat"
echo if not exist "%%STARTMENU%%\TransInputMethod" mkdir "%%STARTMENU%%\TransInputMethod" >> "TransInputMethod-Portable\Setup.bat"
echo powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%%STARTMENU%%\TransInputMethod\TransInputMethod.lnk'); $s.TargetPath = '%%TARGET_PATH%%'; $s.WorkingDirectory = '%%CURRENT_DIR%%'; $s.Description = '悬浮翻译输入法工具'; $s.Save()" >> "TransInputMethod-Portable\Setup.bat"
echo. >> "TransInputMethod-Portable\Setup.bat"
echo echo. >> "TransInputMethod-Portable\Setup.bat"
echo echo 安装完成！您现在可以通过以下方式运行 TransInputMethod： >> "TransInputMethod-Portable\Setup.bat"
echo echo - 桌面快捷方式 >> "TransInputMethod-Portable\Setup.bat"
echo echo - 开始菜单 >> "TransInputMethod-Portable\Setup.bat"
echo echo - 或直接运行此文件夹中的 TransInputMethod.exe >> "TransInputMethod-Portable\Setup.bat"
echo echo. >> "TransInputMethod-Portable\Setup.bat"
echo pause >> "TransInputMethod-Portable\Setup.bat"

:: 创建卸载脚本
echo @echo off > "TransInputMethod-Portable\Uninstall.bat"
echo chcp 65001 ^>nul >> "TransInputMethod-Portable\Uninstall.bat"
echo echo 正在移除 TransInputMethod 快捷方式... >> "TransInputMethod-Portable\Uninstall.bat"
echo echo. >> "TransInputMethod-Portable\Uninstall.bat"
echo. >> "TransInputMethod-Portable\Uninstall.bat"
echo set DESKTOP=%%USERPROFILE%%\Desktop >> "TransInputMethod-Portable\Uninstall.bat"
echo if exist "%%DESKTOP%%\TransInputMethod.lnk" ^( >> "TransInputMethod-Portable\Uninstall.bat"
echo     del "%%DESKTOP%%\TransInputMethod.lnk" >> "TransInputMethod-Portable\Uninstall.bat"
echo     echo 已删除桌面快捷方式 >> "TransInputMethod-Portable\Uninstall.bat"
echo ^) >> "TransInputMethod-Portable\Uninstall.bat"
echo. >> "TransInputMethod-Portable\Uninstall.bat"
echo set STARTMENU=%%APPDATA%%\Microsoft\Windows\Start Menu\Programs\TransInputMethod >> "TransInputMethod-Portable\Uninstall.bat"
echo if exist "%%STARTMENU%%" ^( >> "TransInputMethod-Portable\Uninstall.bat"
echo     rmdir /s /q "%%STARTMENU%%" >> "TransInputMethod-Portable\Uninstall.bat"
echo     echo 已删除开始菜单快捷方式 >> "TransInputMethod-Portable\Uninstall.bat"
echo ^) >> "TransInputMethod-Portable\Uninstall.bat"
echo. >> "TransInputMethod-Portable\Uninstall.bat"
echo echo. >> "TransInputMethod-Portable\Uninstall.bat"
echo echo 快捷方式已删除。 >> "TransInputMethod-Portable\Uninstall.bat"
echo echo 如需完全删除，请手动删除整个文件夹。 >> "TransInputMethod-Portable\Uninstall.bat"
echo pause >> "TransInputMethod-Portable\Uninstall.bat"

echo ✅ 便携版文件创建完成

echo.
echo ========================================
echo 🎉 构建完成！
echo ========================================
echo.
echo 📁 便携版位置: TransInputMethod-Portable\
echo 📋 程序入口: TransInputMethod-Portable\TransInputMethod.exe
echo 🔧 安装脚本: TransInputMethod-Portable\Setup.bat
echo 📖 使用说明: TransInputMethod-Portable\README.txt
echo.
echo 💡 提示：运行 Setup.bat 可以创建桌面和开始菜单快捷方式
echo.
pause