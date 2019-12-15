using System;
using DrawTheWorld.Core.Platform;
using FLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls.Board
{
	/// <summary>
	/// Displays timer of the board.
	/// </summary>
	sealed partial class Timer
		: Grid
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("UI.Board.Timer");

		private static readonly UISettings UISettings = App.Current.UISettings;
		private static readonly IGameManager GameManager = App.Current.GameManager;

		private readonly DispatcherTimer Clock = null;
		private TextBlockWithShadow _SnappedTimer = null;

		public TextBlockWithShadow SnappedTimer
		{
			get { return this._SnappedTimer; }
			set
			{
				Validate.Debug(() => value, v => v.NotNull());
				Validate.Debug(() => this._SnappedTimer, v => v.Null());
				this._SnappedTimer = value;
			}
		}

		/// <summary>
		/// Initializes the timer.
		/// </summary>
		public Timer()
		{
			this.InitializeComponent();

			this.Clock = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(0.2)
			};
			this.Clock.Tick += this.UpdateTimerText;

			this.Unloaded += (s, e) => this.Clock.Stop();
			UISettings.SettingsUpdated += this.UpdateSizes;
		}

		/// <summary>
		/// Resets the timer and adjusts its visibility to the current game(hidden in designer).
		/// </summary>
		/// <param name="forceHideTimer">Forcibly hides timer. Used only in TutorialBoard.</param>
		public void Reset(bool forceHideTimer = false)
		{
			Logger.Info("Resetting timer.");
			this.Clock.Stop();

			if (forceHideTimer || GameManager.Game is Core.Designer)
			{
				Logger.Trace("In designed, hiding timer.");
				this.Visibility = Visibility.Collapsed;
				this.SnappedTimer.Visibility = Visibility.Collapsed;
			}
			else
			{
				Logger.Trace("In game, showing timer.");
				this.Visibility = Visibility.Visible;
				this.SnappedTimer.Visibility = Visibility.Visible;
				this.Clock.Start();
			}
		}

		/// <summary>
		/// Shows fine alert above timer.
		/// </summary>
		/// <param name="fine"></param>
		public void ShowFineAlert(int fine)
		{
			Logger.Trace("Showing fine alert.");
			this.FineAlertText.Text = "+{0}m".FormatWith(fine);
			this.ShowFineAlertAnimation.Begin();
		}

		/// <summary>
		/// Disables the timer(use <see cref="Reset"/> to reenable).
		/// </summary>
		public void Disable()
		{
			Logger.Info("Disablind timer.");
			this.Clock.Stop();
		}

		private void UpdateTimerText(object sender, object e)
		{
			var diff = (DateTime.UtcNow - GameManager.Game.StartDate).Add(TimeSpan.FromMinutes(GameManager.Game.Fine));
			string text = string.Empty;
			if (diff.Hours > 0)
				text += diff.Hours + "h";
			if (diff.Minutes > 0)
				text += diff.Minutes + "m";
			text += diff.Seconds + "s";

			this.TimeCounter.Text = text;
			this.SnappedTimer.Text = text;
		}

		private void UpdateSizes(object sender, object e)
		{
			this.TimeCounter.FontSize = (double)this.Resources["DefaultTimerTextFontSize"] * UISettings.Factor;
			this.FineAlertText.FontSize = (double)this.Resources["DefaultFineAlertTextStyle"] * UISettings.Factor;
		}
	}
}
