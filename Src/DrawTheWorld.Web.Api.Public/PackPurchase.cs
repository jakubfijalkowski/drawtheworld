using System;

namespace DrawTheWorld.Web.Api.Public
{
	/// <summary>
	/// Represents information about the pack purchase.
	/// </summary>
	public sealed class PackPurchase
	{
		/// <summary>
		/// Gets or sets the id of the pack that was purchased.
		/// </summary>
		public Guid PackId { get; set; }

		/// <summary>
		/// Gets or sets the date the purchase was made.
		/// </summary>
		public DateTime PurchaseDate { get; set; }
	}
}
