using FreeImageAPI;
using HandyControl.Tools.Extension;
using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Logging;
using ImageManager.Settings;
using ImageManager.Tools.Helper;
using Microsoft.EntityFrameworkCore;
using StyletIoC;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static ImageManager.Settings.ViewSettingsData;
using Label = ImageManager.Data.Model.Label;
using Path = System.IO.Path;

namespace ImageManager.ViewModels
{
    public class MainPageViewModel : PropertyChangedBase, IInjectionAware
    {
        private readonly object _fatherViewModel;
        private ImageContext _context;
        private int _skipNum = 0;
        private readonly Logger _logger = LoggerFactory.GetLogger(nameof(MainPageViewModel));

        [Inject]
        public IWindowManager WindowManager;
        [Inject]
        public IContainer Container;
        [Inject]
        public ViewSettingsData ViewSettings { get; set; }
        [Inject]
        public ConfigurationData Configuration { get; set; }
        [Inject]
        public AppStateData AppState { get; set; }


        public bool IsAddPictureMode => _fatherViewModel is AddImageViewModel;
        public int PictureNum { get; set; }
        public int SelectPictureNum { get; set; }
        public string Message
        {
            get
            {
                var message = string.Format("{0:N0}张图片", PictureNum);
                if (SelectPictureNum != 0)
                    message += string.Format("，选中{0:N0}张", SelectPictureNum);
                return message;
            }
        }
        public BindableCollection<PictureSelectableItemWrapper> Pictures { get; set; }

        /// <summary>
        /// 是否正在加载图片，用于骨架屏。
        /// </summary>
        public bool IsLoading { get; set; }
        /// <summary>
        /// 图库里是否已有图片，用于空态。
        /// </summary>
        public bool HasPictures => Pictures?.Count > 0;
        /// <summary>
        /// 是否有选中项。
        /// </summary>
        public bool HasSelection => SelectPictureNum > 0;

        public bool IsDesc { get => ViewSettings.IsDesc; set => ViewSettings.IsDesc = value; }
        public OrderByEnum OrderBy { get => ViewSettings.OrderBy; set => ViewSettings.OrderBy = value; }
        public bool IsOrderByAddTime => OrderBy == OrderByEnum.AddTime;
        public bool IsOrderByTitle => OrderBy == OrderByEnum.Title;
        public bool IsOrderByAddingState => OrderBy == OrderByEnum.AddState;

