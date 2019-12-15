#if WINRT
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace FLib.UI
{
	/// <summary>
	/// Extensions for <see cref="Point"/>.
	/// </summary>
	public static class PointExtensions
	{
		/// <summary>
		/// Clamps both values.
		/// </summary>
		/// <param name="pt"></param>
		/// <param name="minX"></param>
		/// <param name="maxX"></param>
		/// <param name="minY"></param>
		/// <param name="maxY"></param>
		/// <returns></returns>
		public static Point Clamp(this Point pt, double minX, double maxX, double minY, double maxY)
		{
			if (pt.X < minX)
				pt.X = minX;
			else if (pt.X > maxX)
				pt.X = maxX;

			if (pt.Y < minY)
				pt.Y = minY;
			else if (pt.Y > maxY)
				pt.Y = maxY;
			return pt;
		}
	}
}
