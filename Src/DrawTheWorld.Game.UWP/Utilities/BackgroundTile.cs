using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Tile "brush"(control) for backgrounds.
	/// 
	/// Based on: http://www.robfe.com/2012/07/creating-tiled-backgrounds-in-metro-style-xaml-apps/
	/// </summary>
	public sealed class BackgroundTile
		: Canvas
	{
		private static readonly ImageSource ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Images/Other/BackgroundPattern120x120.png"));
		private static readonly Size BackgroundSize = new Size(120, 120);

		private Size LastActualSize = default(Size);

		public BackgroundTile()
		{
			this.Loaded += (s, e) => this.LayoutUpdated += this.UpdateLayout;
			this.Unloaded += (s, e) => this.LayoutUpdated -= this.UpdateLayout;
		}

		private void UpdateLayout(object sender, object e)
		{
			if (this.LastActualSize.Width < this.ActualWidth || this.LastActualSize.Height < this.ActualHeight)
			{
				this.LastActualSize.Width = Math.Max(this.LastActualSize.Width, this.ActualWidth);
				this.LastActualSize.Height = Math.Max(this.LastActualSize.Height, this.ActualHeight);
				this.Rebuild();
			}
		}

		private void Rebuild()
		{
			this.Children.Clear();
			for (int x = 0; x < this.LastActualSize.Width; x += (int)BackgroundSize.Width)
			{
				for (int y = 0; y < this.LastActualSize.Height; y += (int)BackgroundSize.Height)
				{
					var img = new Image
					{
						Source = ImageSource,
						CacheMode = new BitmapCache()
					};
					Canvas.SetLeft(img, x);
					Canvas.SetTop(img, y);
					this.Children.Add(img);
				}
			}
			this.Clip = new RectangleGeometry
			{
				Rect = new Rect(0, 0, this.LastActualSize.Width, this.LastActualSize.Height)
			};
		}
	}
}
