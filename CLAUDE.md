# CLAUDE.md

本文件为 Claude Code (claude.ai/code) 在本仓库工作时提供指导说明。

## 项目概述

**素材管理姬 (ImageManager)** — 一款面向画师的 WPF 桌面图片素材管理工具。核心功能：基于标签的分类管理、感知哈希去重、截图捕获、多格式图片支持。

- 框架：.NET 8.0-windows，WPF
- UI：HandyControls（Material Design）、XAML
- MVVM：Stylet 1.3.7（IoC 容器 + MVVM 框架）
- `INotifyPropertyChanged` 通过 **PropertyChanged.Fody**（编译期 IL 织入）实现，ViewModel 中无需手动调用 `OnPropertyChanged`

## 构建与运行

```powershell
# 构建（推荐直接构建项目；sln 见下方说明）
dotnet build ImageManager/ImageManager.csproj

# 构建整个解决方案
dotnet build ImageManager.sln

# 运行
dotnet run --project ImageManager/ImageManager.csproj

# 发布（Release）
dotnet publish ImageManager/ImageManager.csproj -c Release
```

> ⚠️ 命令行构建 `ImageManager.sln` 可能报 `NETSDK1022`（`Properties\DesignTimeResources.xaml` 被重复包含）：
> csproj 里对该文件有一条带 `Condition` 的显式 `<Page Include>`，在解决方案上下文下条件成立，与 SDK 默认通配重复。
> 遇到时改用 `dotnet build ImageManager/ImageManager.csproj`，或给 csproj 加 `EnableDefaultPageItems=false` 再显式声明。
> 构建前需先关闭正在运行的 `ImageManager.exe`，否则输出被占用（`MSB3027`）。

项目没有自动化测试，测试均为运行应用后手动验证。

## 架构

### Stylet MVVM

