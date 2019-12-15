using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// Available features.
	/// </summary>
	public enum Feature
	{
		Designer,
		PackInstall
	}

	/// <summary>
	/// Available products.
	/// </summary>
	public enum Product
	{
		/// <summary>
		/// Full app. User should not be able to buy it, but if he/she had bought it before v2, receipt should be returned.
		/// </summary>
		FullApp,

		/// <summary>
		/// The pack of coins. See <see cref="IFeatureProvider.CoinTiers"/> for more info about the tiers.
		/// </summary>
		CoinPack
	}

	/// <summary>
	/// The exception that is thrown when no more iAP products are available.
	/// </summary>
	public sealed class NoMoreConsumablesException
		: Exception
	{
		public NoMoreConsumablesException() { }
		public NoMoreConsumablesException(string message) : base(message) { }
		public NoMoreConsumablesException(string message, Exception inner) : base(message, inner) { }
	}

	/// <summary>
	/// Describes coin pack tier.
	/// </summary>
	public sealed class CoinTierDescription
	{
		/// <summary>
		/// Gets the tier id.
		/// </summary>
		public int Tier { get; private set; }

		/// <summary>
		/// Gets the name of the tier.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the coins amount.
		/// </summary>
		public int CoinsAmount { get; private set; }

		/// <summary>
		/// Gets the formatted price.
		/// </summary>
		public string Price { get; private set; }

		public CoinTierDescription(int tier, string name, int coins, string price)
		{
			this.Tier = tier;
			this.Name = name;
			this.CoinsAmount = coins;
			this.Price = price;
		}
	}

	/// <summary>
	/// Provides checks for features(eg. iAPs).
	/// </summary>
	public interface IFeatureProvider
	{
		/// <summary>
		/// Gets the description about tiers of coin packs.
		/// </summary>
		IReadOnlyList<CoinTierDescription> CoinTiers { get; }

		/// <summary>
		/// Requests particular <paramref name="feature"/>.
		/// </summary>
		/// <param name="feature"></param>
		/// <returns></returns>
		Task<bool> RequestFeature(Feature feature);

		/// <summary>
		/// Checks if particular <paramref name="feature"/> is enabled. Does not show purchase UI.
		/// </summary>
		/// <param name="feature"></param>
		/// <returns></returns>
		bool CheckFeature(Feature feature);

		/// <summary>
		/// Buys particular product.
		/// </summary>
		/// <param name="productType"></param>
		/// <param name="tier"></param>
		/// <returns></returns>
		Task<string> BuyProduct(Product productType, int tier);

		/// <summary>
		/// Saves the unfullfilled receipt in backing store for future use.
		/// </summary>
		/// <param name="product"></param>
		/// <param name="tier"></param>
		/// <param name="receipt"></param>
		void SaveUnfulfilledReceipt(Product productType, int tier, string receipt);
	}
}
