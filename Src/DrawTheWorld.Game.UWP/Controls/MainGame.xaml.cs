using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Combines game-related controls and manages its layout.
	/// </summary>
	sealed partial class MainGame
		: Grid
	{
		private double ToolboxesSize = 0;
		private ApplicationViewState LastState = ApplicationViewState.FullScreenPortrait;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public MainGame()
		{
			this.InitializeComponent();

			this.Toolbox.BoardDisplay = this.BoardDisplay;

			this.Toolbox.SizeChanged += this.UpdateToolboxSize;
			this.Loaded += (s, e) => Window.Current.SizeChanged += this.OnWindowSizeChanged;
			this.Unloaded += (s, e) => Window.Current.SizeChanged -= this.OnWindowSizeChanged;
		}

		/// <summary>
		/// Reloads the content.
		/// </summary>
		public void Reload()
		{
			this.MinimizeUI.Stop();

			this.Toolbox.ReloadToolbox();
			this.BoardDisplay.ReloadBoard();
			this.ColorPicker.ReloadPicker();

			if (ApplicationView.Value != this.LastState)
				this.UpdateOrientation(ApplicationView.Value);
		}

		/// <summary>
		/// Reloads only board.
		/// </summary>
		public void ReloadBoard()
		{
			this.MinimizeUI.Stop();
			this.BoardDisplay.ReloadBoard();
		}

		/// <summary>
		/// Disables the UI and minimizes it.
		/// </summary>
		/// <param name="availableSize">The size that can be used by the game(from the center of the screen).</param>
		/// <returns>Size that is really used.</returns>
		public Size DisableAndMinimize(Size availableSize)
		{
			this.MinimizeUI.Begin();
			return this.BoardDisplay.DisableAndMinimize(availableSize);
		}

		private void UpdateOrientation(ApplicationViewState state)
		{
			if (this.ToolboxesSize < 1)
				return;
			this.ColumnDefinitions.Clear();
			this.RowDefinitions.Clear();

			switch (state)
			{
				case ApplicationViewState.FullScreenLandscape:
				case ApplicationViewState.Filled:

					this.Toolbox.Visibility = Visibility.Visible;
					this.ColorPicker.Visibility = Visibility.Visible;
					this.BoardDisplay.IsSnapped = false;

					this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ToolboxesSize) });
					this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
					this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ToolboxesSize) });

					this.Toolbox.Orientation = Orientation.Vertical;
					this.ColorPicker.Orientation = Orientation.Vertical;

					break;

				case ApplicationViewState.FullScreenPortrait:

					this.Toolbox.Visibility = Visibility.Visible;
					this.ColorPicker.Visibility = Visibility.Visible;
					this.BoardDisplay.IsSnapped = false;

					this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(ToolboxesSize) });
					this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
					this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(ToolboxesSize) });

					this.Toolbox.Orientation = Orientation.Horizontal;
					this.ColorPicker.Orientation = Orientation.Horizontal;

					break;

				case ApplicationViewState.Snapped:

					this.Toolbox.Visibility = Visibility.Collapsed;
					this.ColorPicker.Visibility = Visibility.Collapsed;
					this.BoardDisplay.IsSnapped = true;

					break;
			}

			this.LastState = state;
		}

		private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
		{
			this.UpdateOrientation(ApplicationView.Value);
		}

		private void UpdateToolboxSize(object sender, SizeChangedEventArgs e)
		{
			if (System.Math.Abs(e.NewSize.Width - this.ToolboxesSize) > 0.1)
			{
				this.ToolboxesSize = ApplicationView.Value != ApplicationViewState.FullScreenPortrait ? e.NewSize.Width : e.NewSize.Height;
				this.UpdateOrientation(ApplicationView.Value);
			}
		}
	}
}
