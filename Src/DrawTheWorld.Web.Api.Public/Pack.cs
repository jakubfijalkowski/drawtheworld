using System;

namespace DrawTheWorld.Web.Api.Public
{
	/// <summary>
	/// Describes pack.
	/// </summary>
	/// <remarks>
	/// <see cref="Name"/> and <see cref="Description"/> contains only translated version that fits the language best.
	/// </remarks>
	public sealed class Pack
	{
		/// <summary>
		/// Gets or sets the id of the pack.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets the translated name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the translated description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the author.
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// Gets or sets the author's address(prefferably homepage).
		/// </summary>
		public string AuthorAddress { get; set; }

		/// <summary>
		/// Gets or sets the date when the pack was added.
		/// </summary>
		public DateTime DateAdded { get; set; }

		/// <summary>
		/// Gets or sets the number of boards inside the pack.
		/// </summary>
		public int BoardsNumber { get; set; }

		/// <summary>
		/// Gets or sets the pack price.
		/// </summary>
		public int Price { get; set; }
	}
}
