using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Game.Helpers;
using FLib;
using Windows.ApplicationModel.Store;

#if DEBUG || FORCE_APPSIMULATOR
using CurrentApp = Windows.ApplicationModel.Store.CurrentAppSimulator;
#else
using CurrentApp = Windows.ApplicationModel.Store.CurrentApp;
#endif

namespace DrawTheWorld.Game.Platform
{
	/// <summary>
	/// Default implementation of the <see cref="FeatureProvider"/>.
	/// </summary>
	sealed class FeatureProvider
		: IFeatureProvider
	{
		private const string FeatureReceiptKey = "Feature_";
		private const string CoinPackReceiptKey = "CoinPack";

		private const string ResourceKey = "Platform_FeatureProvider_";

		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.FeatureProvider");

		private ListingInformation ListingInfo = null;
		private readonly Dictionary<Product, Dictionary<int, ProductTierInformation>> Products = new Dictionary<Product, Dictionary<int, ProductTierInformation>>();

		/// <summary>
		/// Gets a value that indicates whether the user had purchased the game before v2.
		/// </summary>
		public bool IsOldFullVersion { get; private set; }

		/// <inheritdoc />
		public IReadOnlyList<CoinTierDescription> CoinTiers { get; private set; }

		/// <inheritdoc />
		public Task<bool> RequestFeature(Feature feature)
		{
            return Task.FromResult(true);
			//if (this.IsOldFullVersion)
			//	return Task.FromResult(true);

			//return this.RequestProduct(FeatureReceiptKey + feature.ToString(), "Feature");
		}

		/// <inheritdoc />
		public bool CheckFeature(Feature feature)
		{
            return true;
			//if (this.IsOldFullVersion)
			//	return true;

			//return CurrentApp.LicenseInformation.ProductLicenses[FeatureReceiptKey + feature.ToString()].IsActive;
		}

		/// <inheritdoc />
		public Task<string> BuyProduct(Product productType, int tier)
		{
			if (productType == Product.FullApp)
			{
				return this.IsOldFullVersion ? CurrentApp.GetAppReceiptAsync().AsTask() : Task.FromResult<string>(null);
			}

			var productInfo = this.Products[productType][tier];
			string receipt;
			if (productInfo.SavedReceipts.TryDequeue(out receipt))
				return Task.FromResult(receipt);

			string product = productInfo.Products.FirstOrDefault(p => !CurrentApp.LicenseInformation.ProductLicenses[p].IsActive);
			if (string.IsNullOrEmpty(product))
			{
				Logger.Error("We ran out of empty slots for product {0}.", product);
				throw new NoMoreConsumablesException();
			}

			return this.BuyProduct(product);
		}

		/// <inheritdoc />
		public void SaveUnfulfilledReceipt(Product productType, int tier, string receipt)
		{
			Validate.Debug(() => receipt, v => v.NotNull());
			this.Products[productType][tier].SavedReceipts.Enqueue(receipt);
		}

		/// <summary>
		/// Initializes the provider.
		/// </summary>
		/// <returns></returns>
		public async Task Initialize()
		{
			this.ListingInfo = await CurrentApp.LoadListingInformationAsync();

			var tiers = from li in this.ListingInfo.ProductListings
						where li.Key.StartsWith(CoinPackReceiptKey)
						let pi = ParseProductInformation(li.Key, CoinPackReceiptKey.Length)
						select new { ProductName = li.Key, Name = li.Value.Name, Tier = pi.Item1, Price = li.Value.FormattedPrice, Coins = pi.Item2 };
			var tiers2 = tiers.GroupBy(p => p.Tier).OrderBy(g => g.Key).ToList();

			this.Products.Add(Product.CoinPack, tiers2.ToDictionary(g => g.Key, g => new ProductTierInformation(g.Select(p => p.ProductName))));
			this.CoinTiers = tiers2.Select(g => new CoinTierDescription(g.Key, g.First().Name, g.First().Coins, g.First().Price)).ToList();

			this.IsOldFullVersion = await this.CheckForPaidVersion();
		}

		private async Task<bool> RequestProduct(string productId, string key)
		{
			if (CurrentApp.LicenseInformation.ProductLicenses[productId].IsActive)
				return true;
			if (await ShowAskDialog(key, null))
				return await this.BuyProduct(productId) != null;
			return false;
		}

		private Task<string> BuyProduct(string productId)
		{
			//string receipt = null;
			try
			{
				Logger.Trace("Trying to buy {0}.", productId);
				//receipt = await CurrentApp.RequestProductPurchaseAsync(productId, true);
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot finalize product(id: {0}) purchase.".FormatWith(productId), ex);
				throw;
			}
//#if DEBUG
			Logger.Debug("Debug receipt generated.");
            return Task.FromResult(GenerateProductReceipt(productId));
//#else
//			if (CurrentApp.LicenseInformation.ProductLicenses[productId].IsActive)
//			{
//				Logger.Info("User purchased product {0}.", productId);
//				return receipt;
//			}
//			Logger.Info("Purchase of product {0} failed.", productId);
//			return null;
//#endif
		}

		private static Task<bool> ShowAskDialog(string contentKey, string titleKey, params object[] args)
		{
			return PopupHelper.ShowYesNoDialog(
				Strings.Get(ResourceKey + (titleKey ?? "DialogTitle")), Strings.Get(ResourceKey + contentKey).FormatWith(args),
				Strings.Get(ResourceKey + "BuyButton"), Strings.Get(ResourceKey + "CancelButton"));
		}

		private static Tuple<int, int> ParseProductInformation(string str, int nameLen)
		{
			str = str.Substring(nameLen + 4);
			var numIdx = CountNumChars(str);
			var tier = int.Parse(str.Remove(numIdx));
			int amount = 0;

			if (str.Substring(numIdx).StartsWith("_Amount"))
			{
				str = str.Substring(numIdx + 7);
				numIdx = CountNumChars(str);
				amount = int.Parse(str.Remove(numIdx));
			}
			return Tuple.Create(tier, amount);
		}

		private static int CountNumChars(string str)
		{
			int numIdx = 0;
			while (char.IsNumber(str[numIdx]))
				++numIdx;
			return numIdx;
		}

//#if DEBUG
		private static string GenerateProductReceipt(string productId)
		{
			var date = DateTime.Now.ToString("s") + "Z";
			var expires = (DateTime.Now + TimeSpan.FromDays(1)).ToString("s") + "Z";
			return
@"<Receipt Version='1.0' ReceiptDate='" + date + @"' CertificateId='b809e47cd0110a4db043b3f73e83acd917fe1336' ReceiptDeviceId='" + Guid.NewGuid().ToString() + @"'>
	<ProductReceipt Id='" + Guid.NewGuid().ToString() + @"' ProductId='" + productId + @"' PurchaseDate='" + date + @"' ExpirationDate='" + expires + @"' ProductType='Durable' AppId='" + Windows.ApplicationModel.Package.Current.Id.FamilyName + @"' />
	<Signature xmlns='http://www.w3.org/2000/09/xmldsig#'>
		<SignedInfo>
			<CanonicalizationMethod Algorithm='http://www.w3.org/2001/10/xml-exc-c14n#' />
			<SignatureMethod Algorithm='http://www.w3.org/2001/04/xmldsig-more#rsa-sha256' />
			<Reference URI=''>
				<Transforms>
					<Transform Algorithm='http://www.w3.org/2000/09/xmldsig#enveloped-signature' />
				</Transforms>
				<DigestMethod Algorithm='http://www.w3.org/2001/04/xmlenc#sha256' />
				<DigestValue>NotUsed</DigestValue>
			</Reference>
		</SignedInfo>
		<SignatureValue>NotUsed</SignatureValue>
	</Signature>
</Receipt>";
		}
//#endif

		private async Task<bool> CheckForPaidVersion()
		{
			try
			{
				var receipt = await CurrentApp.GetAppReceiptAsync();
				XDocument doc = XDocument.Parse(receipt);
				var appReceipt = doc.Root.Element("AppReceipt");
				// Here is hardcoded release date of the v1.6, first free version
				return appReceipt.Attribute("LicenseType").Value == "Full" && DateTime.Parse(appReceipt.Attribute("PurchaseDate").Value) <= DateTime.FromBinary(635148864000000000);
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot parse full app receipt. Assuming not paid version.", ex);
				return false;
			}
		}

		private sealed class ProductTierInformation
		{
			public string[] Products { get; private set; }
			public ConcurrentQueue<string> SavedReceipts { get; private set; }

			public ProductTierInformation(IEnumerable<string> products)
			{
				this.Products = products.ToArray();
				this.SavedReceipts = new ConcurrentQueue<string>();
			}
		}
	}
}
