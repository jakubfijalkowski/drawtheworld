using System;
using DrawTheWorld.Core;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using FLib;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Plays sounds when game or game data changes.
	/// </summary>
	sealed class GameSounds
		: IDisposable
	{
		private static readonly ISoundManager SoundManager = App.Current.SoundManager;

		private IGame Game = null;
		private GameData Data = null;
		private ToolData BrushData = null;

		/// <summary>
		/// Registers necessary handlers.
		/// </summary>
		/// <param name="data"></param>
		public GameSounds(IGame game, GameData data)
		{
			Validate.Debug(() => game, v => v.NotNull());
			Validate.Debug(() => data, v => v.NotNull());

			this.Game = game;
			this.Data = data;
			this.BrushData = this.Data.GetTool(Core.PredefinedTool.Brush);

			if (!(game is Core.Designer))
				this.Game.GameFinished += this.OnGameFinished;
			this.Data.PropertyChanged += this.OnToolChanged;
			this.BrushData.PropertyChanged += this.OnBrushDataChanged;
		}

		/// <summary>
		/// Unregisters the handlers.
		/// </summary>
		public void Dispose()
		{
			this.Game.GameFinished -= this.OnGameFinished;
			this.Data.PropertyChanged -= this.OnToolChanged;
			this.BrushData.PropertyChanged -= this.OnBrushDataChanged;

			this.Game = null;
			this.Data = null;
			this.BrushData = null;
		}

		private void OnGameFinished(IGame game, FinishReason reason)
		{
			switch (reason)
			{
				case FinishReason.Correct:
					SoundManager.PlaySound(Sound.GameWon);
					//TODO: play other sound for "unlocked track" or sth else in next version(eg. when on super-hard mode)
					break;

				case FinishReason.TooMuchFine:
				case FinishReason.User:
					SoundManager.PlaySound(Sound.GameLost);
					break;
			}
		}

		private void OnToolChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.Data.NameOf(_ => _.SelectedTool))
				SoundManager.PlaySound(Sound.ToolChanged);
		}

		private void OnBrushDataChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.BrushData.NameOf(_ => _.Data))
				SoundManager.PlaySound(Sound.ToolChanged);
		}
	}
}
