@echo off
setlocal enabledelayedexpansion

echo =====================================
echo Building TransInputMethod Installer
echo =====================================

:: Set paths
set PROJECT_DIR=%~dp0
set INSTALLER_DIR=%PROJECT_DIR%Installer
set PUBLISH_DIR=%PROJECT_DIR%bin\Release\net8.0-windows\win-x64\publish
set WIX_DIR="C:\Program Files (x86)\WiX Toolset v3.11\bin"

:: Check if WiX is installed
if not exist %WIX_DIR%\candle.exe (
    echo ERROR: WiX Toolset v3.11 not found at %WIX_DIR%
    echo Please download and install WiX Toolset v3.11 from https://github.com/wixtoolset/wix3/releases
    pause
    exit /b 1
)

:: Clean previous builds
echo Cleaning previous builds...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%INSTALLER_DIR%\*.wixobj" del /q "%INSTALLER_DIR%\*.wixobj"
if exist "%INSTALLER_DIR%\*.msi" del /q "%INSTALLER_DIR%\*.msi"

:: Build and publish the main application
echo Building main application...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false

if !errorlevel! neq 0 (
    echo ERROR: Failed to build main application
    pause
    exit /b 1
)

:: Check if published files exist
if not exist "%PUBLISH_DIR%\TransInputMethod.exe" (
    echo ERROR: TransInputMethod.exe not found in publish directory
    echo Expected: %PUBLISH_DIR%\TransInputMethod.exe
    dir "%PUBLISH_DIR%"
    pause
    exit /b 1
)

:: Compile WiX files
echo Compiling WiX files...
cd /d "%INSTALLER_DIR%"
%WIX_DIR%\candle.exe -dSourceDir="%PUBLISH_DIR%" TransInputMethod.wxs

if !errorlevel! neq 0 (
    echo ERROR: Failed to compile WiX files
    pause
    exit /b 1
)

:: Link MSI
echo Linking MSI...
%WIX_DIR%\light.exe -ext WixUIExtension TransInputMethod.wixobj -out TransInputMethodSetup.msi

if !errorlevel! neq 0 (
    echo ERROR: Failed to link MSI
    pause
    exit /b 1
)

:: Copy the resulting MSI to the project root
if exist "TransInputMethodSetup.msi" (
    copy "TransInputMethodSetup.msi" "%PROJECT_DIR%"
    echo SUCCESS: MSI installer created at %PROJECT_DIR%TransInputMethodSetup.msi
) else (
    echo ERROR: MSI file not found
    pause
    exit /b 1
)

echo =====================================
echo Build completed successfully!
echo =====================================
pause