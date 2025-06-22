@echo off
setlocal enabledelayedexpansion

echo =========================================
echo Creating TransInputMethod Portable Pack
echo =========================================

:: Set paths
set PROJECT_DIR=%~dp0
set PUBLISH_DIR=%PROJECT_DIR%bin\Release\net8.0-windows\win-x64\publish
set PORTABLE_DIR=%PROJECT_DIR%TransInputMethod-Portable
set ZIP_FILE=%PROJECT_DIR%TransInputMethod-v0.1.0-Portable.zip

:: Clean previous builds
echo Cleaning previous builds...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%PORTABLE_DIR%" rmdir /s /q "%PORTABLE_DIR%"
if exist "%ZIP_FILE%" del /q "%ZIP_FILE%"

:: Build and publish the main application (self-contained)
echo Building self-contained application...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true

if !errorlevel! neq 0 (
    echo ERROR: Failed to build application
    pause
    exit /b 1
)

:: Create portable directory structure
echo Creating portable package...
mkdir "%PORTABLE_DIR%"

:: Copy main files
copy "%PUBLISH_DIR%\*" "%PORTABLE_DIR%\"

:: Create README file for portable version
echo Creating README for portable version...
(
echo TransInputMethod v0.1.0 - Portable Version
echo ==========================================
echo.
echo This is a portable version of TransInputMethod that requires no installation.
echo.
echo QUICK START:
echo 1. Run TransInputMethod.exe
echo 2. Configure your API key in Settings
echo 3. Use Shift+Space to open translation window
echo 4. Use Ctrl+Enter to translate or copy
echo.
echo SYSTEM REQUIREMENTS:
echo - Windows 10/11
echo - .NET 8.0 Runtime ^(included in this package^)
echo.
echo For more information, visit:
echo https://github.com/maxazure/TransInputMethod
echo.
echo Copyright ^(c^) 2024 maxazure
echo Licensed under MIT License
) > "%PORTABLE_DIR%\README.txt"

:: Create a simple installer script
echo Creating setup script...
(
echo @echo off
echo echo Setting up TransInputMethod...
echo.
echo :: Create desktop shortcut
echo set DESKTOP=%%USERPROFILE%%\Desktop
echo set TARGET=%%~dp0TransInputMethod.exe
echo.
echo powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut(\"%%DESKTOP%%\TransInputMethod.lnk\"); $s.TargetPath = \"%%TARGET%%\"; $s.WorkingDirectory = \"%%~dp0\"; $s.Description = \"悬浮翻译输入法工具\"; $s.Save()"
echo.
echo :: Create start menu shortcut
echo set STARTMENU=%%APPDATA%%\Microsoft\Windows\Start Menu\Programs
echo if not exist "%%STARTMENU%%\TransInputMethod" mkdir "%%STARTMENU%%\TransInputMethod"
echo.
echo powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut(\"%%STARTMENU%%\TransInputMethod\TransInputMethod.lnk\"); $s.TargetPath = \"%%TARGET%%\"; $s.WorkingDirectory = \"%%~dp0\"; $s.Description = \"悬浮翻译输入法工具\"; $s.Save()"
echo.
echo echo Setup completed! You can now run TransInputMethod from:
echo echo - Desktop shortcut
echo echo - Start menu
echo echo - Or directly from this folder
echo echo.
echo pause
) > "%PORTABLE_DIR%\Setup.bat"

:: Create PowerShell script for modern installer
echo Creating PowerShell installer...
(
echo # TransInputMethod Setup Script
echo Write-Host "Setting up TransInputMethod..." -ForegroundColor Green
echo.
echo $currentDir = $PSScriptRoot
echo $targetPath = Join-Path $currentDir "TransInputMethod.exe"
echo.
echo # Create desktop shortcut
echo $desktop = [Environment]::GetFolderPath("Desktop"^)
echo $shortcutPath = Join-Path $desktop "TransInputMethod.lnk"
echo $shell = New-Object -ComObject WScript.Shell
echo $shortcut = $shell.CreateShortcut($shortcutPath^)
echo $shortcut.TargetPath = $targetPath
echo $shortcut.WorkingDirectory = $currentDir
echo $shortcut.Description = "悬浮翻译输入法工具"
echo $shortcut.Save()
echo.
echo # Create start menu shortcut
echo $startMenu = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\TransInputMethod"
echo if (-not (Test-Path $startMenu^)^) { New-Item -ItemType Directory -Path $startMenu -Force }
echo $startMenuShortcut = Join-Path $startMenu "TransInputMethod.lnk"
echo $shortcut2 = $shell.CreateShortcut($startMenuShortcut^)
echo $shortcut2.TargetPath = $targetPath
echo $shortcut2.WorkingDirectory = $currentDir
echo $shortcut2.Description = "悬浮翻译输入法工具"
echo $shortcut2.Save()
echo.
echo Write-Host "Setup completed successfully!" -ForegroundColor Green
echo Write-Host "You can now run TransInputMethod from:" -ForegroundColor Yellow
echo Write-Host "- Desktop shortcut" -ForegroundColor White
echo Write-Host "- Start menu" -ForegroundColor White
echo Write-Host "- Or directly: $targetPath" -ForegroundColor White
echo.
echo Read-Host "Press Enter to continue"
) > "%PORTABLE_DIR%\Setup.ps1"

:: Create uninstaller
echo Creating uninstaller...
(
echo @echo off
echo echo Removing TransInputMethod shortcuts...
echo.
echo :: Remove desktop shortcut
echo set DESKTOP=%%USERPROFILE%%\Desktop
echo if exist "%%DESKTOP%%\TransInputMethod.lnk" del "%%DESKTOP%%\TransInputMethod.lnk"
echo.
echo :: Remove start menu shortcuts
echo set STARTMENU=%%APPDATA%%\Microsoft\Windows\Start Menu\Programs\TransInputMethod
echo if exist "%%STARTMENU%%" rmdir /s /q "%%STARTMENU%%"
echo.
echo echo Shortcuts removed. You can manually delete this folder if desired.
echo pause
) > "%PORTABLE_DIR%\Uninstall.bat"

:: Create ZIP package using PowerShell
echo Creating ZIP package...
powershell -Command "Compress-Archive -Path '%PORTABLE_DIR%\*' -DestinationPath '%ZIP_FILE%' -Force"

if exist "%ZIP_FILE%" (
    echo SUCCESS: Portable package created at %ZIP_FILE%
    echo Package size:
    dir "%ZIP_FILE%" | findstr ".zip"
) else (
    echo ERROR: Failed to create ZIP package
    pause
    exit /b 1
)

echo.
echo =========================================
echo Portable package created successfully!
echo =========================================
echo.
echo Package location: %ZIP_FILE%
echo Folder location: %PORTABLE_DIR%
echo.
echo Users can:
echo 1. Extract the ZIP file anywhere
echo 2. Run Setup.bat or Setup.ps1 for shortcuts
echo 3. Or directly run TransInputMethod.exe
echo.
pause