using System.Globalization;

namespace mau.Utils.Converters;

public sealed class PageInfoConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        values.Length < 2 ? string.Empty : $"{values[0]} из {values[1]}";

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        Array.ConvertAll(targetTypes, _ => Binding.DoNothing);
}
