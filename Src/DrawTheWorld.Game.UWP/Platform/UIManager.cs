using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using FLib;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DrawTheWorld.Game.Platform
{
	/// <summary>
	/// Displays background tile, provides loading popup and 
	/// </summary>
	sealed class UIManager
		: IUIManager
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.UIManager");
		private static readonly Lazy<Brush> LoadingPopupBackgroundBrush = new Lazy<Brush>(() => (Brush)Application.Current.Resources["LoadingPopupBackgroundBrush"]);

		private static readonly string[] ImagesToPreload = new string[]
		{
			"Assets/Images/Lists/Pack.png",
			"Assets/Images/Lists/PackFolders.png",
			"Assets/Images/Lists/NotFinished.png",
			"Assets/Images/Other/ButtonNormal.png",
			"Assets/Images/Other/MainMenuFrame.png",
			"Assets/Images/Logos/BlankLogo.png",
			"Assets/Images/Popup/NormalWithShadow.png",
			"Assets/Images/SettingsPanel/Background.png",
			"Assets/Images/AppBar/Background.png"
		};
		private static readonly string[] DirsToPreload = new string[]
		{
			"Assets/Tools"
		};

		private readonly ISoundManager SoundManager = null;
		private readonly IPartsProvider PartsProvider = null;

		private readonly Grid MainGrid = new Grid();
		private readonly Utilities.UIContainer PageContainer = null;
		private readonly Panel _MediaContainer = new StackPanel { Visibility = Visibility.Collapsed };

		private readonly Lazy<Controls.SettingsAndAccount> Account = null;
		private readonly Lazy<Controls.CoinsUI> Coins = null;
		private readonly Lazy<Controls.ModeSelector> ModeSelector = null;
		private readonly Lazy<Controls.AppInfo> AppInfo = null;
		private readonly Lazy<Controls.BugReporting> BugReporting = null;
		private Controls.LoadingPopup LoadingPopup = null;

		/// <summary>
		/// Gets the container for the media.
		/// </summary>
		public Panel MediaContainer
		{
			get { return this._MediaContainer; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="soundManager"></param>
		/// <param name="partsProvider"></param>
		/// <param name="container"></param>
		/// <param name="account"></param>
		/// <param name="coins"></param>
		/// <param name="modeSelector"></param>
		/// <param name="appInfo"></param>
		public UIManager(
			ISoundManager soundManager, IPartsProvider partsProvider,
			Utilities.UIContainer container, Lazy<Controls.SettingsAndAccount> account, Lazy<Controls.ModeSelector> modeSelector, Lazy<Controls.CoinsUI> coins, Lazy<Controls.AppInfo> appInfo,
			Lazy<Controls.BugReporting> bugReporting
			)
		{
			this.SoundManager = soundManager;
			this.PartsProvider = partsProvider;

			this.PageContainer = container;
			this.Account = account;
			this.ModeSelector = modeSelector;
			this.Coins = coins;
			this.AppInfo = appInfo;
			this.BugReporting = bugReporting;
		}

		/// <inheritdoc />
		public T GetCurrentPage<T>()
			where T : class, IPage
		{
			return this.PageContainer.CurrentPage as T;
		}

		/// <inheritdoc />
		public T NavigateTo<T>()
			where T : class, IPage
		{
			Logger.Info("Navigating to {0}.", typeof(T).Name);
			bool isFirst = this.PageContainer.CurrentPage == null;
			var page = this.PageContainer.Navigate<T>();
			if (!isFirst)
				this.SoundManager.PlaySound(Sound.PageChanged);

			if (this.PageContainer.Visibility == Visibility.Visible)
				this.MainGrid.Background = this.PageContainer.CurrentPage.PageBackground;
			return page;
		}

		/// <inheritdoc />
		public IPage NavigateBack()
		{
			Logger.Info("Navigating back.");
			var page = this.PageContainer.NavigateBack();
			this.SoundManager.PlaySound(Sound.PageChanged);
			this.MainGrid.Background = page.PageBackground;
			return page;
		}

		/// <inheritdoc />
		public Task ShowLoading()
		{
			this.PageContainer.Visibility = Visibility.Collapsed;
			this.MainGrid.Background = LoadingPopupBackgroundBrush.Value;
			return this.LoadingPopup.Show();
		}

		/// <inheritdoc />
		public void HideLoadingPopup()
		{
			this.PageContainer.Visibility = Visibility.Visible;
			if (this.PageContainer.CurrentPage != null)
				this.MainGrid.Background = this.PageContainer.CurrentPage.PageBackground;
			this.LoadingPopup.Hide();
		}

		/// <inheritdoc />
		public Task<Core.IGameMode> SelectGameMode()
		{
			this.SoundManager.PlaySound(Sound.PageChanged);
			return this.ModeSelector.Value.SelectMode();
		}

		/// <inheritdoc />
		public Task<bool> ShowCoinsUI(bool showHint)
		{
			if (!showHint)
				this.SoundManager.PlaySound(Sound.PageChanged);
			return this.Coins.Value.Show(showHint);
		}

		/// <summary>
		/// Initializes the UI and activates current Window.
		/// Shows the loading popup, that has to be hidden after initialization process is completed.
		/// </summary>
		public async Task Initialize(SplashScreen splashScreen)
		{
			// Show loading popup before any element
			this.LoadingPopup = new Controls.LoadingPopup(splashScreen);
			this.MainGrid.Children.Add(new Utilities.BackgroundTile());
			this.MainGrid.Children.Add(this.LoadingPopup);
			var ignored = this.ShowLoading();

			Window.Current.Content = this.MainGrid;
			Window.Current.Activate();

			await this.PreloadBackground();

			// Add rest of the elements
			this.MainGrid.Children.AddBeforeLast(this.MediaContainer);
			this.MainGrid.Children.AddBeforeLast(this.Account.Value);
			this.MainGrid.Children.AddBeforeLast(this.AppInfo.Value);
			this.MainGrid.Children.AddBeforeLast(this.PageContainer);

			this.MainGrid.Children.Add(this.ModeSelector.Value);
			this.MainGrid.Children.Add(this.Coins.Value);
			this.MainGrid.Children.Add(this.BugReporting.Value);

			this.PreloadImagesAsync();
		}

		/// <summary>
		/// Only activates current <see cref="Window"/>.
		/// </summary>
		public void ActivateOnly()
		{
			Window.Current.Content = this.MainGrid;
			Window.Current.Activate();
		}

		/// <summary>
		/// Shows <see cref="IMessageDialog"/>.
		/// </summary>
		/// <param name="container"></param>
		/// <returns></returns>
		public async Task ShowMessageDialog(IMessageDialog dialog)
		{
			Validate.Debug(() => dialog, v => v.NotNull().IsOfType(typeof(UIElement)));
			Validate.Debug(() => this.MainGrid, v => v.That(g => !(g.Children.Last() is IMessageDialog), "Message dialog is already shown."));

			this.SoundManager.PlaySound(Sound.PageChanged);
			this.MainGrid.Children.Add((UIElement)dialog);
			await dialog.OpenAsync();
			await WaitFor.Event<object>(h => dialog.Closed += h, h => dialog.Closed -= h);
			this.MainGrid.Children.RemoveAt(this.MainGrid.Children.Count - 1);
		}

		private async void PreloadImagesAsync()
		{
			var images = await ListImages();
			foreach (var img in images)
				this.PreloadImage(new Uri("ms-appx:///" + img));
		}

		private Task PreloadBackground()
		{
			Logger.Debug("Preloading background from file: {0}.", Utilities.GameBackgroundChooser.SelectedBackgroundPath.AbsolutePath);

			var img = new Image { Source = Utilities.GameBackgroundChooser.SelectedBackground, Visibility = Visibility.Collapsed };
			this.MainGrid.Children.AddBeforeLast(img);
			ExceptionRoutedEventHandler h2 = null;
			return WaitFor.RoutedEvent(
				h =>
				{
					h2 = (s, e) => h(s, e);
					img.ImageOpened += h;
					img.ImageFailed += h2;
				},
				h =>
				{
					img.ImageOpened -= h;
					img.ImageFailed -= h2;
				});
		}

		private static async Task<IEnumerable<string>> ListImages()
		{
			List<string> list = new List<string>();
			list.AddRange(ImagesToPreload);

			foreach (var dir in DirsToPreload)
			{
				var parts = dir.Split('/');
				var folder = Package.Current.InstalledLocation;
				foreach (var p in parts)
					folder = await folder.GetFolderAsync(p);

				var images = await folder.GetFilesAsync();
				list.AddRange(images.Select(img => dir + "/" + img.Name));
			}

			return list;
		}

		private void PreloadImage(Uri path)
		{
			Logger.Debug("Preloading image '{0}'.", path.AbsolutePath);

			var src = new BitmapImage(path);
			this.MainGrid.Children.AddBeforeLast(new Image { Source = src, Visibility = Visibility.Collapsed });
		}
	}

	/// <summary>
	/// Defines classes that acts as a dialog.
	/// </summary>
	interface IMessageDialog
	{
		/// <summary>
		/// Opens the dialog and returns <see cref="IAsyncAction"/> that waits for its open.
		/// </summary>
		/// <returns></returns>
		IAsyncAction OpenAsync();

		/// <summary>
		/// Raised when the dialog is closed.
		/// </summary>
		event EventHandler<object> Closed;
	}

	static class UIElementCollectionHelper
	{
		public static void AddBeforeLast(this UIElementCollection collection, UIElement element)
		{
			collection.Insert(collection.Count - 1, element);
		}
	}
}
