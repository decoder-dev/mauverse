using System.Globalization;
using mau.DTOModels;

namespace mau.Utils.Converters;

public sealed class SelectedDeptConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is null || values[1] is not int id)
            return Binding.DoNothing;

        return new DeptInfoDTO
        {
            Name = values[0].ToString() ?? string.Empty,
            Id = id
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        Array.ConvertAll(targetTypes, _ => Binding.DoNothing);
}
