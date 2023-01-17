using HandyControl.Tools.Extension;
using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Tools;
using Microsoft.EntityFrameworkCore;
using Stylet;
using StyletIoC;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Linq.Dynamic.Core;
using System.Windows.Media;
using Label = ImageManager.Data.Model.Label;
using Path = System.IO.Path;

namespace ImageManager.ViewModels
{
    public class MainPageViewModel : PropertyChangedBase, IInjectionAware
    {
        [Inject]
        public UserSettingData UserSetting { get; set; }
        [Inject]
        public ImageContext Context { get; set; }
        private RootViewModel _rootViewModel;
        private IWindowManager _windowManager;
        private IContainer _container;

        public string Message { get; set; }
        public BindableCollection<SelectableItemWrapper<Picture>> Pictures { get; set; }
        public IEnumerable<Picture> SelectedPictures => Pictures.Where(x => x.IsSelected).Select(x => x.Item);
        //public enum SelectedModeEnum { None = 0, Single = 1, Multiply = 2 }
        //public SelectedModeEnum SelectedMode => (SelectedModeEnum)SelectedPictures.Take(2).Count();
        //public bool IsAnySelected => Pictures.Any(x => x.IsSelected);
        public bool IsDesc { get; set; }
        public enum OrderByEnum { AddTime, Title }
        public OrderByEnum OrderBy { get; set; }
        public bool IsOrderByAddTime => OrderBy == OrderByEnum.AddTime;
        public bool IsOrderByTitle => OrderBy == OrderByEnum.Title;
        public string SearchText => _rootViewModel.SearchText;
        public bool IsGroup { get; set; }
        public List<IGrouping<string, SelectableItemWrapper<Picture>>>? PictureGroups
        {
            get
            {
                if (IsGroup)
                {
                    return OrderBy switch
                    {
                        OrderByEnum.AddTime => Pictures.GroupBy(p => p.Item.AddTime.ToString("yyyy-MM")).ToList(),
                        OrderByEnum.Title => Pictures.GroupBy(p => (p.Item.Title ?? "")[0].ToString()).ToList(),
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
            get => UserSetting?.CardWidth ?? 240;
            set
            {
                UserSetting.CardWidth = value;
                _saveUserSettingTimer.Stop();
                _saveUserSettingTimer.Start();
            }
        }
        public double MaxCardWidth { get; set; }
        public int GroupNum => Math.Max(Math.Min(Pictures.Count, (int)(MaxCardWidth / CardWidth)), 1);
        public BindableCollection<Label> FilterLabels { get; set; } = new();

        public bool ShowFilterLabelPanel { get; set; }
        private System.Timers.Timer _saveUserSettingTimer;

        public MainPageViewModel(RootViewModel rootViewModel, IWindowManager windowManager, IContainer container)
        {
            _rootViewModel = rootViewModel;
            _windowManager = windowManager;
            _container = container;

            // 设置保存用户设置的定时器
            _saveUserSettingTimer = new(200)
            {
                AutoReset = false
            };
            _saveUserSettingTimer.Elapsed += (object? sender, ElapsedEventArgs e) => UserSetting.Save();
            // 筛选标签更变
            FilterLabels.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) =>
            {
                ShowFilterLabelPanel = FilterLabels.Count > 0;
                UpdatePicture();
            };
        }

        public void UpdatePicture()
        {
            var query = Context.Pictures.AsQueryable();
            if (SearchText != null)
                query = query.Where(p => EF.Functions.Like(p.Title, "%" + SearchText + "%"));
            if (FilterLabels.Count != 0)
                query = query.Where(p => p.Labels.Count(l => FilterLabels.Contains(l)) == FilterLabels.Count);
            var orderBy = Enum.GetName(OrderBy);
            if (IsDesc)
                orderBy += " desc";
            query = query.OrderBy(orderBy);
            var resPictures = query.Select(p => new SelectableItemWrapper<Picture>(p));
            Pictures = new BindableCollection<SelectableItemWrapper<Picture>>(resPictures);
            Message = string.Format("{0:N0}张图片", Pictures.Count);
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
            var picture = (SelectableItemWrapper<Picture>)(parent).DataContext;

            picture.Item.Labels.Remove(label);
            Context.SaveChanges();
        }

        public void PictureSelectionChange()
        {
            var selectNum = Pictures.Count(p => p.IsSelected);
            if (selectNum > 0)
                Message = string.Format("{0:N0}张图片\t选中{1:N0}个项目", Pictures.Count, selectNum);
            else
                Message = string.Format("{0:N0}张图片", Pictures.Count);
        }

        public void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                MaxCardWidth = e.NewSize.Width - 5;
                CardWidth = Math.Min(CardWidth, MaxCardWidth);
            }
        }

