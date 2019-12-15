using System;
using FLib;
using Windows.UI.Xaml.Media.Imaging;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Chooses the appropriate background for the <see cref="Pages.MainMenu"/>.
	/// </summary>
	static class GameBackgroundChooser
	{
		private const string BackgroundPath = "ms-appx:///Assets/Images/Other/MainBackground.{0}.png";
		private static readonly int[] AvailableSizes = new int[] { 1366, 1920, 2560 };

		/// <summary>
		/// Gets the path to the selected background.
		/// </summary>
		public static readonly Uri SelectedBackgroundPath = null;

		/// <summary>
		/// Gets the selected background image.
		/// </summary>
		public static readonly BitmapImage SelectedBackground = null;

		static GameBackgroundChooser()
		{
			var w = Windows.UI.Xaml.Window.Current.Bounds.Width;
			int i = 0;
			while (AvailableSizes[i] < w && i < AvailableSizes.Length)
				++i;

			SelectedBackgroundPath = new Uri(BackgroundPath.FormatWith(AvailableSizes[i]));
			SelectedBackground = new BitmapImage(SelectedBackgroundPath);
		}
	}
}
