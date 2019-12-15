using System.Threading.Tasks;
using FLib;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Loading popup displayed on top of the main UI.
	/// Can be used only by <see cref="Subsystems.UIManager"/>, because it needs to be integrated into the UI.
	/// </summary>
	sealed partial class LoadingPopup
		: Grid
	{
		private readonly SplashScreen Splash = null;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public LoadingPopup(SplashScreen splash)
		{
			this.Splash = splash;
			this.InitializeComponent();

			Window.Current.SizeChanged += (s, e) => this.Reposition();
			this.Reposition();
			this.Hide();
		}

		/// <summary>
		/// Shows popup and waits till its displayed.
		/// </summary>
		/// <returns></returns>
		public Task Show()
		{
			if (this.Visibility == Visibility.Visible)
				return Task.FromResult<object>(null);

			this.Progress.IsActive = true;
			this.Visibility = Visibility.Visible;

			return WaitFor.Event<object>(h => this.LayoutUpdated += h, h => this.LayoutUpdated -= h);
		}

		/// <summary>
		/// Hides popup.
		/// </summary>
		public void Hide()
		{
			this.Progress.IsActive = false;
			this.Visibility = Visibility.Collapsed;
		}

		private void Reposition()
		{
			Canvas.SetLeft(this.SplashScreenImage, this.Splash.ImageLocation.Left);
			Canvas.SetTop(this.SplashScreenImage, this.Splash.ImageLocation.Top);
			this.SplashScreenImage.Width = this.Splash.ImageLocation.Width;
			this.SplashScreenImage.Height = this.Splash.ImageLocation.Height;

			this.SplashCanvas.Height = this.Splash.ImageLocation.Bottom;
		}
	}
}
