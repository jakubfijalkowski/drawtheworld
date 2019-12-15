using System;
using System.Threading.Tasks;
using DrawTheWorld.Core.Online;
using FLib;
//using Microsoft.Live;
using Windows.Security.Authentication.OnlineId;

namespace DrawTheWorld.Game.Platform
{
	/// <summary>
	/// <see cref="IAccountProvider"/> for Live Connect.
	/// </summary>
	sealed class LiveAccountProvider
		: IAccountProvider
	{
		private static readonly string[] Scopes = new string[] { "wl.signin" };

		//private readonly LiveAuthClient AuthClient = new LiveAuthClient("http://dtw.fiolek.org");

		//private LiveConnectSession Session = null;

		/// <inheritdoc />
		public string AuthTokenType
		{
			get { return "live"; }
		}

		/// <inheritdoc />
		public string AuthToken
		{
			get
			{
                //Validate.Debug(() => this.Session, v => v.NotNull());
                //return this.Session.AuthenticationToken;
                return "test-token";
			}
		}

		/// <inheritdoc />
		public string UserId { get; private set; } = "test-user";

		/// <inheritdoc />
		public bool CanSignOut
		{
			get { return false; }
		}

		/// <inheritdoc />
		public Task<bool> SignIn(bool silent)
		{
			//if (this.Session != null)
			//	return true;

			//var result = silent ? await this.AuthClient.InitializeAsync(Scopes) : await this.AuthClient.LoginAsync(Scopes);
			//if (result != null && result.Status == LiveConnectSessionStatus.Connected)
			//{
			//	this.Session = result.Session;
			//	var authenticator = new OnlineIdAuthenticator();
			//	if (!string.IsNullOrWhiteSpace(authenticator.AuthenticatedSafeCustomerId))
			//	{
			//		this.UserId = authenticator.AuthenticatedSafeCustomerId;
			//	}
			//	else
			//	{
			//		var auth = await authenticator.AuthenticateUserAsync(new OnlineIdServiceTicketRequest(string.Join(" ", Scopes), "DELEGATION"));
			//		this.UserId = auth.SafeCustomerId;
			//	}
			//	return true;
			//}
			//return false;
            return Task.FromResult(true);
		}

		/// <inheritdoc />
		public Task SignOut()
		{
			//Validate.Debug(() => this.CanSignOut, v => v.True());
			//Validate.Debug(() => this.Session, v => v.NotNull());

			//this.AuthClient.Logout();

			//this.Session = null;
			//this.UserId = null;

			return Task.FromResult(0);
		}

		/// <inheritdoc />
		public Task<Web.Api.Public.User> GetUserInformation()
		{
			//var liveClient = new LiveConnectClient(this.Session);
			//var result = await liveClient.GetAsync("me");
			//dynamic me = result.Result;
			return Task.FromResult(new Web.Api.Public.User
			{
				FirstName = "Have",
				LastName = "Fun"
			});
		}

		/// <inheritdoc />
		public void Reset()
		{
			//if (this.AuthClient.CanLogout)
			//	this.AuthClient.Logout();
			//this.Session = null;
		}
	}
}
