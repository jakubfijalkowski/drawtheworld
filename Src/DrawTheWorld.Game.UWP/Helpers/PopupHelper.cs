using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace DrawTheWorld.Game.Helpers
{
	static class PopupHelper
	{
		private const string ResourceKey = "Helpers_Popup_";

		/// <summary>
		/// Shows simple message dialog.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static Task ShowMessageDialog(string title, string message)
		{
			var dialog = new MessageDialog(message, title);
			dialog.Commands.Add(new UICommand(Strings.Get(ResourceKey + "OkButton")));
			dialog.CancelCommandIndex = dialog.DefaultCommandIndex = 0;

			return dialog.ShowAsync().AsTask();
		}

		/// <summary>
		/// Shows yes/no dialog.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="noText"></param>
		/// <param name="yesText"></param>
		/// <returns></returns>
		public static async Task<bool> ShowYesNoDialog(string title, string message, string yesText = null, string noText = null)
		{
			var dialog = new MessageDialog(message, title);
			var yesButton = new UICommand(yesText ?? Strings.Get(ResourceKey + "YesButton"));
			var noButton = new UICommand(noText ?? Strings.Get(ResourceKey + "NoButton"));
			dialog.Commands.Add(yesButton);
			dialog.Commands.Add(noButton);

			dialog.CancelCommandIndex = 1;
			dialog.DefaultCommandIndex = 0;

			return await dialog.ShowAsync() == yesButton;
		}
	}
}
