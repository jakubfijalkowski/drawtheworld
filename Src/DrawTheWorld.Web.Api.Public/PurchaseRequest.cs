namespace DrawTheWorld.Web.Api.Public
{
	/// <summary>
	/// Supported stores.
	/// </summary>
	public enum Store
		: int
	{
		WindowsStore = 1
	}

	/// <summary>
	/// Supported product types.
	/// </summary>
	public enum ProductType
		: int
	{
		/// <summary>
		/// Full application.
		/// Emitted only by Windows Store.
		/// </summary>
		FullApplication,

		/// <summary>
		/// Coin pack.
		/// </summary>
		CoinPack
	}


	/// <summary>
	/// Request of the purchase.
	/// </summary>
	public sealed class PurchaseRequest
	{
		/// <summary>
		/// Gets or sets the store which the purchase was made from.
		/// </summary>
		public Store Store { get; set; }

		/// <summary>
		/// Gets or sets the product which user wants to buy.
		/// </summary>
		public ProductType ProductType { get; set; }

		/// <summary>
		/// Gets or sets the tier of the product.
		/// </summary>
		public int Tier { get; set; }

		/// <summary>
		/// Gets or sets the receipt given by the store.
		/// </summary>
		public string Receipt { get; set; }
	}
}
