using System;
using Windows.UI.Xaml.Data;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Coverts bool to double and back.
	/// 
	/// true -> 1
	/// false -> 0
	/// 
	/// 0 -> false
	/// !0 -> true
	/// </summary>
	public sealed class BoolToDoubleConverter
		: IValueConverter
	{
		/// <inheritdoc />
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value is bool && (bool)value ? 1.0 : 0.0;
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return (value is double) && ((double)value) != 0.0;
		}
	}
}
