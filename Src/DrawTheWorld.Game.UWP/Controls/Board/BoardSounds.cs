using DrawTheWorld.Core;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using FLib;

namespace DrawTheWorld.Game.Controls.Board
{
	static class BoardSounds
	{
		private static readonly ISoundManager SoundManager = App.Current.SoundManager;

		/// <summary>
		/// Decides what sound should be played and plays it.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="tool"></param>
		/// <param name="isFine"></param>
		public static void PlaySoundOnAction(GameData data, ToolData tool, bool isFine)
		{
			Validate.Debug(() => data, v => v.NotNull());
			Validate.Debug(() => tool, v => v.NotNull());

			if (tool == data.GetTool(PredefinedTool.Brush))
				SoundManager.PlaySound(Sound.FieldFilled);
			else if (tool == data.GetTool(PredefinedTool.Eraser))
				SoundManager.PlaySound(Sound.FieldErased);
			else
				SoundManager.PlaySound(Sound.OtherFieldAction);

			if (isFine)
				SoundManager.PlaySound(Sound.FineCharged);
		}
	}
}
