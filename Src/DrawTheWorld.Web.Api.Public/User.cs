
namespace DrawTheWorld.Web.Api.Public
{
	/// <summary>
	/// Represents information about the user.
	/// </summary>
	public sealed class User
	{
		/// <summary>
		/// Gets or sets the first name of the user.
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the last name of the user.
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		/// Gets or sets the address(url) of the user.
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// Gets or sets the amount of coins the user has.
		/// </summary>
		public long Coins { get; set; }
	}
}
