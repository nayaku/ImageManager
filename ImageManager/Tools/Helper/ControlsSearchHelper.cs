using System.Windows;
using System.Windows.Media;

namespace ImageManager.Tools.Helper
{
    public class ControlsSearchHelper
    {
        /// <summary>
        /// 查找父控件
        /// </summary>
        /// <typeparam name="T">父控件的类型</typeparam>
        /// <param name="obj">要找的是obj的父控件</param>
        /// <param name="name">想找的父控件的Name属性</param>
        /// <returns>目标父控件</returns>
        public static T? GetParentObject<T>(DependencyObject obj, string name = "") where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is T t && t.Name == name | string.IsNullOrEmpty(name))
                {
                    return t;
                }

                // 在上一级父控件中没有找到指定名字的控件，就再往上一级找
                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }


        /// <summary>
        /// 查找子控件
        /// </summary>
        /// <typeparam name="T">子控件的类型</typeparam>
        /// <param name="obj">要找的是obj的子控件</param>
        /// <param name="name">想找的子控件的Name属性</param>
        /// <returns>目标子控件</returns>
        public static T? GetChildObject<T>(DependencyObject obj, string name = "", bool onlyVisible = false) where T : FrameworkElement
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(obj);

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(parent) - 1; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is FrameworkElement element && onlyVisible && element.Visibility != Visibility.Visible)
                        continue;

                    queue.Enqueue(child);
                    if (child is T t && t.Name == name | string.IsNullOrEmpty(name))
                        return t;
                }
            }
            return null;

        }


        /// <summary>
        /// 获取所有同一类型的子控件
        /// </summary>
        /// <typeparam name="T">子控件的类型</typeparam>
        /// <param name="obj">要找的是obj的子控件集合</param>
        /// <param name="name">想找的子控件的Name属性</param>
        /// <returns>子控件集合</returns>
        public static List<T> GetChildObjects<T>(DependencyObject obj, string name = "") where T : FrameworkElement
        {
            List<T> childList = [];

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child is T t && (t.Name == name || string.IsNullOrEmpty(name)))
                {
                    childList.Add(t);
                }

                childList.AddRange(GetChildObjects<T>(child, ""));
            }

            return childList;

        }
    }
}