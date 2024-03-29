﻿; 脚本用 Inno Setup 脚本向导 生成。
; 查阅文档获取创建 INNO SETUP 脚本文件的详细资料！
#define MyAppName "素材管理姬"
#define MyAppPublisher "NGMKS"
#define MyAppURL "https://github.com/nayaku/ImageManager"
#define MyAppExeName "ImageManager.exe"
#define ProjectPath "..\publish"
#define SetupIconFilePath  "icon.ico"
#define LicenseFilePath "..\license.txt"

#define AppVerText() \
   ParseVersion(ProjectPath+'\'+MyAppExeName, Local[0], Local[1], Local[2], Local[3]), \
   Str(Local[0]) + "." + Str(Local[1]) + "." + Str(Local[2])

[Setup]
; 注意: AppId 的值是唯一识别这个程序的标志。
; 不要在其他程序中使用相同的 AppId 值。
; (在编译器中点击菜单“工具 -> 产生 GUID”可以产生一个新的 GUID)
AppId={{C0B2618D-A2BC-4A8B-8600-5ED31A9A5583}
AppName={#MyAppName}
AppVersion={#AppVerText}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\ImageManager
DisableProgramGroupPage=yes
DefaultGroupName={#MyAppName}
OutputBaseFilename=ImageManager_{#AppVerText}
PrivilegesRequired=admin
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=commandline
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile={#SetupIconFilePath}
VersionInfoVersion={#AppVerText}

[Dirs]
Name: {app}; Permissions: users-full

[Languages]
Name: "chinesesimp"; MessagesFile: "Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkablealone
;Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "{#ProjectPath}\ImageManager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectPath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; 注意: 不要在任何共享的系统文件使用 "Flags: ignoreversion"

[Icons]
;Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
;Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
;Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
;Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


