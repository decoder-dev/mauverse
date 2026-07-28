using System.Globalization;
using mau.Models;

namespace mau.Utils.Converters;

public sealed class MenuPopupConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.Length >= 2 && values[0] is Border button && values[1] is int scheduleId
            ? new ButtonParameters { Button = button, Id = scheduleId }
            : Binding.DoNothing;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        Array.ConvertAll(targetTypes, _ => Binding.DoNothing);
}
