using System;
using DrawTheWorld.Core.Online;
using DrawTheWorld.Core.Platform;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Wires settings and some events.
	/// </summary>
	sealed class GlobalSettingsManager
	{
		private readonly AccountManager Account;
		private readonly ISettingsProvider Settings;

		public GlobalSettingsManager(AccountManager account, ISettingsProvider settings)
		{
			this.Account = account;
			this.Settings = settings;

			this.Account.UserSignedIn += this.OnUserSignedIn;
			this.Account.UserSignedOut += this.OnUserSignedOut;
		}

		private void OnUserSignedIn(AccountManager account, User user)
		{
			this.Settings[SettingsState.UserSignedIn] = true;
		}

		private void OnUserSignedOut(AccountManager account, User user)
		{
			this.Settings[SettingsState.UserSignedIn] = false;
		}
	}
}
