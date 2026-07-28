using System.Globalization;

namespace mau.Utils.Converters;

public sealed class HideLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && !string.IsNullOrWhiteSpace(text);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

public sealed class HideMultiLabelConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        values.Any(value => value is string text && !string.IsNullOrWhiteSpace(text));

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        Array.ConvertAll(targetTypes, _ => Binding.DoNothing);
}
