using System;

#if WINRT
using Windows.UI.Xaml.Data;
#else
using System.Windows.Data;
#endif

namespace FLib.UI.Data.Converters
{
	/// <summary>
	/// Converts reference too boolean.
	/// If parameter is null/false returns true when value is null. If parameter is true, returns true when value is not null.
	/// </summary>
#if !WINRT
	[ValueConversion(typeof(object), typeof(bool))]
#endif
	public sealed class IsNullConverter
		: IValueConverter
	{
		#region IValueConverter Members
		/// <inheritdoc />
		public object Convert(object value, Type targetType, object parameter,
#if WINRT
			string language
#else
 System.Globalization.CultureInfo culture
#endif
)
		{
			return parameter is bool && (bool)parameter ? value != null : value == null;
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter,
#if WINRT
			string language
#else
 System.Globalization.CultureInfo culture
#endif
)
		{
			throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
		}
		#endregion
	}
}
