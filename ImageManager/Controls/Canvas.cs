using System.Windows;

namespace ImageManager.Controls
{
    public class Canvas : System.Windows.Controls.Canvas
    {
        protected override Size MeasureOverride(Size constraint)
        {
            foreach (UIElement child in InternalChildren)
            {
                child?.Measure(constraint);
            }
            return new Size();
        }
    }
}
