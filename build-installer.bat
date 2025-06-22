@echo off
setlocal enabledelayedexpansion

echo =====================================
echo Building TransInputMethod Installer
echo =====================================

:: Set paths
set PROJECT_DIR=%~dp0
set INSTALLER_DIR=%PROJECT_DIR%Installer
set PUBLISH_DIR=%PROJECT_DIR%bin\Release\net8.0-windows\win-x64\publish

:: Clean previous builds
echo Cleaning previous builds...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%INSTALLER_DIR%\bin" rmdir /s /q "%INSTALLER_DIR%\bin"

:: Build and publish the main application
echo Building main application...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false

if !errorlevel! neq 0 (
    echo ERROR: Failed to build main application
    exit /b 1
)

:: Check if published files exist
if not exist "%PUBLISH_DIR%\TransInputMethod.exe" (
    echo ERROR: TransInputMethod.exe not found in publish directory
    echo Expected: %PUBLISH_DIR%\TransInputMethod.exe
    dir "%PUBLISH_DIR%"
    exit /b 1
)

:: Install WiX if not already installed
echo Installing WiX toolset...
dotnet tool install --global wix --version 4.0.4 >nul 2>&1
if !errorlevel! neq 0 (
    echo WiX already installed or installation failed, continuing...
)

:: Build the installer
echo Building MSI installer...
cd /d "%INSTALLER_DIR%"
dotnet build

if !errorlevel! neq 0 (
    echo ERROR: Failed to build installer
    exit /b 1
)

:: Copy the resulting MSI to the project root
if exist "%INSTALLER_DIR%\bin\Release\TransInputMethodSetup.msi" (
    copy "%INSTALLER_DIR%\bin\Release\TransInputMethodSetup.msi" "%PROJECT_DIR%"
    echo SUCCESS: MSI installer created at %PROJECT_DIR%TransInputMethodSetup.msi
) else (
    echo ERROR: MSI file not found
    exit /b 1
)

echo =====================================
echo Build completed successfully!
echo =====================================
pause