        #region 右键菜单栏
        public void OpenPicture()
        {
            // TODO 使用贴片打开
            throw new NotImplementedException();
        }
        public void OpenPictureWithexternalProgram()
        {
            SelectedPictures.ForEach(picture =>
            {
                var path = Path.Join(Picture.ImageFolderPath, picture.Path);
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
            //var pictures = Pictures.Where(p => p.IsSelected).Select(p => p.Item).ToList();
            var body = string.Join("<br>", SelectedPictures.Select(picture =>
               $"<img src='{Path.Join(Picture.ImageFolderPath, picture.Path)}'>"
            ));

            var html = ClipboardHelper.GetHtml(body);
            Debug.WriteLine(html);
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, html);
            Clipboard.SetDataObject(dataObject);
            //}

        }
        public void CopyPicturePath()
        {
            var paths = new List<string>();
            SelectedPictures.ForEach(picture =>
            {
                var path = Path.Join(Picture.ImageFolderPath, picture.Path);
                paths.Add(path);
            });
            Clipboard.SetDataObject(string.Join('\n', paths));
        }
        public void AddPictureLabel()
        {
            var pictureAddLabelViewModel = new PictureAddLabelViewModel();
            _container.BuildUp(pictureAddLabelViewModel);
            bool? res = _windowManager.ShowDialog(pictureAddLabelViewModel);
            if (res ?? false)
            {
                var labelName = pictureAddLabelViewModel.SearchText;
                var label = Context.Labels.SingleOrDefault(l => l.Name == labelName)
                    ?? new Label { Name = labelName };

                SelectedPictures.ForEach(p =>
                {
                    if (!p.Labels.Contains(label))
                        p.Labels.Add(label);
                });
                Pictures.Refresh();
                Context.SaveChanges();
            }
        }
        public void DeletePicture()
        {
            var dialogViewModel = new DialogViewModel
            {
                Title = "删除图片",
                Message = "确定删除选中的图片吗？",
                ConfirmText = "删  除",
                CancelText = "取  消",
                ShowCancel = true,
                ConfirmButtonStyle = "ButtonDanger"
            };
            var res = _windowManager.ShowDialog(dialogViewModel);
            if (res ?? false)
            {
                var deletePictures = Pictures.Where(p => p.IsSelected).ToList();
                Context.Pictures.RemoveRange(deletePictures.Select(p => p.Item));
                Context.SaveChanges();
                Pictures.RemoveRange(deletePictures);
                deletePictures.ForEach(p => p.Item.DeleteFile());
            }
        }

        public void UpdateOrderBy(string orderByString)
        {
            var orderBy = (OrderByEnum)Enum.Parse(typeof(OrderByEnum), orderByString);
            if (orderBy != OrderBy)
            {
                OrderBy = orderBy;
                UpdatePicture();
            }
        }

        public void UpdateIsDesc(string isDescString)
        {
            var isDesc = Boolean.Parse(isDescString);
            if (IsDesc != isDesc)
            {
                IsDesc = isDesc;
                UpdatePicture();
            }
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
            UpdatePicture();
        }
    }
}