        public bool IsGroup { get => ViewSettings.IsGroup; set => ViewSettings.IsGroup = value; }
        public List<IGrouping<string, PictureSelectableItemWrapper>>? PictureGroups
        {
            get
            {
                if (IsGroup)
                {
                    return OrderBy switch
                    {
                        OrderByEnum.AddTime => Pictures.GroupBy(p => p.Item.AddTime.ToString("yyyy-MM")).ToList(),
                        OrderByEnum.Title => Pictures.GroupBy(p => !string.IsNullOrEmpty(p.Item.Title) ? StringInfo.GetNextTextElement(p.Item.Title) : string.Empty).ToList(),
                        OrderByEnum.AddState => Pictures.GroupBy(p => p.Item.AddState switch
                        {
                            PictureAddStateEnum.WaitToAdd => "待添加",
                            PictureAddStateEnum.SameConflict => "相同冲突",
                            PictureAddStateEnum.SimilarConflict => "相似冲突",
                            _ => "未知状态"
                        }).ToList(),
                        _ => null,
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        public double CardWidth
        {
            get => ViewSettings.CardWidth;
            set { ViewSettings.CardWidth = value; }
        }
        public double MaxCardWidth { get; set; }

        public BindableCollection<Label> FilterLabels { get; set; } = new();
        public bool ShowFilterLabelPanel { get; set; }

        public MainPageViewModel(object fatherViewModel, ImageContext context)
        {
            _fatherViewModel = fatherViewModel;

            // 筛选标签更变
            FilterLabels.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) =>
            {
                ShowFilterLabelPanel = FilterLabels.Count > 0;
                RefreshPicture();
            };
            _context = context;
        }

        public void UpdatePicture()
        {
            var takeNum = Configuration.TakePictureNumOneTime;
            if (_skipNum == 0)
                takeNum = Configuration.FirstLoadPictureNum;
            IQueryable<Picture> query;
            if (_fatherViewModel is AddImageViewModel addImageViewModel)
                query = addImageViewModel.Pictures.AsQueryable();
            else
                query = _context.Pictures.AsQueryable();

            if (_fatherViewModel is RootViewModel rootViewModel && rootViewModel.SearchText != null)
                query = query.Where(p => EF.Functions.Like(p.Title, "%" + rootViewModel.SearchText + "%"));
            if (FilterLabels.Count != 0)
                query = query.Where(p => p.Labels.Count(l => FilterLabels.Contains(l)) == FilterLabels.Count);
            var orderBy = Enum.GetName(OrderBy)!;
            if (IsDesc)
                orderBy += " desc";
            query = query.OrderBy(orderBy);
            // 获取总数
            // 第一次加载
            if (_skipNum == 0)
                PictureNum = query.Count();
            // 分页
            query = query.Skip(_skipNum).Take(takeNum);
            var resPictures = query.Select(p => new PictureSelectableItemWrapper(p, IsAddPictureMode));
            // 第一次加载
            if (_skipNum == 0)
            {
                Pictures = new BindableCollection<PictureSelectableItemWrapper>(resPictures);
            }
            else if (resPictures.Any())
            {
                Pictures.AddRange(resPictures);
            }

            if (resPictures.Any())
            {
                _skipNum += takeNum;
                NotifyOfPropertyChange(nameof(Pictures));
                NotifyOfPropertyChange(nameof(PictureGroups));
            }
        }

        public void RefreshPicture()
        {
            _skipNum = 0;
            var dispatcher = Application.Current?.Dispatcher;
            // 空库刷新时先让骨架屏渲染一帧，再执行查询
            if (dispatcher == null || HasPictures)
            {
                UpdatePicture();
                return;
            }
            IsLoading = true;
            dispatcher.BeginInvoke(
                new Action(() =>
                {
                    try
                    {
                        UpdatePicture();
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }),
                DispatcherPriority.Background);
        }

        public void FetchMorePicture()
        {
            UpdatePicture();
        }

        public void PictureLabelClick(Label label)
        {
            if (!FilterLabels.Contains(label))
                FilterLabels.Add(label);
        }
        public void DeletePictureLabel(object sender, RoutedEventArgs e)
        {
            var label = (Label)((FrameworkElement)e.OriginalSource).DataContext;
            var parent = (FrameworkElement)VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource);
            var picture = (PictureSelectableItemWrapper)(parent).DataContext;

            picture.Item.Labels.Remove(label);
            if (!IsAddPictureMode)
                _context.SaveChanges();
        }

        public void PictureSelectionChange()
        {
            var selectNum = Pictures.Count(p => p.IsSelected);
            SelectPictureNum = selectNum;
        }

        public void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                // 最小两栏
                MaxCardWidth = (e.NewSize.Width) / 2 - 5;
                CardWidth = Math.Min(CardWidth, MaxCardWidth);
            }
        }

        #region 右键菜单栏
        public void StickPicture(object sender, MouseButtonEventArgs e)
        {
            if (IsAddPictureMode)
                return;
            var picture = ((PictureSelectableItemWrapper)((Image)sender).DataContext).Item;
            var picturePath = Path.Join(picture.ImageFolderPath, picture.Path);
            //var sticker = new StickerWindow(Path.Join(picture.ImageFolderPath, picture.Path));
            //sticker.Show();
            var stickerViewModel = new StickerViewModel(picturePath);
            WindowManager.ShowWindow(stickerViewModel);
        }
        public void StickPictureCommand()
        {
            SelectedPictures.ForEach(picture =>
            {
                var path = Path.Join(picture.ImageFolderPath, picture.Path);
                var stickerViewModel = new StickerViewModel(path);
                WindowManager.ShowWindow(stickerViewModel);
            });
        }

        /// <summary>
        /// 卡片悬停浮层：把单张图片贴到桌面。
        /// </summary>
        public void StickPictureOf(PictureSelectableItemWrapper? wrapper)
        {
            if (wrapper?.Item == null)
                return;
            var picture = wrapper.Item;
            var picturePath = Path.Join(picture.ImageFolderPath, picture.Path);
            WindowManager.ShowWindow(new StickerViewModel(picturePath));
        }

        /// <summary>
        /// 空态：添加图片，仅在主页可用。
        /// </summary>
        public void EmptyAddPictures()
        {
            if (_fatherViewModel is RootViewModel root)
                root.AddPictures();
        }

        /// <summary>
        /// 空态：添加文件夹，仅在主页可用。
        /// </summary>
        public void EmptyAddFolders()
        {
            if (_fatherViewModel is RootViewModel root)
                root.AddFolders();
        }
        private IEnumerable<Picture> SelectedPictures => Pictures.Where(x => x.IsSelected).Select(x => x.Item);
        public void OpenPictureWithExternalProgram()
        {
            SelectedPictures.ForEach(picture =>
            {
                var path = Path.Join(picture.ImageFolderPath, picture.Path);
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo(path)
                    {
                        UseShellExecute = true
                    }
                };
                p.Start();
            });

        }

