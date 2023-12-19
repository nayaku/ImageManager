using HandyControl.Tools.Command;
using HandyControl.Tools.Extension;
using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Tools.Helper;
using ImageManager.Windows;
using Microsoft.EntityFrameworkCore;
using StyletIoC;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static ImageManager.Data.UserSettingData;
using Label = ImageManager.Data.Model.Label;
using Path = System.IO.Path;

namespace ImageManager.ViewModels
{
    public class MainPageViewModel : PropertyChangedBase, IInjectionAware
    {
        private readonly object _fatherViewModel;
        private ImageContext _context;
        private int _skipNum = 0;
        private bool _canFetchMore = false;
        private bool _needRefresh = false;
        private DateTime _lastFetchTime = DateTime.Now;

        [Inject]
        public IWindowManager WindowManager;
        [Inject]
        public IContainer Container;
        [Inject]
        public UserSettingData UserSetting { get; set; }


        public bool IsAddPictureMode => _fatherViewModel is AddImageViewModel;
        public string Message { get; set; }
        public BindableCollection<PictureSelectableItemWrapper> Pictures { get; set; }

        public bool IsDesc { get => UserSetting.IsDesc; set => UserSetting.IsDesc = value; }
        public OrderByEnum OrderBy { get => UserSetting.OrderBy; set => UserSetting.OrderBy = value; }
        public bool IsOrderByAddTime => OrderBy == OrderByEnum.AddTime;
        public bool IsOrderByTitle => OrderBy == OrderByEnum.Title;
        public bool IsOrderByAddingState => OrderBy == OrderByEnum.AddState;

