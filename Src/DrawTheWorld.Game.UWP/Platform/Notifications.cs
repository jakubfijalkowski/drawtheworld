using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Core.UI.Messages;
using DrawTheWorld.Game.Helpers;
using FLib;
using Windows.UI.Core;
using Windows.UI.Notifications;

namespace DrawTheWorld.Game.Platform
{
	sealed class Notifications
		: Core.UI.Notifications
	{
		internal const string ResourceKey = "Platform_Notifications_";

		private readonly UIManager UIManager = null;
		private readonly Lazy<Controls.RequireSignIn> RequireSignInControl = null;

		private Windows.UI.Core.CoreDispatcher Dispatcher = null;

		public Notifications(UIManager uiManager, Lazy<Controls.RequireSignIn> requireSignIn)
		{
			this.UIManager = uiManager;
			this.RequireSignInControl = requireSignIn;
		}

		/// <summary>
		/// Fully initializes the notification subsystem.
		/// </summary>
		public void Initialize()
		{
			this.Dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
		}

		protected override async Task NotEnoughMoneyHandler(IMessage message)
		{
			var msg = (NotEnoughMoneyMessage)message;
			msg.Result = await this.UIManager.ShowCoinsUI(true);
		}

		protected override Task CannotAccessApiHandler(IMessage message)
		{
			return this.ShowDialog(message);
		}

		/// <summary>
		/// Warning: there must be at least 2 strings for both 'normal' and 'WithErrors' message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		protected override Task PacksSynchronizedHandler(IMessage message)
		{
			var msg = (PacksSynchronizedMessage)message;

			var texts = new List<string>(3);
			string key = msg.GetKey() + (msg.Failures > 0 ? "WithErrors" : string.Empty);
			texts.Add(LanguageHelper.GetNoun(msg.Successes, key + "1").FormatWith(msg.Successes, msg.Failures));
			if (msg.Failures > 0)
				texts.Add(LanguageHelper.GetNoun(msg.Failures, key + "2").FormatWith(msg.Successes, msg.Failures));
			else
				texts.Add(LanguageHelper.GetNoun(msg.Successes, key + "2").FormatWith(msg.Successes, msg.Failures));
			var third = Strings.Get(key + "3");
			if (!string.IsNullOrEmpty(third))
				texts.Add(LanguageHelper.GetNoun(msg.Successes, key + "3").FormatWith(msg.Successes, msg.Failures));

			this.ShowNotification(texts, () => this.UIManager.NavigateTo<Pages.GameList>());
			return null;
		}

		protected override Task PackPurchasedHandler(IMessage message)
		{
			var msg = (PackPurchasedMessage)message;
			var texts = message.GetMultipleMessages(args: msg.Pack.Name.MainTranslation);
			this.ShowNotification(texts, () => this.UIManager.NavigateTo<Pages.GameList>().ShowPack(msg.Pack.Id));
			return null;
		}

		protected override Task PackDataErrorHandler(IMessage message)
		{
			return this.ShowDialog(message);
		}

		protected override Task PackLoadErrorHandler(IMessage message)
		{
			var msg = (PackLoadErrorMessage)message;
			return PopupHelper.ShowMessageDialog(message.GetSingleMessage("Title"), message.GetSingleMessage(msg.Type.ToString()));
		}

		protected override Task PackSaveErrorHandler(IMessage message)
		{
			var msg = (PackSaveErrorMessage)message;
			this.ShowNotification(message.GetMultipleMessages(args: msg.Pack.Name.MainTranslation));
			return null;
		}

		protected override Task StorageProblemHandler(IMessage message)
		{
			return this.ShowDialog(message);
		}

		protected override async Task SignInRequiredHandler(IMessage message)
		{
			this.RequireSignInControl.Value.Requirement = Controls.RequirementType.Action;
			await this.UIManager.ShowMessageDialog(this.RequireSignInControl.Value);
			((SignInRequiredMessage)message).Result = this.RequireSignInControl.Value.Result;
		}

		protected override Task ErrorHandler(IMessage message)
		{
			return this.ShowDialog(message);
		}

		protected override Task BoardImportErrorHandler(IMessage message)
		{
			var msg = (BoardImportErrorMessage)message;
			return this.ShowDialog(message, msg.ErrorCount);
		}

		protected override Task PackNotValidHandler(IMessage message)
		{
			var msg = (PackNotValidMessage)message;
			return this.ShowDialog(message, msg.Pack.Name.MainTranslation);
		}

		protected override Task PackIsEmptyHandler(IMessage message)
		{
			return this.ShowDialog(message);
		}

		private void ShowNotification(List<string> texts, DispatchedHandler handler = null)
		{
			var notifier = ToastNotificationManager.CreateToastNotifier();
			if (notifier.Setting == NotificationSetting.Enabled)
			{
				var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText04);
				var el = template.GetElementsByTagName("text");
				for (int i = 0; i < Math.Min(texts.Count, el.Length); i++)
					el[i].AppendChild(template.CreateTextNode(texts[i]));

				var toast = new ToastNotification(template);
				if (handler != null)
					toast.Activated += (s, e) => { var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, handler); };

				notifier.Show(toast);
			}
		}

		private Task<bool> ShowAskDialog(IMessage message)
		{
			return PopupHelper.ShowYesNoDialog(message.GetSingleMessage("Title"), message.GetSingleMessage());
		}

		private Task ShowDialog(IMessage message, params object[] args)
		{
			return PopupHelper.ShowMessageDialog(message.GetSingleMessage("Title"), message.GetSingleMessage(args: args));
		}
	}

	static class MessageExtensions
	{
		public static string GetKey(this IMessage message)
		{
			var key = message.GetType().Name;
			return Notifications.ResourceKey + key.Remove(key.Length - 7);
		}

		public static string GetSingleMessage(this IMessage message, string modifier, params object[] args)
		{
			return Strings.Get(message.GetKey() + modifier).FormatWith(args);
		}

		public static string GetSingleMessage(this IMessage message, params object[] args)
		{
			return GetSingleMessage(message, string.Empty, args);
		}

		public static List<string> GetMultipleMessages(this IMessage message, string modifier, params object[] args)
		{
			var baseKey = message.GetKey() + modifier;

			var ret = new List<string>(3);
			int i = 1;
			while (true)
			{
				var str = Strings.Get(baseKey + i);
				if (string.IsNullOrEmpty(str))
					break;
				ret.Add(str.FormatWith(args));
				++i;
			}
			return ret;
		}

		public static List<string> GetMultipleMessages(this IMessage message, params object[] args)
		{
			return GetMultipleMessages(message, string.Empty, args);
		}
	}
}
