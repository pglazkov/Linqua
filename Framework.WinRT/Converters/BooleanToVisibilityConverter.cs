using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Framework.Converters
{
	public sealed class BooleanToVisibilityConverter : IValueConverter
	{
		public bool IsReversed { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var val = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);

			if (IsReversed)
			{
				val = !val;
			}

			if (val)
			{
				return Visibility.Visible;
			}

			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			var visibility = (Visibility)value;

			var val = visibility == Visibility.Visible;

			return IsReversed ? !val : val;
		}
	}
}