using System.Windows;
using System.Windows.Media;

namespace ImageManager.Controls
{
    /// <summary>
    /// 按圆角裁剪元素。
    /// Border 的 CornerRadius 不会裁剪子元素，这里在尺寸变化时重设 Clip。
    /// <see cref="RadiusProperty" /> 裁剪四角，<see cref="TopRadiusProperty" /> 只裁剪上面两角，
    /// 用于卡片图片与下方信息区平直衔接。
    /// </summary>
    public static class CornerClip
    {
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.RegisterAttached(
                "Radius",
                typeof(double),
                typeof(CornerClip),
                new PropertyMetadata(0d, OnRadiusChanged));

        public static readonly DependencyProperty TopRadiusProperty =
            DependencyProperty.RegisterAttached(
                "TopRadius",
                typeof(double),
                typeof(CornerClip),
                new PropertyMetadata(0d, OnRadiusChanged));

        public static void SetRadius(DependencyObject element, double value) =>
            element.SetValue(RadiusProperty, value);

        public static double GetRadius(DependencyObject element) =>
            (double)element.GetValue(RadiusProperty);

        public static void SetTopRadius(DependencyObject element, double value) =>
            element.SetValue(TopRadiusProperty, value);

        public static double GetTopRadius(DependencyObject element) =>
            (double)element.GetValue(TopRadiusProperty);

        private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element)
                return;
            element.SizeChanged -= OnSizeChanged;
            element.SizeChanged += OnSizeChanged;
            ApplyClip(element);
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e) =>
            ApplyClip((FrameworkElement)sender);

        private static void ApplyClip(FrameworkElement element)
        {
            var width = element.ActualWidth;
            var height = element.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                element.Clip = null;
                return;
            }

            var topRadius = GetTopRadius(element);
            if (topRadius > 0)
            {
                element.Clip = CreateTopRoundedGeometry(width, height, topRadius);
                return;
            }

            var radius = GetRadius(element);
            element.Clip = radius <= 0
                ? null
                : new RectangleGeometry(new Rect(0, 0, width, height), radius, radius);
        }

        /// <summary>
        /// 生成只有上面两角带圆角的矩形。
        /// </summary>
        private static Geometry CreateTopRoundedGeometry(double width, double height, double radius)
        {
            radius = Math.Min(radius, Math.Min(width, height) / 2);
            var figure = new PathFigure
            {
                StartPoint = new Point(0, radius),
                IsClosed = true,
                IsFilled = true
            };
            figure.Segments.Add(new ArcSegment(
                new Point(radius, 0), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(new Point(width - radius, 0), true));
            figure.Segments.Add(new ArcSegment(
                new Point(width, radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(new Point(width, height), true));
            figure.Segments.Add(new LineSegment(new Point(0, height), true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.Freeze();
            return geometry;
        }
    }
}
