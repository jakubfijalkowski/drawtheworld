using System;
using System.Linq;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Game.Helpers;
using DrawTheWorld.Game.Utilities;
using FLib.UI.Controls;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DrawTheWorld.Game.Pages
{
	/// <summary>
	/// Page that displays designer and related info.
	/// </summary>
	sealed partial class DesignerPage
		: UIPage
	{
		private const string ResourceKey = "Pages_DesignerPage_";
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.DesignerPage");

		private readonly IUIManager UIManager = null;
		private readonly IGameManager GameManager = null;

		private readonly ZoomHandler Zoom = null;

		private Core.Designer Game = null;
		private Core.UserData.MutableBoardData Board = null;

		/// <summary>
		/// Gets value that indicates whether user requested save or wants to discard the changes.
		/// </summary>
		public bool SaveRequested { get; private set; }

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public DesignerPage(IUIManager uiManager, IGameManager gameManager, UISettings uiSettings)
		{
			this.UIManager = uiManager;
			this.GameManager = gameManager;

			this.InitializeComponent();

			this.Zoom = new ZoomHandler(uiManager, uiSettings, this.ZoomButton, this.BottomAppBar, this.MainGame);
		}

		protected internal override void OnNavigatedTo(Type sourcePageType)
		{
			Logger.Trace("Navigated to the DesignerPage, showing board.");


			this.Zoom.Reset();
			this.SaveRequested = false;
			this.Game = GameManager.Game as Core.Designer;
			this.Board = this.Game.Board.BoardInfo as Core.UserData.MutableBoardData;

			this.BoardSettings.DataContext = this.Board;

			if (this.Palette != null)
				this.Palette.Children.Clear();

			this.MainGame.Reload();
		}

		protected internal override void OnNavigatedFrom(Type destinationPageType)
		{
			Logger.Trace("Navigated from tht DesignerPage, cleaning.");

			this.Game = null;
			this.Board = null;

			this.BottomAppBar.IsOpen = false;
		}

		private void OpenSettings(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Logger.Trace("Opening settings.");
			this.BottomAppBar.IsOpen = false;
			this.BoardSettings.IsOpen = true;
		}

		private void DiscardChanges(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Logger.Trace("Discarding changed and finishing the game.");
			this.BottomAppBar.IsOpen = false;
			this.SaveRequested = false;
			this.Game.Finish();
		}

		private void SaveBoard(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			Logger.Trace("Saving changes and finishing the game.");
			this.BottomAppBar.IsOpen = false;
			this.SaveRequested = true;
			this.Game.Finish();
		}

		private void UpdateSettings(FLib.UI.Controls.SettingsPanel sender, object args)
		{
			Logger.Trace("Updating board settings panel data.");

			// See SettingsPanel remarks for justification
			if (this.BoardSizeTextBox == null)
				this.BoardSizeTextBox = (TextBox)((FrameworkElement)this.BoardSettings.Content).FindName("BoardSizeTextBox");
			if (this.Palette == null)
				this.Palette = (UniformGrid)((FrameworkElement)this.BoardSettings.Content).FindName("Palette");
			if (this.ColorPicker == null)
				this.ColorPicker = (Controls.ColorPicker.SettingsPicker)((FrameworkElement)this.BoardSettings.Content).FindName("ColorPicker");

			this.BoardSizeTextBox.Text = this.Board.Size.Width + " x " + this.Board.Size.Height;

			if (this.Palette.Children.Count == 0)
			{
				foreach (var entry in this.Game.Palette)
					this.Palette.Children.Add(this.CreatePaletteEntry(entry));

				// Rest of colors
				for (int i = this.Palette.Children.Count; i < Core.Config.MaxPaletteSize; i++)
					this.Palette.Children.Add(this.CreatePaletteEntry(Colors.Transparent));
			}
		}

		private async void CommitSettings(FLib.UI.Controls.SettingsPanel sender, object args)
		{
			Logger.Debug("Commiting board settings changes.");
			await UIManager.ShowLoading();

			var parts = this.BoardSizeTextBox.Text.Split('x');
			int w, h;
			if (parts.Length == 2 && int.TryParse(parts[0], out w) && int.TryParse(parts[1], out h) &&
				(this.Board.Size.Width != w || this.Board.Size.Height != h))
			{
				Logger.Info("Changing board size to {0}x{1}.", w, h);
				this.Board.Size = new Core.Size(w, h);
				this.MainGame.ReloadBoard();
			}

			Logger.Trace("Reloading palette.");
			this.Game.Palette =
				this.Palette.Children
				.Select(c => ((SolidColorBrush)((Control)c).Background).Color)
				.Where(c => c.A == 255)
				.Distinct()
				.ToArray();

			UIManager.HideLoadingPopup();
		}

		private void OpenColorPicker(object sender, RoutedEventArgs e)
		{
			var btn = (Button)sender;
			this.ColorPicker.Open((Controls.ColorPicker.PaletteEntry)btn.Tag, btn);
		}

		private Button CreatePaletteEntry(Color color)
		{
			var btn = new Button();
			var bg = new SolidColorBrush(color);
			btn.Background = bg;
			btn.Tag = new Controls.ColorPicker.PaletteEntry(bg);
			btn.Click += this.OpenColorPicker;
			return btn;
		}
	}
}
