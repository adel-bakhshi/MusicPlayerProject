using MusicPlayerProject.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicPlayerProject.Converters
{
    [ValueConversion(typeof(Music), typeof(Visibility))]
    public class SelectedMusicVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Music ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}