- **Bootstrapper.cs** — IoC 注册、启动时执行 EF Core 迁移、单实例 Mutex 检测
- **ViewModels/** — 全部业务逻辑；Stylet 按命名约定将 VM 与 View 绑定（`FooViewModel` ↔ `FooView`）
- **Views/** — 纯 XAML，几乎无代码隐藏；通过 `{Binding}` 绑定 VM 属性
- **Controls/** — 自定义控件与附加属性：`ScrollViewerMonitor`（滚动到底部时自动触发懒加载命令，支撑下方"滚动追加"）、`Canvas`、`CornerClip`（圆角裁剪：`Radius` 裁四角，`TopRadius` 只裁上面两角——`Border` 自身不会裁子元素）
- **Windows/** — 独立浮窗（`ScreenShotWindow`），运行于主 Shell 之外
- **Logging/** — 轻量自定义日志 `Logger` / `LoggerFactory`，以及崩溃上报 `Report`，详见下方"日志与崩溃上报"

页面导航使用 Stylet 的 `INavigationController` / `IConductor`，托管在 `RootViewModel` 中。

> 注：贴纸窗口已从早期的 `Windows/StickerWindow` 重构为标准 MVVM（`StickerViewModel : Screen` + `StickerView`），通过 Stylet 的 `WindowManager.ShowWindow(vm)` 弹出，旧实现已删除。详见下方"贴纸（Sticker）"。

### 贴纸（Sticker）

将图片以无边框置顶浮窗"贴"在桌面，供画师参考。入口：主页双击/右键打开图片，或向主窗口拖入图片。

- **StickerViewModel.cs** — 单张贴纸的全部逻辑：缩放、旋转、水平/垂直翻转、窗口透明度（均可用滚轮 + 修饰键调节：Ctrl=透明度、Shift=旋转、无=缩放）
- **视图变换全部下沉到绑定**：VM 不再持有 `DisplayWidth` / `FlipScaleX` / `EffectiveOpacity` 之类的派生属性，也不手算 DPI；
  `StickerView.xaml` 中 `Image` 用 `Stretch="None"`，翻转/缩放/旋转/透明度一律绑定 `StickerState` 上的计算属性
  （`FlipScaleX`、`FlipScaleY`、`EffectiveOpacity`），DPI 交由 WPF 处理
- 双击**折叠（Fold）**：将贴纸收缩为点击位置的局部小图，再次双击展开。
  裁剪用 WPF `CroppedBitmap` 直接切源位图，边长为 `64 / ZoomRate` 源像素——
  即折叠后屏幕尺寸恒为 64×64，与当前缩放无关。折叠时按旋转角算包围盒，反推 `FoldOffsetX/Y` 让点击点落在新窗口中心
- **初始缩放**：图库/剪贴板来源的贴片在 `DispatcherPriority.Loaded` 后测量 `ActualWidth/Height`，
  若超出所在屏幕工作区（换算为 DIP 后）的 90%，则按较小比例把 `ZoomRate` 压到屏幕内（再乘 0.9 留边）；
  截图（已指定 `initPoint`）与启动还原不参与，由 `_canZoomInInitially` 控制
- `Instances` 静态集合保存所有已创建贴片的缩略图，供集中管理/聚焦
- **落盘**：创建贴片时图片即写入 `SD\STMP`（图库来源用 `File.Copy` 保留原格式，截图/剪贴板编码为 PNG，文件名为 GUID）。`AddToDatabase()` 直接复用该 STMP 文件回存图库，不再写临时文件
- **状态持久化**：每张贴片一个独立的 `STMP\{GUID}.xml`，由 `StickerStateData`（继承 `SettingsBase`）自行防抖存盘——
  任意属性变更即触发，无轮询、无状态比对。窗口坐标由 XAML 双向绑定（`Left/Top` `Mode=TwoWay`）直接写入 state。
  `AppStateData.Stickers` 仅是一份 `ObservableCollection<string>` 文件名登记表，不含状态数据。
  状态只保存 DPI 无关量（缩放因子、源像素裁剪、DIP 坐标）
- **启动还原**：勾选主界面「启动时还原贴片」（`RestoreStickerOnStartup`）后，`RootViewModel.RestoreOrClearStickers()`
  遍历登记表逐个 `SettingsBase.Load<StickerStateData>` 重建贴片，剔除图片或 xml 缺失的残留登记，并删除未被引用的 STMP 孤儿文件；
  未勾选则清空登记表与 STMP 全部文件。关闭单张贴片只移出登记表，其 STMP 文件留待下次启动作为孤儿清理
- 四种构造：`StickerViewModel(string imagePath)`（来自图库文件，`IsFromDatabase=true`，允许初始缩放）/ `StickerViewModel(Bitmap)`（剪贴板，允许初始缩放）/ `StickerViewModel(Bitmap, Point)`（截图，落点为 `initPoint`，不做初始缩放）/ `StickerViewModel(StickerStateData)`（启动还原，从 STMP 已有文件加载、不复制）

### 拖拽导入

主窗口（`RootViewModel.DragOver` / `Drop`）支持三类拖入：

- `FileDrop` — 本地文件路径
- `Bitmap` — 内存位图（如从其他程序拖出的图像）
- `FileContents` — **虚拟文件**（浏览器、压缩包等直接拖出、磁盘上尚不存在的文件），由 `Tools/VirtualFileHelper.cs` 通过 COM `IDataObject`（FileGroupDescriptorW / FileContents）读取并落盘到临时目录后导入

### 数据层

- **ImageContext.cs** — EF Core `DbContext`，使用 SQLite（数据库文件 `Image.db` 位于可执行文件旁）；通过 EF 代理实现懒加载
- **Data/Model/** — `Picture`（图片记录，含 MD5 和感知哈希）、`Label`（标签），多对多关系
- **PictureFactory.cs** — 图片导入处理：格式识别、MD5 哈希、感知哈希（Shipwreck.Phash）、缩略图生成
- **PictureDataArchive.cs** — 自定义图库归档格式 `.pdaf`（Magic `PDAF`）：`Save` / `Load` 实现整库的导入/导出，并通过 `ProgressChanged` 事件汇报进度（对应 `ExportImageProgressViewModelWrap` 等 VM）
- **Migrations/** — EF Core 代码优先迁移；新建 Schema 变更需执行 `dotnet ef migrations add <Name>`。`MigrationCustom.cs` / `MigratorEx.cs` 为自定义迁移逻辑（标准 EF 迁移之外的数据/结构修补）

### 配置持久化

配置类全部位于 `Settings/`（命名空间 `ImageManager.Settings`），均为 XML 文件（非 SQLite），按用途拆分：

| 类 | 文件 | 内容 |
|---|---|---|
| `UserSettingsData` | `UserSettings.xml` | 用户可调设置：存储路径（`StorePath` 及派生的 `ImageFolderPath` / `TempFolderPath` / `StickerFolderPath`）、`RestoreStickerOnStartup`、`ClearUnUsedLabel`、缩略图宽度、相似度阈值、`IsHideWhenScreenShoot`；用 `[SettingsItem]` / `[HideInSettings]`（`SettingsAttribute.cs`）标注展示名与说明，数值项另用 `[Range]` 标注取值范围；构造时自动创建 IMG/TMP/STMP 目录 |
| `ViewSettingsData` | `ViewSettings.xml` | 界面状态：主题模式（`ThemeMode`：明 / 暗 / 跟随系统，默认跟随系统）、强调色序号（`AccentIndex`）、卡片宽度、排序（`OrderByEnum`）、升降序、分组 |
| `ConfigurationData` | `Configuration.xml` | 内部参数：`FirstLoadPictureNum`、`TakePictureNumOneTime` |
| `AppStateData` | `AppState.xml` | 程序运行状态：`WaitToDeleteFiles`、`Stickers`（贴片文件名登记表） |
| `StickerStateData` | `STMP\{GUID}.xml` | 单张贴片状态，详见"贴纸（Sticker）" |

前四个类各有 `static Default` 单例，并在 `Bootstrapper` 中以 `ToInstance` 注册进 IoC，VM 通过 `[Inject]` 获取。

`SettingsBase.cs` 是所有配置类的基类。新增配置类照此约定即可自动获得存盘能力：

- 继承 `SettingsBase : PropertyChangedBase`，重写 `abstract string FilePath`（只读属性不会被序列化；若可写则须标 `[XmlIgnore]`），并提供 `static Default => Load<T>(...) ?? new T()` 单例
- **自动防抖存盘**：`OnPropertyChanged` 触发 800ms 单次 `Timer`，期间的连续变更合并为一次写盘。
  由 PropertyChanged.Fody 织入，故普通自动属性赋值即会触发——无需手写存盘调用
- `Flush()` — 停掉定时器并立即写盘（如 `RootViewModel.OnClose` 中对全部配置）
- `AutoSave = false` — 临时实例可关闭自动存盘
- `static Load<T>(filePath)` — 反序列化；文件不存在或解析失败返回 `null`（异常已记入日志），调用方须处理 null
- `Save()` 内部加锁并自动创建目录

### 日志与崩溃上报

- **目录**：`Tools/AppPaths.cs` 统一定义，根目录为 `%LocalAppData%\ImageManager`（安装目录可能不可写），
  日志在 `Logs\`，崩溃包在 `Crashes\`
- **日志**：`Logger.Log` 写 Info 及以上等级，文件名为 `{PID}_{yyyy-MM-dd}.log`（按进程、按天），
  多线程写入由静态锁保护；`Logger.GetLogPath(date)` 供打包时定位日志文件
- **崩溃上报**（仅 Release）：`Log(..., report: true)` 触发 `Report.ReportCrash`，
  打包 `crash.txt`（版本、OS、运行时、内存、异常）和当前进程最近 3 天的日志为 zip，
  以 multipart 上传至 `ReportUrl`，zip 无论成败均保留在 `Crashes\`；同一崩溃 10 分钟内只报一次
- **同步/异步**：默认在线程池后台上报，不阻塞 UI；进程即将退出的场景（启动失败、
  `AppDomain.UnhandledException`）须传 `waitReport: true` 同步等待，否则后台任务会被杀掉
- `Report` 内部不抛异常，所有 Growl 通知经 `Notify` 切回 UI 线程
- **机器信息**：`Tools/Machine.cs` 通过 WMI（`System.Management`）获取 CPU/主板/系统盘序列号与 OS 名称；
  `GetMachineCode()` 为三者拼接后的 MD5，随崩溃包上报。启动时会把这些信息写入日志
- 版本号统一用 `App.AppVersion`

### 机器心跳与在线人数

- `Tools/ServerApi.cs` 集中服务端访问，`BaseUrl` 为唯一的服务端地址（`Report` 的上报地址也由它拼接）
- **心跳**：`RootViewModel.Loaded` 调用 `ServerApi.StartHeartbeat()`，启动即 POST `machines/`，
  之后每 20 分钟一次（须小于服务端 1 小时会话窗口）；Debug/Release 均运行，失败只记 Warn 日志、不弹通知
- **在线人数**：`GetOnlineCountAsync()` 请求 `machines/current`，失败返回 `null`；仅在「关于」窗口显示
- `Machine.GetMachineCode()` 结果已用 `Lazy` 缓存

### 关键工具类（`Tools/`）

| 文件 | 用途 |
|---|---|
| `AppPaths.cs` | 用户数据目录（日志、崩溃包）路径常量 |
| `Machine.cs` | WMI 读取硬件序列号、OS 名称，生成机器码 |
| `ServerApi.cs` | 服务端地址、机器心跳、在线人数查询 |
| `HotKey.cs` | 全局 Win32 热键注册（截图：Ctrl+Shift+Alt+X） |
| `DisplayAPI.cs` | 多显示器检测与 DPI 处理 |
| `ImageUtility.cs` | 使用 FreeImage 加载/缩放图片 |
| `MouseTool.cs` | 全局鼠标钩子，用于贴纸拖拽 |
| `VirtualFileHelper.cs` | 通过 COM `IDataObject` 将拖入的虚拟文件落盘到临时目录 |
| `Helper/ClipboardHelper.cs` | 剪贴板图片粘贴支持 |
| `Helper/ImageClipboardHelper.cs` | 复制图片到剪贴板（仅写 PNG，保留透明）；`GetClipboardOpener()` 查询剪贴板占用进程，复制失败（`ExternalException`）时先记日志再弹窗 |
| `Helper/ControlsSearchHelper.cs` | 可视化树查找子控件（如定位 `ScrollViewer`） |
| `Extension/ContainerEx.cs` | StyletIoC `BuildUp` 扩展，注入后回调 `IInjectionAware.ParametersInjected` |
| `Converter/` | XAML 绑定用的 WPF `IValueConverter` 实现 |

### 图片加载与性能

- 图片懒加载；首批加载数量由 `ConfigurationData.FirstLoadPictureNum` 控制（默认 50），滚动时按 `TakePictureNumOneTime`（默认 20）追加
- FreeImage.Standard 处理非标准格式（WebP、PSD 等）；标准格式使用 `System.Drawing` / WPF `BitmapImage`
- 导入时计算感知哈希（`Shipwreck.Phash`），用于相似图搜索

### 主题与设计令牌

HandyControls 负责明暗切换，程序自己的配色令牌在 `Themes/` 维护。**禁止硬编码 `Color` / `Brush`**，一律引用资源键（`DynamicResource` 或 `StaticResource`）。

- **`Themes/AppTheme.cs`** — `ThemeMode` 枚举（`Light` / `Dark` / `System`）与唯一入口 `Apply(mode, accentIndex)`
  - 明 / 暗：`ThemeManager.Current.UsingSystemTheme = false` 后设 `ApplicationTheme`
  - 跟随系统：`UsingSystemTheme = true`，并监听 `SystemEvents.UserPreferenceChanged` 重新同步令牌
  - 强调色：6 种预设（`Tokens.Common.xaml` 的 `AppAccent0..5`），写入 `ThemeManager.Current.AccentColor`
  - **跟随系统时不下发自定义强调色**——HandyControl 会改用系统强调色；此时主题面板展示 `GetAccentColorFromSystem()` 的系统色，6 个预设色块禁用（`RootViewModel.IsAccentSelectable`）
- **`Themes/AppPalette.cs`** — 程序自有令牌：`AppPageBrush`、`AppCardBrush`、`AppSurfaceBrush`、`AppStrokeBrush`、`AppImageBackdropBrush`
  - 切换主题时**替换 `Application.Resources` 中同名的资源值**（新画刷实例）
  - ⚠️ 不要复用画刷实例改 `Color`：`ResourceDictionary` 会把放进去的 `Freezable` 冻结
- **`Themes/Tokens.Common.xaml`** — 常量与通用样式：圆角（`AppCardCorner` 10 / `AppPanelCorner` 12）、三档阴影（`AppCardShadow` / `AppHoverShadow` / `AppPopupShadow`）、强调色 `AppAccent0..5`、`AppCircleCheck`（圆形勾选框）、`AppRoundIconButton`（卡片悬浮圆钮）、`AppSegmentButton`、`AppAccentDot`（强调色色块）、`AppTagPill`、`AppCountPill`、「不添加」提示的三个画刷（`AppRejectChip*`）
- **`Themes/Overrides.xaml`** — 全局 `ToolTip` 样式、`AppCardWidthSlider`（状态栏卡片宽度滑块）、`AppThinProgress`（6px 细进度条）
- 合并顺序：`App.xaml` 在 `hc:ThemeResources` / `hc:Theme` 之后依次合并 `Tokens.Common.xaml`、`Overrides.xaml`；`App.OnStartup` 先调 `AppPalette.Register()`，`RootViewModel.ParametersInjected()` 再按已保存设置 `AppTheme.Apply(...)`
- 主题面板：`RootView.xaml` 右上角齿轮 → 明 / 暗 / 跟随系统 + 6 色强调色 + 恢复默认（`RootViewModel.ChangeTheme` / `ChangeAccent` / `ResetAppearance`）

### 图库界面（MainPageView）

主图库用 `hc:WaterfallPanel` 瀑布流（`DesiredLength` 绑 `CardWidth`，滚动懒加载依赖它，**不要改成等高网格**）。卡片样式为 `PictureCard`，状态语言如下：

| 状态 | 表现 |
|---|---|
| 默认 / 悬停 | 白卡 + 1px 描边 + 轻阴影；悬停上浮 2px 并加深阴影 |
| 选中 | **整卡** 2px 强调色描边（`DataTrigger` 改 `BorderThickness` / `BorderBrush`），不再有 ✓ 角标 |
| 添加模式·接受 | 左上角 `AppCircleCheck`：强调色填充 + 白勾 |
| 添加模式·未接受 | 圆形只剩灰色描边；图片与信息区降到 55% 不透明度，中央浮出「不添加」大字（深色底片，无图标） |
| 冲突（重复 / 相似） | 右上角红色文字角标（`ConflictText`），圆形勾禁用（灰、不描红），**不变暗** |
| 悬停快捷操作 | 右下角两个圆钮：贴片式打开 / 用外部程序打开；**添加图片界面不显示** |

- 图片顶角跟随卡片圆角、底部两角为直角：`controls:CornerClip.TopRadius="10"`
- 空图库：矢量插画 + 添加图片/文件夹按钮（由 `HasPictures` 取反控制）；首批加载先渲染骨架屏（`IsLoading`，`RefreshPicture` 用 `Dispatcher.BeginInvoke` 让骨架先出一帧）
- 状态栏：`● N 张图片` +「已选 N」胶囊 + 卡片宽度滑块（120 ~ `MaxCardWidth`）
- 卡片右键菜单为**原生声明式 `MenuItem`**（与画布空白处菜单同一套样式，图标用主题画刷），普通 / 添加模式靠 `IsAddPictureMode` 切换可见性；批量操作用 `AcceptAdd()` / `RejectAdd()` 薄包装，避免 `CommandParameter="True"` 字符串直接传给 `bool` 参数。早期的 `MenuItemViewModel` + `ContextMenuItems` 数据驱动方案已删除，新增菜单项直接写 XAML
- `PictureSelectableItemWrapper` 提供派生状态 `IsRejected`、`ContentOpacity`、`ConflictText`；`AcceptToAdd` 的 setter 里手动 `NotifyOfPropertyChange` 通知它们（依赖 `Item.*`，Fody 侦测不到）

### 检查更新（UpdateView / UpdateViewModel）

「帮助 → 检测更新」打开，`LoadedAsync` 走完整流程：检查 → 拉取更新日志 → 自动下载 → 启动安装程序并退出。

- 状态机 `State`：`Checking` / `IsLatest` / `Updating` / `Error`；文案拆三层：`StateText`（标题）、`StateSubText`（一句说明）、`ErrorDetail`（异常原文，放淡红卡片）
- **更新日志**解析成 `ReleaseNote`（版本号 / 日期 / 是否当前版本 / 是否待升级的新版本 / 要点列表），用 `ItemsControl` 渲染（不用 Markdown）：
  - `IsLatest` 取**全部历史版本（不限条数）**，标题显示「更新历史」
  - `Updating` 只取到当前版本为止，标题显示「更新内容」
- 下载进度：`ReceivedBytes` / `TotalBytes` / `BytesPerSecond` → `DownloadProgressPercentText` 与 `DownloadProgressText`（已下载 / 总量 · 剩余时间）
- ⚠️ `NeedUpdateAsync` 失败后不能再把状态覆盖成「已是最新版本」，否则网络异常会伪装成"已最新"

## 重要约定

- **单实例**：通过 `App.xaml.cs` 中的命名 `Mutex` 强制单实例；第二次启动会激活已有窗口
- **错误处理**：`App.xaml.cs` 中有全局 `UnhandledException`（同步上报后退出）和
  `DispatcherUnhandledException`（`Handled = true`，后台上报、程序继续运行）处理器，仅 Release 注册
- **允许不安全代码**：用于 Win32 P/Invoke 和 FreeImage 互操作
- 中文（`zh-CN`）为默认语言；XAML 中所有 UI 字符串均为中文。按钮文案不用空格撑宽（写 `确定`，不写 `确    定`），宽度用 `Width` 控制
- 安装包使用 **Inno Setup** 构建（拖放相关已知问题见 `design/development.md`）
- **注释风格**：XML 文档注释 `<summary>` 标签后须换行、内容独立成行；
  注释简洁有重点、不啰嗦，单行过长则折行。

## 界面开发注意事项（踩过的坑）

- **`Border` 不裁子元素**：`CornerRadius` 只影响自身描边/背景，图片依旧直角溢出 → 用 `Controls/CornerClip`
- **`ResourceDictionary` 会冻结 `Freezable`**：放进去的画刷不能再改 `Color`（抛"对象处于只读状态"）→ 切主题时替换资源值（`AppPalette.Apply`）
- **主题令牌要按"目标模式"判定**：刚设置 `ApplicationTheme` 时 `ActualApplicationTheme` 可能还是旧值，读它会导致令牌不切换
- **`DataTemplate.Triggers` 里用 `RelativeSource AncestorType` 不可靠**（卡片悬停浮层曾因此完全不出现）→ 触发器写进元素自己的 `Style`
- **`MenuItem.Icon` 内不要用 `RelativeSource AncestorType=MenuItem` 取 `Foreground`**：取不到，图标会因 `Stroke=null` 而不显示 → 直接引用主题画刷；危险色由外层 `Foreground` + 内层 `TemplateBinding` 传递
- **HandyControl 的 `ProgressBar` 自带百分比文字叠加**：需要纯净进度条时套自定义模板（`AppThinProgress`）
- **MdXaml 不适合结构化日志**：`MarkdownScrollViewer` 会把 `##` 与列表渲染得松散、样式难控 → 版本历史用 `ItemsControl` + `ReleaseNote` 渲染（包仍在引用，仅更新窗口不再使用）
- **Stylet `s:Action` 的 `CommandParameter` 是字符串**：传给 `int` / `bool` 形参会抛 `ArgumentException` → 用 `string` 形参自行解析，或加无参薄包装（如 `AcceptAdd` / `RejectAdd`）
- **Fody 只通知同类属性**：依赖 `Item.xxx` 的派生属性（`IsRejected`、`ContentOpacity`）Fody 侦测不到，需在 setter 里手动 `NotifyOfPropertyChange`
- **重复导入同一张图会命中 MD5 去重**：卡片显示「重复」角标且圆形勾禁用，属预期行为，不是 UI bug
- 设计稿在 `design/mockups/`（HTML + 渲染 PNG，不参与编译），验收截图在 `design/screens/`
