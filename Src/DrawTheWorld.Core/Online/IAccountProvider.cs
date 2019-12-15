using System.Threading.Tasks;

namespace DrawTheWorld.Core.Online
{
	/// <summary>
	/// Provides user account(using one of the providers).
	/// </summary>
	public interface IAccountProvider
	{
		/// <summary>
		/// If true, user can be signed out.
		/// </summary>
		bool CanSignOut { get; }

		/// <summary>
		/// Gets tht type of the token of signed user.
		/// </summary>
		string AuthTokenType { get; }

		/// <summary>
		/// Gets the auth token of signed user.
		/// </summary>
		string AuthToken { get; }

		/// <summary>
		/// Gets the user id, returned by external provider.
		/// </summary>
		string UserId { get; }

		/// <summary>
		/// Tries to sign user in.
		/// </summary>
		/// <param name="silent">If true, user should not be prompted for credentials - they should be restored from backing store.</param>
		Task<bool> SignIn(bool silent);

		/// <summary>
		/// Signs user out.
		/// </summary>
		Task SignOut();

		/// <summary>
		/// Gets information about the user for registeration purposes.
		/// </summary>
		/// <returns></returns>
		Task<Web.Api.Public.User> GetUserInformation();

		/// <summary>
		/// Forcibly resets user information when signing in proces fails.
		/// </summary>
		void Reset();
	}
}
