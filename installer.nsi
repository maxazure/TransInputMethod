; TransInputMethod NSIS Installer Script
; 版本 0.1.0

!define APPNAME "TransInputMethod"
!define APPVERSION "0.1.0"
!define APPNAMEANDVERSION "${APPNAME} ${APPVERSION}"
!define APPCOMPANY "maxazure"
!define APPURL "https://github.com/maxazure/TransInputMethod"

; 安装程序属性
Name "${APPNAMEANDVERSION}"
OutFile "TransInputMethod-${APPVERSION}-Setup.exe"
InstallDir "$LOCALAPPDATA\${APPNAME}"
InstallDirRegKey HKCU "Software\${APPCOMPANY}\${APPNAME}" "InstallDir"
RequestExecutionLevel user

; 版本信息
VIProductVersion "${APPVERSION}.0"
VIAddVersionKey "ProductName" "${APPNAME}"
VIAddVersionKey "ProductVersion" "${APPVERSION}"
VIAddVersionKey "CompanyName" "${APPCOMPANY}"
VIAddVersionKey "FileDescription" "悬浮翻译输入法工具安装程序"
VIAddVersionKey "FileVersion" "${APPVERSION}.0"
VIAddVersionKey "LegalCopyright" "Copyright © ${APPCOMPANY} 2024"

; 现代UI
!include "MUI2.nsh"

; 界面设置
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

; 安装页面
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\${APPNAME}.exe"
!define MUI_FINISHPAGE_RUN_TEXT "运行 ${APPNAME}"
!insertmacro MUI_PAGE_FINISH

; 卸载页面
!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

; 语言
!insertmacro MUI_LANGUAGE "SimpChinese"

; 安装器部分
Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite on
  
  ; 主程序文件
  File "TransInputMethod-Portable\TransInputMethod.exe"
  File "TransInputMethod-Portable\TransInputMethod.dll"
  File "TransInputMethod-Portable\TransInputMethod.deps.json"
  File "TransInputMethod-Portable\TransInputMethod.runtimeconfig.json"
  
  ; 依赖文件
  File "TransInputMethod-Portable\Microsoft.Data.Sqlite.dll"
  File "TransInputMethod-Portable\Newtonsoft.Json.dll"
  File "TransInputMethod-Portable\e_sqlite3.dll"
  
  ; 运行时文件
  File "TransInputMethod-Portable\*.dll"
  File "TransInputMethod-Portable\*.exe"
  
  ; 语言资源
  SetOutPath "$INSTDIR\zh-Hans"
  File /r "TransInputMethod-Portable\zh-Hans\*"
  
  ; 其他必要的文件夹
  SetOutPath "$INSTDIR"
  File /r /x "*.txt" /x "*.bat" "TransInputMethod-Portable\*"
  
  ; 创建开始菜单快捷方式
  CreateDirectory "$SMPROGRAMS\${APPNAME}"
  CreateShortCut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\${APPNAME}.exe"
  CreateShortCut "$SMPROGRAMS\${APPNAME}\卸载 ${APPNAME}.lnk" "$INSTDIR\Uninstall.exe"
  
  ; 创建桌面快捷方式
  CreateShortCut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\${APPNAME}.exe"
  
  ; 写入注册表
  WriteRegStr HKCU "Software\${APPCOMPANY}\${APPNAME}" "InstallDir" "$INSTDIR"
  WriteRegStr HKCU "Software\${APPCOMPANY}\${APPNAME}" "Version" "${APPVERSION}"
  
  ; 写入卸载信息
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAMEANDVERSION}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "Publisher" "${APPCOMPANY}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "URLInfoAbout" "${APPURL}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${APPVERSION}"
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoModify" 1
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoRepair" 1
  
  ; 创建卸载程序
  WriteUninstaller "$INSTDIR\Uninstall.exe"
SectionEnd

; 卸载器部分
Section "Uninstall"
  ; 删除快捷方式
  Delete "$DESKTOP\${APPNAME}.lnk"
  Delete "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk"
  Delete "$SMPROGRAMS\${APPNAME}\卸载 ${APPNAME}.lnk"
  RMDir "$SMPROGRAMS\${APPNAME}"
  
  ; 删除程序文件
  RMDir /r "$INSTDIR"
  
  ; 删除注册表项
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
  DeleteRegKey HKCU "Software\${APPCOMPANY}\${APPNAME}"
  
  ; 询问是否删除用户数据
  MessageBox MB_YESNO|MB_ICONQUESTION "是否删除配置文件和翻译历史？$\n$\n选择"是"将完全删除所有数据$\n选择"否"将保留配置和历史记录" IDNO skip_data_deletion
  
  ; 删除配置文件和数据库
  Delete "$INSTDIR\config.json"
  Delete "$INSTDIR\config.key"
  Delete "$INSTDIR\translations.db"
  
  skip_data_deletion:
SectionEnd

; 部分描述
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC01} "${APPNAME} 主程序和必要文件"
!insertmacro MUI_FUNCTION_DESCRIPTION_END