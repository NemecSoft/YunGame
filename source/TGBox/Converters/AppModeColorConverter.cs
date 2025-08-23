using System;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TGBox.Models;

namespace TGBox.Converters
{
    public class AppModeColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is AppMode mode)
            {
                return mode switch
                {
                    AppMode.PlayMode => new SolidColorBrush(Colors.Green),
                    AppMode.ManageMode => new SolidColorBrush(Colors.Orange),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}