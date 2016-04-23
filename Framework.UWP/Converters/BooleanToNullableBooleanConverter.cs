using System;
using Windows.UI.Xaml.Data;

namespace Framework.Converters
{
    public class BooleanToNullableBooleanConverter : IValueConverter
    {
        public bool NullValue { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var boolValue = (bool)value;

            return (bool?)boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var nullableBoolValue = (bool?)value;

            return nullableBoolValue.GetValueOrDefault(NullValue);
        }
    }
}