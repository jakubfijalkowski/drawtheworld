namespace DrawTheWorld.Web.Api.Public
{
	public static class Constraints
	{
		/// <summary>
		/// Gets or sets the maximum length of <see cref="User.FirstName"/> and <see cref="User.LastName"/>.
		/// </summary>
		public const int MaxNameLength = 128;

		/// <summary>
		/// The maximum length of links(eg. <see cref="Pack.AuthorAddress"/>).
		/// </summary>
		public const int MaxLinkLength = 512;
	}
}
