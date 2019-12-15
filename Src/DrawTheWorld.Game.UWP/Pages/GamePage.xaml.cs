using System;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Game.Helpers;
using FLib;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace DrawTheWorld.Game.Pages
{
	/// <summary>
	/// Page that displays UI for game.
	/// </summary>
	sealed partial class GamePage
		: Utilities.UIPage
	{
		private const string ResourceKey = "Pages_GamePage_";
		private static readonly Size MaxBoardThumbnailSize = new Size(500, 500);

		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.GamePage");

		private readonly IGameManager GameManager = null;

		private readonly ZoomHandler Zoom = null;

		private Core.IGame Game = null;

		private double TopSuccessUIHeight = -1;

		/// <summary>
		/// This is not size of the full UI, but "bottom part" times two. This enforces correct layout.
		/// Same goes for <see cref="FailureUIHeight"/>
		/// </summary>
		private double SuccessUIHeight = 0;

		private double TopFailureUIHeight = -1;
		private double FailureUIHeight = 0;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public GamePage(IGameManager gameManager, IUIManager uiManager, UISettings uiSettings)
		{
			this.GameManager = gameManager;

			this.InitializeComponent();

			this.Zoom = new ZoomHandler(uiManager, uiSettings, this.ZoomButton, this.BottomAppBar, this.MainGame);
		}

		/// <summary>
		/// Shows UI for finished game.
		/// </summary>
		public Task ShowFinishUI()
		{
			Task task = this.Game.FinishReason == Core.FinishReason.Correct ? this.ShowSuccessUI() : this.ShowFailureUI();
			return task;
		}

		/// <summary>
		/// Reloads the display and restores app bars.
		/// </summary>
		protected internal override void OnNavigatedTo(Type sourcePageType)
		{
			Logger.Trace("Navigated to the GamePage, reloading board.");

			this.Zoom.Reset();
			this.Game = this.GameManager.Game;

			if (this.TopSuccessUIHeight > -1)
				this.SuccessUI.Visibility = Visibility.Collapsed;
			if (this.TopFailureUIHeight > -1)
				this.FailureUI.Visibility = Visibility.Collapsed;

			this.MainGame.Reload();
            this.BottomAppBar.Visibility = Visibility.Visible;
		}

		protected internal override void OnNavigatedFrom(Type destinationPageType)
		{
			Logger.Trace("Navigating from the GamePage.");

			this.Game = null;
		}

		protected internal override void OnViewStateChanged(Windows.UI.ViewManagement.ApplicationViewState state)
		{
			if (this.Game.FinishReason != Core.FinishReason.NotFinished)
				(this.Game.FinishReason == Core.FinishReason.Correct ? this.SuccessUI : this.FailureUI).Visibility = state == Windows.UI.ViewManagement.ApplicationViewState.Snapped ? Visibility.Collapsed : Visibility.Visible;
		}

		private Task ShowSuccessUI()
		{
			Logger.Trace("Showing success UI.");

			// Align board
			var used = this.MainGame.DisableAndMinimize(GetThumbnailSize(this.SuccessUIHeight));
			this.BoardSpaceInSuccessUI.Height = new GridLength(used.Height);
			this.SuccessUI.Margin = new Thickness(0, (Window.Current.Bounds.Height - used.Height) / 2 - this.TopSuccessUIHeight, 0, 0);

			this.TimeText.Text = this.FormatGameTime();
			this.BoardNameText.Text = Strings.Get(ResourceKey + "BoardTitle").FormatWith(this.Game.Board.BoardInfo.MainName);

			return this.ShowUIAndWaitForClick(this.SuccessUI, this.SuccessUIOKButton);
		}

		private Task ShowFailureUI()
		{
			Logger.Trace("Showing failure UI.");

			// Align board
			var used = this.MainGame.DisableAndMinimize(GetThumbnailSize(this.FailureUIHeight));
			this.BoardSpaceInFailureUI.Height = new GridLength(used.Height);
			this.FailureUI.Margin = new Thickness(0, (Window.Current.Bounds.Height - used.Height) / 2 - this.TopFailureUIHeight, 0, 0);

			switch (this.Game.FinishReason)
			{
				case Core.FinishReason.TooMuchFine:
					this.FailureReason.Text = Strings.Get(ResourceKey + "FineLimit");
					break;
				case Core.FinishReason.User:
					this.FailureReason.Text = Strings.Get(ResourceKey + "UserAction");
					break;
			}

			return this.ShowUIAndWaitForClick(this.FailureUI, this.FailureUIOKButton);
		}

		private void FinishGame(object sender, RoutedEventArgs e)
		{
			Logger.Debug("User wants to end the game, finishing.");
			this.BottomAppBar.IsOpen = false;
            this.BottomAppBar.Visibility = Visibility.Collapsed;
			this.Game.Finish();
		}

		private void CalculateSuccessUISize(object sender, SizeChangedEventArgs e)
		{
			this.SuccessUI.SizeChanged -= this.CalculateSuccessUISize;
			this.SuccessUI.Visibility = Visibility.Collapsed;

			this.TopSuccessUIHeight = this.SuccessUI.RowDefinitions.TakeWhile(r => r != this.BoardSpaceInSuccessUI).Sum(r => r.ActualHeight);
			this.SuccessUIHeight = (e.NewSize.Height - this.TopSuccessUIHeight) * 2;

			this.SuccessUI.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.SuccessUI.VerticalAlignment = VerticalAlignment.Stretch;
		}

		private void CalculateFailureUISize(object sender, SizeChangedEventArgs e)
		{
			this.FailureUI.SizeChanged -= this.CalculateFailureUISize;
			this.FailureUI.Visibility = Visibility.Collapsed;

			this.TopFailureUIHeight = this.FailureUI.RowDefinitions.TakeWhile(r => r != this.BoardSpaceInFailureUI).Sum(r => r.ActualHeight);
			this.FailureUIHeight = (e.NewSize.Height - this.TopFailureUIHeight) * 2;

			this.FailureUI.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.FailureUI.VerticalAlignment = VerticalAlignment.Stretch;
		}

		private string FormatGameTime()
		{
			string timeString = string.Empty;
			var time = (this.Game.FinishDate - this.Game.StartDate).Add(TimeSpan.FromMinutes(this.Game.Fine));
			if (time.Hours > 0)
				timeString += time.Hours + "h ";
			timeString += time.Minutes.ToString(time.Hours > 0 ? "00" : "#0") + "m " + time.Seconds.ToString("00") + "s";
			return timeString;
		}

		private static Size GetThumbnailSize(double heightOffset)
		{
			return new Size(
				Math.Min(Window.Current.Bounds.Width, MaxBoardThumbnailSize.Width),
				Math.Min(Window.Current.Bounds.Height - heightOffset, MaxBoardThumbnailSize.Height)
			);
		}

		private async Task ShowUIAndWaitForClick(FrameworkElement ui, Button button)
		{
			ui.Visibility = Visibility.Visible;
			Storyboard.SetTarget(this.ShowFinishUIAnimation.Children[0], ui);
			this.ShowFinishUIAnimation.Begin();
			await WaitFor.RoutedEvent(h => button.Click += h, h => button.Click -= h);
			this.ShowFinishUIAnimation.Stop();
			ui.Visibility = Visibility.Collapsed;
		}
	}
}