        /// <summary>
        /// 卡片悬停浮层：用外部程序打开单张图片。
        /// </summary>
        public void OpenPictureOf(PictureSelectableItemWrapper? wrapper)
        {
            if (wrapper?.Item == null)
                return;
            var path = Path.Join(wrapper.Item.ImageFolderPath, wrapper.Item.Path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        public void CopyPicture()
        {
            var pictures = SelectedPictures.ToList();
            if (pictures.Count == 0) return;

            try
            {
                if (pictures.Count == 1)
                {
                    // 单选：复制真实位图（PNG + CF_BITMAP），可直接粘入 PS / 聊天框
                    var path = Path.Join(pictures[0].ImageFolderPath, pictures[0].Path);
                    using var fib = FreeImageBitmap.FromFile(path);
                    using var bitmap = fib.ToBitmap();
                    ImageClipboardHelper.SetImage(bitmap);
                }
                else
                {
                    // 多选：以文件拖放格式复制路径列表，粘到文件夹 / 其他程序直接出文件
                    var paths = pictures
                        .Select(p => Path.Join(p.ImageFolderPath, p.Path))
                        .ToArray();
                    var dataObject = new DataObject(DataFormats.FileDrop, paths);
                    Clipboard.SetDataObject(dataObject, true);
                }
            }
            catch (ExternalException ex)
            {
                // 先记录占用者再弹窗：弹窗期间占用者可能已释放
                _logger.Error($"{ex.Message}，剪贴板占用者：{ImageClipboardHelper.GetClipboardOpener()}");
                MessageBox.Show($"复制图片到剪贴板失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void CopyPicturePath()
        {
            var paths = new List<string>();
            SelectedPictures.ForEach(picture =>
            {
                var path = Path.Join(picture.ImageFolderPath, picture.Path);
                paths.Add(path);
            });
            var dataObject = new DataObject(DataFormats.Text, string.Join('\n', paths));
            try
            {
                Clipboard.SetDataObject(dataObject, true);
            }
            catch (ExternalException ex)
            {
                _logger.Error($"{ex.Message}，剪贴板占用者：{ImageClipboardHelper.GetClipboardOpener()}");
                MessageBox.Show($"复制图片路径到剪贴板失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void AddPictureLabel()
        {
            var pictureAddLabelViewModel = new PictureAddLabelViewModel();
            Container.BuildUp(pictureAddLabelViewModel);
            bool? res = WindowManager.ShowDialog(pictureAddLabelViewModel);
            if (res ?? false)
            {
                var label = _context.Labels.SingleOrDefault(l => l.Name == pictureAddLabelViewModel.SearchText)
                    ?? new Label { Name = pictureAddLabelViewModel.SearchText };

                SelectedPictures.ForEach(p =>
                {
                    if (!p.Labels.Contains(label))
                        p.Labels.Add(label);
                });
                Pictures.Refresh();
                NotifyOfPropertyChange(nameof(PictureGroups));
                if (!IsAddPictureMode)
                    _context.SaveChanges();
            }
        }
        public void DeletePicture()
        {
            var res = DialogViewModel.Show(Container, "删除图片",
                "确定删除选中的图片吗？", "删  除", "取  消", confirmButtonStyle: "ButtonDanger");
            if (res ?? false)
            {
                var deletePictures = Pictures.Where(p => p.IsSelected).ToList();
                _context.Pictures.RemoveRange(deletePictures.Select(p => p.Item));
                _context.SaveChanges();
                Pictures.RemoveRange(deletePictures);
                var deleteFiles = new List<string>();
                deletePictures.ForEach(p =>
                {
                    deleteFiles.Add(Path.Join(p.Item.ImageFolderPath, p.Item.Path));
                    if (p.Item.ThumbnailPath != null)
                        deleteFiles.Add(Path.Join(p.Item.ImageFolderPath, p.Item.ThumbnailPath));
                });
                AppState.WaitToDeleteFiles = deleteFiles;
            }
        }

        public void UpdateOrderBy(string orderByString)
        {
            var orderBy = (OrderByEnum)Enum.Parse(typeof(OrderByEnum), orderByString);
            if (orderBy != OrderBy)
            {
                OrderBy = orderBy;
                RefreshPicture();
            }
        }

        public void UpdateIsDesc(string isDescString)
        {
            var isDesc = Boolean.Parse(isDescString);
            if (IsDesc != isDesc)
            {
                IsDesc = isDesc;
                RefreshPicture();
            }
        }
        public void SetGroup(string isGroupString)
        {
            var isGroup = Boolean.Parse(isGroupString);
            IsGroup = isGroup;
            RefreshPicture();
        }

        public void AcceptToAdd(bool accept)
        {
            Pictures.Where(x => x.IsSelected).ForEach(p =>
            {
                if (p.IsEnabled)
                    p.AcceptToAdd = accept;
            });
        }

        /// <summary>
        /// 右键菜单：接受添加。
        /// </summary>
        public void AcceptAdd() => AcceptToAdd(true);

        /// <summary>
        /// 右键菜单：拒绝添加。
        /// </summary>
        public void RejectAdd() => AcceptToAdd(false);
        #endregion

        #region 来自上一层调用
        public void SelectAll()
        {
            Pictures.ForEach(p => p.IsSelected = true);
        }
        public void SelectNone()
        {
            Pictures.ForEach(p => p.IsSelected = false);
        }
        public void SelectInvert()
        {
            Pictures.ForEach(p => p.IsSelected = !p.IsSelected);
        }
        #endregion

        public void ParametersInjected()
        {
            RefreshPicture();
        }
    }
}
