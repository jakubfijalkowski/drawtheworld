using System;
#if WINRT
using Windows.UI;

#else
using System.Windows.Media;
#endif

namespace FLib.UI
{
	#region Config
	static class HsvColorConfig
	{
		public const double Epsilon = 0.000001;
	}
	#endregion

	/// <summary>
	/// HSV representation of color.
	/// </summary>
	public partial struct HsvColor
	{
		/// <summary>
		/// Hue.
		/// Value ranges from 0.0 to 360.
		/// </summary>
		public double Hue;

		/// <summary>
		/// Saturation.
		/// Value ranges from 0.0 to 1.0.
		/// </summary>
		public double Saturation;

		/// <summary>
		/// Value.
		/// Value ranges from 0.0 to 1.0.
		/// </summary>
		public double Value;
	}

	/// <summary>
	/// Helper classes for <see cref="HsvColor"/>.
	/// </summary>
	public static class HsvColorHelper
	{
		/// <summary>
		/// Casts <see cref="Color"/> to <see cref="HsvColor"/>.
		/// </summary>
		/// <returns></returns>
		public static HsvColor ToHSV(this Color rgb)
		{
			var hsv = new HsvColor();
			ToHSV(rgb.R, rgb.G, rgb.B, ref hsv);
			return hsv;
		}

		/// <summary>
		/// Casts <see cref="HsvColor"/> to <see cref="Color"/>.
		/// </summary>
		/// <returns></returns>
		public static Color ToRGB(this HsvColor hsv)
		{
			double r = 0.0, g = 0.0, b = 0.0;

			if (hsv.Saturation == 0.0)
			{
				r = g = b = hsv.Value;
			}
			else
			{
				double h = (hsv.Hue == 360 ? 0 : hsv.Hue) / 60;

				int i = (int)h;
				double f = h - i,
					p = hsv.Value * (1.0 - hsv.Saturation),
					q = hsv.Value * (1.0 - hsv.Saturation * f),
					t = hsv.Value * (1.0 - hsv.Saturation * (1.0 - f));
				switch (i)
				{
					case 0:
						r = hsv.Value;
						g = t;
						b = p;
						break;

					case 1:
						r = q;
						g = hsv.Value;
						b = p;
						break;

					case 2:
						r = p;
						g = hsv.Value;
						b = t;
						break;

					case 3:
						r = p;
						g = q;
						b = hsv.Value;
						break;

					case 4:
						r = t;
						g = p;
						b = hsv.Value;
						break;

					default:
						r = hsv.Value;
						g = p;
						b = q;
						break;
				}
			}

			return Color.FromArgb(255, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
		}

		/// <summary>
		/// Converts RGB value to <see cref="HsvColor"/>.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static HsvColor FromRGB(byte r, byte g, byte b)
		{
			var hsv = new HsvColor();
			ToHSV(r, g, b, ref hsv);
			return hsv;
		}

		/// <summary>
		/// Checks for equality of two <see cref="HsvColor"/>s.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool Equals(this HsvColor a, HsvColor b)
		{
			return Math.Abs(a.Hue - b.Hue) < HsvColorConfig.Epsilon &&
				Math.Abs(a.Saturation - b.Saturation) < HsvColorConfig.Epsilon &&
				Math.Abs(a.Value - b.Value) < HsvColorConfig.Epsilon;
		}

		internal static void ToHSV(byte r, byte g, byte b, ref HsvColor hsv)
		{
			double min = Math.Min(Math.Min(r, g), b);
			hsv.Value = Math.Max(Math.Max(r, g), b);
			double delta = hsv.Value - min;

			hsv.Saturation = hsv.Value == 0.0 ? 0.0 : delta / hsv.Value;

			hsv.Hue =
				hsv.Saturation == 0.0 ? 0.0 :
				r == hsv.Value ? (g - b) / delta :
				g == hsv.Value ? 2 + (b - r) / delta :
				b == hsv.Value ? 4 + (r - g) / delta : 0.0;

			hsv.Hue *= 60;
			if (hsv.Hue < 0.0)
				hsv.Hue += 360;

			hsv.Value /= 255;
		}
	}

	/// <summary>
	/// Set of predefined colors(converted from <see cref="Colors"/>)
	/// </summary>
	public static class HsvColors
	{
		/// <summary>
		/// Red.
		/// </summary>
		public static HsvColor Red
		{
			get { return Colors.Red.ToHSV(); }
		}

		/// <summary>
		/// Green.
		/// </summary>
		public static HsvColor Green
		{
			get { return Colors.Green.ToHSV(); }
		}

		/// <summary>
		/// Blue.
		/// </summary>
		public static HsvColor Blue
		{
			get { return Colors.Blue.ToHSV(); }
		}

		/// <summary>
		/// White.
		/// </summary>
		public static HsvColor White
		{
			get { return Colors.White.ToHSV(); }
		}

		/// <summary>
		/// Black
		/// </summary>
		public static HsvColor Black
		{
			get { return Colors.Black.ToHSV(); }
		}
	}
}
