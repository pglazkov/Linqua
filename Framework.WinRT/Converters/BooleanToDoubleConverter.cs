using System;
using Windows.UI.Xaml.Data;

namespace Framework.Converters
{
	public class BooleanToDoubleConverter : IValueConverter
	{
		public double TrueValue { get; set; }
		public double FalseValue { get; set; }
		public double NullValue { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (ReferenceEquals(value, null))
			{
				return NullValue;
			}

			var boolValue = (bool)value;

			return boolValue ? TrueValue : FalseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}