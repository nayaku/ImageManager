; 脚本用 Inno Setup 脚本向导 生成。
; 查阅文档获取创建 INNO SETUP 脚本文件的详细资料！

#define MyAppName "素材管理器"
#define MyAppVersion "2.0.5"
#define MyAppPublisher "NGMKS"
#define MyAppURL "im.ngmks.com"
#define MyAppExeName "ImageManager.exe"

[Setup]
; 注意: AppId 的值是唯一识别这个程序的标志。
; 不要在其他程序中使用相同的 AppId 值。
; (在编译器中点击菜单“工具 -> 产生 GUID”可以产生一个新的 GUID)
AppId={{C0B2618D-A2BC-4A8B-8600-5ED31A9A5583}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\ImageManager
DefaultGroupName={#MyAppName}
OutputBaseFilename=素材管理器安装程序
SetupIconFile=e:\code\C#\ImageManager\ImageManager\20150316140013_JkrVV.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Languages\ChineseSimp.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "E:\code\C#\ImageManager\ImageManager\bin\Release\ImageManager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\code\C#\ImageManager\ImageManager\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; 注意: 不要在任何共享的系统文件使用 "Flags: ignoreversion"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
//
//function IsDotNetDetected(version: string; service: cardinal): boolean;
//// Indicates whether the specified version and service pack of the .NET Framework is installed.
////
//// version -- Specify one of these strings for the required .NET Framework version:
////    'v1.1'          .NET Framework 1.1
////    'v2.0'          .NET Framework 2.0
////    'v3.0'          .NET Framework 3.0
////    'v3.5'          .NET Framework 3.5
////    'v4\Client'     .NET Framework 4.0 Client Profile
////    'v4\Full'       .NET Framework 4.0 Full Installation
////    'v4.5'          .NET Framework 4.5
////    'v4.5.1'        .NET Framework 4.5.1
////    'v4.5.2'        .NET Framework 4.5.2
////    'v4.6'          .NET Framework 4.6
////    'v4.6.1'        .NET Framework 4.6.1
////    'v4.6.2'        .NET Framework 4.6.2
////
//// service -- Specify any non-negative integer for the required service pack level:
////    0               No service packs required
////    1, 2, etc.      Service pack 1, 2, etc. required
//var
//    key, versionKey: string;
//    install, release, serviceCount, versionRelease: cardinal;
//    success: boolean;
//begin
//    versionKey := version;
//    versionRelease := 0;
//
//    // .NET 1.1 and 2.0 embed release number in version key
//    if version = 'v1.1' then begin
//        versionKey := 'v1.1.4322';
//    end else if version = 'v2.0' then begin
//        versionKey := 'v2.0.50727';
//    end
//
//    // .NET 4.5 and newer install as update to .NET 4.0 Full
//    else if Pos('v4.', version) = 1 then begin
//        versionKey := 'v4\Full';
//        case version of
//          'v4.5':   versionRelease := 378389;
//          'v4.5.1': versionRelease := 378675; // 378758 on Windows 8 and older
//          'v4.5.2': versionRelease := 379893;
//          'v4.6':   versionRelease := 393295; // 393297 on Windows 8.1 and older
//          'v4.6.1': versionRelease := 394254; // 394271 on Windows 8.1 and older
//          'v4.6.2': versionRelease := 394802; // 394806 on Windows 8.1 and older
//        end;
//    end;
//
//    // installation key group for all .NET versions
//    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + versionKey;
//
//    // .NET 3.0 uses value InstallSuccess in subkey Setup
//    if Pos('v3.0', version) = 1 then begin
//        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
//    end else begin
//        success := RegQueryDWordValue(HKLM, key, 'Install', install);
//    end;
//
//    // .NET 4.0 and newer use value Servicing instead of SP
//    if Pos('v4', version) = 1 then begin
//        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
//    end else begin
//        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
//    end;
//
//    // .NET 4.5 and newer use additional value Release
//    if versionRelease > 0 then begin
//        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
//        success := success and (release >= versionRelease);
//    end;
//
//    result := success and (install = 1) and (serviceCount >= service);
//end;
//
//function InitializeSetup(): Boolean;
//var IEPath, NetV2DownUrl:string;
//var ResultCode:Integer;
//begin
//    if not IsDotNetDetected('v4.6.1', 0) then begin
//
//      if MsgBox('系统缺少程序运行组件.Net Framework 4.6.1，是否立刻下载并安装？', mbConfirmation, MB_YESNO) = idYes then begin
////        if IsWin64 then begin
//          NetV2DownUrl := 'http://go.microsoft.com/fwlink/?LinkId=671728';
////        end else begin
////          NetV2DownUrl := 'https://download.microsoft.com/download/c/6/e/c6e88215-0178-4c6c-b5f3-158ff77b1f38/NetFx20SP2_x86.exe';
////        end;
//        IEPath := ExpandConstant('{pf}\Internet Explorer\iexplore.exe');
//        Exec(IEPath, NetV2DownUrl, '' , SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
//        MsgBox('下载安装好.Net Framework4.6.1组件后，重新运行本程序继续安装！',mbInformation,MB_OK);
//        result := false;
//      end else begin
//        MsgBox('没有安装.Net Framework4.6.1组件，无法运行程序，安装程序即将退出！',mbInformation,MB_OK);
//        result := false;
//      end;
//    end else begin
//        result := true;
//    end
//end;
