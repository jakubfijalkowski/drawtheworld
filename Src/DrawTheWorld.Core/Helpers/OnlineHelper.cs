using System;
using System.Threading;
using System.Threading.Tasks;
using DrawTheWorld.Core.Online;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Web.Api.Public;
using FLib;

namespace DrawTheWorld.Core.Helpers
{
	/// <summary>
	/// Helpers for accessing online service.
	/// </summary>
	public static class OnlineHelper
	{
		/// <summary>
		/// Download, with retries.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="client"></param>
		/// <param name="method"></param>
		/// <param name="errorAction"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<Tuple<bool, T>> Download<T>(this Client client, Func<Client, Task<T>> method, Action<Exception> errorAction, CancellationToken cancellationToken)
		{
			Validate.Debug(() => client, v => v.NotNull());
			Validate.Debug(() => method, v => v.NotNull());

			int retries = 0;
			T down = default(T);
			bool failure = true;
			while (failure && retries < Config.RetryCount && !cancellationToken.IsCancellationRequested)
			{
				if (retries > 0)
					await Task.Delay(Config.RetryDelay);

				try
				{
					down = await method(client);
					failure = false;
				}
				catch (Exception ex)
				{
					if (errorAction != null)
						errorAction(ex);
				}

				retries++;
			}
			return Tuple.Create(failure, down);
		}

		/// <summary>
		/// Download, with retries.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="client"></param>
		/// <param name="method"></param>
		/// <param name="errorAction"></param>
		/// <returns></returns>
		public static Task<Tuple<bool, T>> Download<T>(this Client client, Func<Client, Task<T>> method, Action<Exception> errorAction)
		{
			return Download(client, method, errorAction, CancellationToken.None);
		}

		/// <summary>
		/// Ensures that the user is signed in and if not - tries to sing user in.
		/// </summary>
		/// <param name="account"></param>
		/// <param name="notifications"></param>
		/// <returns></returns>
		public static async Task<SignOutBlockade> EnsureSignedIn(this AccountManager account, Notifications notifications)
		{
			SignOutBlockade blockade;
			do
			{
				blockade = await account.BlockSignOut();
				if (blockade == null)
				{
					var signInMsg = new Core.UI.Messages.SignInRequiredMessage();
					await notifications.NotifyAsync(signInMsg);
					if (!signInMsg.Result)
						break;
				}
			} while (blockade == null);
			return blockade;
		}
	}
}
