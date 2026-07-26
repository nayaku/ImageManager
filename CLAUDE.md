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
# 构建
dotnet build ImageManager.sln

# 运行
dotnet run --project ImageManager/ImageManager.csproj

# 发布（Release）
dotnet publish ImageManager/ImageManager.csproj -c Release
```

项目没有自动化测试，测试均为运行应用后手动验证。

## 架构

### Stylet MVVM

- **Bootstrapper.cs** — IoC 注册、启动时执行 EF Core 迁移、单实例 Mutex 检测
- **ViewModels/** — 全部业务逻辑；Stylet 按命名约定将 VM 与 View 绑定（`FooViewModel` ↔ `FooView`）
- **Views/** — 纯 XAML，几乎无代码隐藏；通过 `{Binding}` 绑定 VM 属性
- **Controls/** — 自定义控件与附加属性：`ScrollViewerMonitor`（滚动到底部时自动触发懒加载命令，支撑下方"滚动追加"）、`Canvas`
- **Windows/** — 独立浮窗（`ScreenShotWindow`），运行于主 Shell 之外
- **Logging/** — 轻量自定义日志：`Logger` / `LoggerFactory`

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
  `UserSettingData.Stickers` 仅是一份 `ObservableCollection<string>` 文件名登记表，不含状态数据。
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
- **SettingsBase.cs** — 所有 XML 配置类的抽象基类，详见下方"配置持久化"
- **UserSettingData.cs** — 用户偏好设置（`SettingsBase` 子类），序列化为 `UserSettings.xml`（XML，非 SQLite）；
  贴片相关成员为 `StickerFolderPath`（`SD\STMP`）、`RestoreStickerOnStartup` 开关，以及
  `ObservableCollection<string> Stickers`——只登记已打开贴片的图片文件名，状态数据存在各贴片自己的 xml 里
- **StickerStateData.cs** — 单张贴片的持久化状态（`SettingsBase` 子类，落盘为 `STMP\{GUID}.xml`），
  仅含 DPI 无关量：坐标、缩放因子、旋转、翻转、透明度、折叠裁剪起点与偏移；
  `FlipScaleX` / `FlipScaleY` / `EffectiveOpacity` 为供 XAML 直接绑定的计算属性
- **Migrations/** — EF Core 代码优先迁移；新建 Schema 变更需执行 `dotnet ef migrations add <Name>`。`MigrationCustom.cs` / `MigratorEx.cs` 为自定义迁移逻辑（标准 EF 迁移之外的数据/结构修补）

### 配置持久化

`Data/SettingsBase.cs` 是所有 XML 配置类的统一入口（`UserSettingData`、`StickerStateData` 均继承它）。新增配置类照此约定即可自动获得存盘能力：

- 继承 `SettingsBase : PropertyChangedBase`，重写 `abstract string FilePath`（须标 `[XmlIgnore]`）
- **自动防抖存盘**：`OnPropertyChanged` 触发 800ms 单次 `Timer`，期间的连续变更合并为一次写盘。
  由 PropertyChanged.Fody 织入，故普通自动属性赋值即会触发——无需手写存盘调用
- `Flush()` — 停掉定时器并立即写盘（如 `RootViewModel.OnClose` 中对 `UserSettingData`）
- `AutoSave = false` — 临时实例可关闭自动存盘
- `static Load<T>(filePath)` — 反序列化；文件不存在或解析失败返回 `null`（异常已记入日志），调用方须处理 null
- `Save()` 内部加锁并自动创建目录

### 关键工具类（`Tools/`）

| 文件 | 用途 |
|---|---|
| `HotKey.cs` | 全局 Win32 热键注册（截图：Ctrl+Shift+Alt+X） |
| `DisplayAPI.cs` | 多显示器检测与 DPI 处理 |
| `ImageUtility.cs` | 使用 FreeImage 加载/缩放图片 |
| `MouseTool.cs` | 全局鼠标钩子，用于贴纸拖拽 |
| `VirtualFileHelper.cs` | 通过 COM `IDataObject` 将拖入的虚拟文件落盘到临时目录 |
| `Helper/ClipboardHelper.cs` | 剪贴板图片粘贴支持 |
| `Helper/ControlsSearchHelper.cs` | 可视化树查找子控件（如定位 `ScrollViewer`） |
| `Extension/ContainerEx.cs` | StyletIoC `BuildUp` 扩展，注入后回调 `IInjectionAware.ParametersInjected` |
| `Converter/` | XAML 绑定用的 WPF `IValueConverter` 实现 |

### 图片加载与性能

- 图片懒加载；首批加载数量由 `UserSettingData.FirstLoadPictureNum` 控制（默认 50），滚动时按 `TakePictureNumOneTime`（默认 20）追加
- FreeImage.Standard 处理非标准格式（WebP、PSD 等）；标准格式使用 `System.Drawing` / WPF `BitmapImage`
- 导入时计算感知哈希（`Shipwreck.Phash`），用于相似图搜索

### 主题

HandyControls 主题在 `App.xaml` 中设置，支持运行时切换明/暗模式。主题资源为合并字典——禁止硬编码 `Colors` 或 `Brushes`，应使用 HandyControls 提供的命名资源键。

## 重要约定

- **单实例**：通过 `App.xaml.cs` 中的命名 `Mutex` 强制单实例；第二次启动会激活已有窗口
- **错误处理**：`App.xaml.cs` 中有全局 `UnhandledException` 和 `DispatcherUnhandledException` 处理器，自动上传错误日志
- **允许不安全代码**：用于 Win32 P/Invoke 和 FreeImage 互操作
- 中文（`zh-CN`）为默认语言；XAML 中所有 UI 字符串均为中文
- 安装包使用 **Inno Setup** 构建（拖放相关已知问题见 `design/development.md`）
- **注释风格**：XML 文档注释 `<summary>` 标签后须换行、内容独立成行；
  注释简洁有重点、不啰嗦，单行过长则折行。
