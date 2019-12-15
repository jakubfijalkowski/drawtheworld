using System;
#if WINRT
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI;
#else
using System.Windows.Data;
using System.Windows.Media;
#endif

namespace FLib.UI.Data.Converters
{
	/// <summary>
	/// Converts <see cref="Color"/> to <see cref="SolidColorBrush"/>.
	/// </summary>
#if !WINRT
	[ValueConversion(typeof(Color), typeof(SolidColorBrush))]
#endif
	public sealed class ColorToSolidBrush
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
			if (value is Color)
			{
				var c = (Color)value;
				return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
			}
			return null;
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
			if (targetType == typeof(Color) && value is SolidColorBrush)
				return ((SolidColorBrush)value).Color;
			return null;
		}
		#endregion
	}
}
