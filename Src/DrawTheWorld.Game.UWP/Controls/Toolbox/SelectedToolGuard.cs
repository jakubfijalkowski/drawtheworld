using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.Tools;
using DrawTheWorld.Core.UI;
using FLib;
using Windows.UI.Xaml.Controls.Primitives;

namespace DrawTheWorld.Game.Controls.Toolbox
{
	/// <summary>
	/// Guards selected tool value.
	/// </summary>
	sealed class SelectedToolGuard
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("UI.Toolbox.Guard");
		private static readonly IGameManager GameManager = App.Current.GameManager;

		private readonly Selector Selector = null;
		private readonly Board.Display Display = null;

		private Core.IGame Game = null;
		private Core.UI.GameData GameData = null;

		/// <summary>
		/// Initializes the object
		/// </summary>
		/// <param name="display"></param>
		public SelectedToolGuard(Selector selector, Board.Display display)
		{
			Validate.Debug(() => selector, v => v.NotNull());
			Validate.Debug(() => display, v => v.NotNull());

			this.Selector = selector;
			this.Display = display;

			this.Selector.SelectionChanged += this.OnSelectedItemChanged;
		}

		/// <summary>
		/// Reloads the object for different game.
		/// </summary>
		public void Reload()
		{
			if (this.GameData != null)
				this.GameData.PropertyChanged -= this.OnGameDataPropertyChanged;

			this.Game = GameManager.Game;
			this.GameData = GameManager.GameData;

			this.Selector.ItemsSource = this.GameData.Tools;
			this.Selector.SelectedItem = this.GameData.SelectedTool;

			this.GameData.PropertyChanged += this.OnGameDataPropertyChanged;
		}

		private void OnSelectedItemChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
		{
			this.Selector.SelectedItem = this.Guard(this.Selector.SelectedItem as ToolData);
		}

		private void OnGameDataPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.GameData.NameOf(g => g.SelectedTool))
				this.Selector.SelectedItem = this.GameData.SelectedTool;
		}

		private ToolData Guard(ToolData newValue)
		{
			if (newValue != null && newValue.Tool.IsSingleAction)
			{
				Logger.Info("Single action tool selected, performing action.");
				this.Game.PerformAction(newValue.Tool, newValue.Data, null);

				if (newValue.Tool is EraseCounterTool)
					this.Display.ClearCounterLayer();
			}
			else
				this.GameData.SelectedTool = newValue;
			return this.GameData.SelectedTool;
		}
	}
}
