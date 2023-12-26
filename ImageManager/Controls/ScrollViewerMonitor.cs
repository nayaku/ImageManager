using ImageManager.Tools.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageManager.Controls
{
    public class ScrollViewerMonitor
    {
        public static readonly DependencyProperty AtEndCommandProperty =
            DependencyProperty.RegisterAttached(
                "AtEndCommand",
                typeof(ICommand),
                typeof(ScrollViewerMonitor),
                new PropertyMetadata(null, OnAtEndCommandChanged));
        public static ICommand GetAtEndCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(AtEndCommandProperty);
        }
        public static void SetAtEndCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(AtEndCommandProperty, value);
        }
        private static void OnAtEndCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.Loaded -= Element_Loaded;
                element.Loaded += Element_Loaded;
            }
        }
        private static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            element.Loaded -= Element_Loaded;
            element.IsVisibleChanged += Element_IsVisibleChanged;
            SetScrollViewerEvent(element);
        }
        private static void FirstCheck(ScrollViewer scrollViewer)
        {
            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 40)
            {
                var command = GetAtEndCommand(scrollViewer);
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                }
            }
        }
        private static void SetScrollViewerEvent(DependencyObject dependencyObject)
        {
            var scrollViewer = dependencyObject as ScrollViewer ??
                ControlsSearchHelper.GetChildObject<ScrollViewer>(dependencyObject, onlyVisible: true);
            if (scrollViewer == null)
                return;
            scrollViewer.ScrollChanged += (sender, e) => ScrollViewer_ScrollChanged(dependencyObject, sender, e);
            FirstCheck(scrollViewer);
        }
        private static void ScrollViewer_ScrollChanged(DependencyObject dependencyObject, object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (e.VerticalOffset >= scrollViewer.ScrollableHeight - 40)
            {
                var command = GetAtEndCommand(dependencyObject);
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                }
            }
        }
        private static void Element_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element.IsVisible)
                SetScrollViewerEvent(element);
        }
    }
}
