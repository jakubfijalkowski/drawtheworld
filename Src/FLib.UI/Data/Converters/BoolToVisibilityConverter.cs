using System;
#if WINRT
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace FLib.UI.Data.Converters
{
	/// <summary>
	/// Converts boolean value to <see cref="Visibility"/>.
	/// </summary>
	/// <remarks>
	/// True changes to <see cref="Visibility.Visible"/>.
	/// False changes to <see cref="Visibility.Collapsed"/>.
	/// </remarks>
#if !WINRT
	[ValueConversion(typeof(bool), typeof(Visibility))]
#endif
	public sealed class BoolToVisibilityConverter
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
			return (bool)value ? Visibility.Visible : Visibility.Collapsed;
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
			return (Visibility)value == Visibility.Visible;
		}
		#endregion
	}
}
