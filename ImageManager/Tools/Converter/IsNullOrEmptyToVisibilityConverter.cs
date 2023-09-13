using System.Globalization;
using System.Windows.Data;

namespace ImageManager.Tools.Converter
{
    public class IsNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(string))
            {
                return System.Windows.Visibility.Collapsed;
            }
            return string.IsNullOrEmpty((string)value) ?
                System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