        public bool IsGroup { get => UserSetting.IsGroup; set => UserSetting.IsGroup = value; }
        public List<IGrouping<string, PictureSelectableItemWrapper>>? PictureGroups
        {
            get
            {
                if (IsGroup)
                {
                    return OrderBy switch
                    {
                        OrderByEnum.AddTime => Pictures.GroupBy(p => p.Item.AddTime.ToString("yyyy-MM")).ToList(),
                        OrderByEnum.Title => Pictures.GroupBy(p => (p.Item.Title ?? "")[0].ToString()).ToList(),
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
            get => UserSetting.CardWidth;
            set { UserSetting.CardWidth = value; }
        }
        public double MaxCardWidth { get; set; }

        public BindableCollection<Label> FilterLabels { get; set; } = new();
        public bool ShowFilterLabelPanel { get; set; }

        // 右键菜单栏
        public List<MenuItemViewModel> ContextMenuItems { get; set; }

        public MainPageViewModel(object fatherViewModel, ImageContext context)
        {
            _fatherViewModel = fatherViewModel;

            // 筛选标签更变
            FilterLabels.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) =>
            {
                ShowFilterLabelPanel = FilterLabels.Count > 0;
                RefreshPicture();
            };

            // 切换右键菜单栏
            if (!IsAddPictureMode)
            {
                ContextMenuItems = new()
                {
                    new MenuItemViewModel()
                    {
                        Header = "贴片式打开",
                        Command = new RelayCommand((obj) =>
                        {
                            OpenPictureCommand();
                        }),
                    },
                    new MenuItemViewModel()
                    {
                        Header = "使用外部程序打开",
                        Command = new RelayCommand((obj) =>
                        {
                            OpenPictureWithExternalProgram();
                        }),
                    },
                    new MenuItemViewModel()
                    {
                        IsSeparator = true,
                    },
                    new MenuItemViewModel()
                    {
                        Header = "拷贝图片",
                        Command = new RelayCommand((obj) =>
                        {
                            CopyPicture();
                        }),
                        InputGestureText = "Ctrl+C",
                    },
                    new MenuItemViewModel()
                    {
                        Header ="拷贝图片路径",
                        Command = new RelayCommand((obj) =>
                        {
                            CopyPicturePath();
                        }),
                    },
                    new MenuItemViewModel()
                    {
                        IsSeparator = true,
                    },
                    new MenuItemViewModel()
                    {
                        Header = "添加标签",
                        Command = new RelayCommand((obj) =>
                        {
                            AddPictureLabel();
                        }),
                    },
                    new MenuItemViewModel()
                    {
                        Header = "删除图片",
                        Command = new RelayCommand((obj) =>
                        {
                            DeletePicture();
                        }),
                    },
                };
            }
            else
            {
                ContextMenuItems = new()
                {
                    new MenuItemViewModel()
                    {
                        Header = "添加标签",
                        Command = new RelayCommand((obj) =>
                        {
                            AddPictureLabel();
                        }),
                    },
                    new MenuItemViewModel()
                    {
                        Header="接受添加",
                        Command = new RelayCommand((obj) =>
                        {
                            AcceptToAdd(true);
                        }),
                    },
                    new MenuItemViewModel()
                    {
                        Header="拒绝添加",
                        Command = new RelayCommand((obj) =>
                        {
                            AcceptToAdd(false);
                        }),
                    },
                };
            }
            _context = context;
        }

        public void UpdatePicture()
        {
            IQueryable<Picture> query;
            if (_fatherViewModel is AddImageViewModel addImageViewModel)
                query = addImageViewModel.Pictures.AsQueryable();
            else
                query = _context.Pictures.AsQueryable();

            if (_fatherViewModel is RootViewModel rootViewModel && rootViewModel.SearchText != null)
                query = query.Where(p => EF.Functions.Like(p.Title, "%" + rootViewModel.SearchText + "%"));
            if (FilterLabels.Count != 0)
                query = query.Where(p => p.Labels.Count(l => FilterLabels.Contains(l)) == FilterLabels.Count);
            var orderBy = Enum.GetName(OrderBy);
            if (IsDesc)
                orderBy += " desc";
            query = query.OrderBy(orderBy);
            // 获取总数
            // 第一次加载
            if (_skipNum == 0)
            {
                var count = query.Count();
                Message = string.Format("{0:N0}张图片", count);
            }
            // 分页
            query = query.Skip(_skipNum).Take(UserSetting.TakePictureNumOneTime);
            var resPictures = query.Select(p => new PictureSelectableItemWrapper(p));
            // 第一次加载
            if (_skipNum == 0)
            {
                Pictures = new BindableCollection<PictureSelectableItemWrapper>(resPictures);
            }
            else if (resPictures.Count() != 0)
            {
                Pictures.AddRange(resPictures);
            }

            if (resPictures.Count() != 0)
            {
                _skipNum += UserSetting.TakePictureNumOneTime;
                NotifyOfPropertyChange(nameof(Pictures));
                NotifyOfPropertyChange(nameof(PictureGroups));
            }
        }

        public void Loaded()
        {
            _canFetchMore = true;
        }
        public void RefreshPicture()
        {
            _skipNum = 0;
            UpdatePicture();
        }

        public void FetchMorePicture()
        {
            if (_canFetchMore && _needRefresh && DateTime.Now - _lastFetchTime > TimeSpan.FromSeconds(1))
            {
                _needRefresh = false;
                UpdatePicture();
                _lastFetchTime = DateTime.Now;
            }
        }
        public void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset >= e.ExtentHeight - e.ViewportHeight - 10)
            {
                Debug.WriteLine("到底了");
                _needRefresh = true;
                FetchMorePicture();
            }
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
            Message = string.Format("{0:N0}张图片", Pictures.Count);
            if (selectNum > 0)
                Message += string.Format("\t选中{0:N0}个项目", selectNum);
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
        public void OpenPicture(object sender, MouseButtonEventArgs e)
        {
            if (IsAddPictureMode)
                return;
            var picture = ((PictureSelectableItemWrapper)((Image)sender).DataContext).Item;
            var sticker = new StickerWindow(Path.Join(picture.ImageFolderPath, picture.Path));
            sticker.Show();
        }
        public void OpenPictureCommand()
        {
            SelectedPictures.ForEach(picture =>
            {
                var path = Path.Join(picture.ImageFolderPath, picture.Path);
                var sticker = new StickerWindow(path);
                sticker.Show();
            });
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
        public void CopyPicture()
        {
            var body = string.Join("<br>", SelectedPictures.Select(picture =>
               $"<img src='{Path.Join(picture.ImageFolderPath, picture.Path)}'>"
            ));

            var html = ClipboardHelper.GetHtml(body);
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, html);
            Clipboard.SetDataObject(dataObject);
        }
        public void CopyPicturePath()
        {
            var paths = new List<string>();
            SelectedPictures.ForEach(picture =>
            {
                var path = Path.Join(picture.ImageFolderPath, picture.Path);
                paths.Add(path);
            });
            Clipboard.SetDataObject(string.Join('\n', paths));
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
                UserSetting.WaitToDeleteFiles = deleteFiles;
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
