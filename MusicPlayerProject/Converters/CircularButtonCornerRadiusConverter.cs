using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicPlayerProject.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class CircularButtonCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)value;
            return width / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double radius = (double)value;
            return radius * 2;
        }
    }
}