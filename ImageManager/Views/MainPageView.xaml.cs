using HandyControl.Controls;
using HandyControl.Tools;
using ImageManager.Tools.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ImageManager.Views
{
    /// <summary>
    /// MainPageView.xaml 的交互逻辑
    /// </summary>
    public partial class MainPageView : UserControl
    {
        private DispatcherTimer _updateMouseStateTimer;
        private Border _currentBoxSelectedBorder = null;//拖动展示的提示框
        private bool _isCtrlDown;
        private bool _isMouseDown;
        private bool _isLeftMouse;
        private Point _startPosition;
        private Point _mousePosition;
        private HashSet<ListBoxItem> _preSelectedItems = new();

        public MainPageView()
        {
            InitializeComponent();
            _updateMouseStateTimer = new DispatcherTimer();
            _updateMouseStateTimer.Tick += UpdateMouseStateTimer_Tick;
            //_updateMouseStateTimer.Interval = new System.TimeSpan(0, 0, 0, 0, 100);
        }

        private void UpdateMouseStateTimer_Tick(object? sender, EventArgs eventArgs)
        {
            UpdateSelectBox();
            if (!_isMouseDown)
            {
                ClearSelectBox();
                _updateMouseStateTimer.Stop();
                _isCtrlDown = false;
                _preSelectedItems.Clear();

            }
        }

        private void MouseHook_StatusChanged(object? sender, HandyControl.Data.MouseHookEventArgs e)
        {
            //var window = System.Windows.Window.GetWindow(this);
            _mousePosition = MainCanva.PointFromScreen(new Point(e.Point.X, e.Point.Y));

            if ((_isLeftMouse && e.MessageType == HandyControl.Data.MouseHookMessageType.LeftButtonUp)
                || (!_isLeftMouse && e.MessageType == HandyControl.Data.MouseHookMessageType.RightButtonUp))
            {
                _isMouseDown = false;
                MouseHook.Stop();
                MouseHook.StatusChanged -= MouseHook_StatusChanged;

                // 右键选框，释放后打开右键菜单栏 TODO
                //var GetChildObjects<ListBoxItem>(MainCanva)[0];
            }
        }

        /// <summary>
        /// 鼠标按下记录起始点
        /// </summary>
        private void MainCanva_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            _isLeftMouse = e.LeftButton == MouseButtonState.Pressed;
            if (Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down)
            {
                _isCtrlDown = true;
                // 保存之前选中的项
                // 获取子控件
                var listBoxItems = ControlsSearchHelper.GetChildObjects<ListBoxItem>(MainCanva);
                foreach (var listBoxItem in listBoxItems)
                {
                    if (listBoxItem.IsSelected)
                        _preSelectedItems.Add(listBoxItem);
                }
            }

            _mousePosition = _startPosition = e.GetPosition(MainCanva);
            _updateMouseStateTimer.Start();

            MouseHook.Start();
            MouseHook.StatusChanged += MouseHook_StatusChanged;
        }

        /// <summary>
        /// 更新框选
        /// </summary>
        private void UpdateSelectBox()
        {
            //绘制跟随鼠标移动的方框
            DrawMultiselectBorder(_mousePosition, _startPosition);

            var tempRect = new Rect(_startPosition, _mousePosition);
            //获取子控件
            var listBoxItems = ControlsSearchHelper.GetChildObjects<ListBoxItem>(MainCanva);
            foreach (var listBoxItem in listBoxItems)
            {
                //获取子控件的位置
                var card = ControlsSearchHelper.GetChildObject<Card>(listBoxItem);
                if (card == null)
                    continue;
                Rect tempRect2 = new(card.TranslatePoint(new Point(0, 0), MainCanva), new Size(card.ActualWidth, card.ActualHeight));
                //判断是否在选框内
                if (tempRect.Contains(tempRect2))
                {
                    if (_isCtrlDown)
                    {
                        if (_preSelectedItems.Contains(listBoxItem))
                            listBoxItem.IsSelected = false;
                        else
                            listBoxItem.IsSelected = true;
                    }
                    else
                    {
                        if (!listBoxItem.IsSelected)
                            listBoxItem.IsSelected = true;
                    }

                }
                else if (!_isCtrlDown)
                {
                    if (listBoxItem.IsSelected)
                        listBoxItem.IsSelected = false;
                }

            }
        }

        /// <summary>
        /// 消除选框
        /// </summary>
        private void ClearSelectBox()
        {
            if (_currentBoxSelectedBorder != null)
            {
                MainCanva.Children.Remove(_currentBoxSelectedBorder);
                _currentBoxSelectedBorder = null;
            }
        }

        /// <summary>
        /// 绘制跟随鼠标移动的方框
        /// </summary>
        private void DrawMultiselectBorder(Point endPoint, Point startPoint)
        {
            if (_currentBoxSelectedBorder == null)
            {
                _currentBoxSelectedBorder = new Border
                {
                    Background = new SolidColorBrush((Color)Application.Current.Resources["BackgroundColor"]),
                    Opacity = 0.4,
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["SecondaryBorderColor"]),
                    IsHitTestVisible = false,
                };
                Canvas.SetZIndex(_currentBoxSelectedBorder, 100);
                MainCanva.Children.Add(_currentBoxSelectedBorder);
            }
            _currentBoxSelectedBorder.Width = Math.Abs(endPoint.X - startPoint.X);
            _currentBoxSelectedBorder.Height = Math.Abs(endPoint.Y - startPoint.Y);
            if (endPoint.X - startPoint.X >= 0)
                Canvas.SetLeft(_currentBoxSelectedBorder, startPoint.X);
            else
                Canvas.SetLeft(_currentBoxSelectedBorder, endPoint.X);
            if (endPoint.Y - startPoint.Y >= 0)
                Canvas.SetTop(_currentBoxSelectedBorder, startPoint.Y);
            else
                Canvas.SetTop(_currentBoxSelectedBorder, endPoint.Y);
        }

        /// <summary>
        /// 问题：内层的ListBox拦截了鼠标滚轮事件，导致外层ListBox不能用鼠标滚轮滑动。
        /// 办法：内层ListBox拦截鼠标滚轮事件后，再手动激发一个鼠标滚轮事件，让事件冒泡给外层ListBox接收到。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InnerLB_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                // 内层ListBox拦截鼠标滚轮事件
                e.Handled = true;

                // 激发一个鼠标滚轮事件，冒泡给外层ListBox接收到
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = MouseWheelEvent,
                    Source = sender
                };
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

    }
}
