using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DrawTheWorld.Core.UserData;
using DrawTheWorld.Web.Api.Public;
using FLib;

namespace DrawTheWorld.Core.Online
{
	/// <summary>
	/// Online API cannot be accessed. See inner exception or <see cref="StatusCode"/> for more details.
	/// </summary>
	public sealed class ApiException
		: Exception
	{
		/// <summary>
		/// Status code of the response.
		/// </summary>
		public HttpStatusCode StatusCode { get; private set; }

		public ApiException(HttpStatusCode code) { this.StatusCode = code; }
		public ApiException(string message, HttpStatusCode code) : base(message) { this.StatusCode = code; }
		public ApiException(string message) : base(message) { }
		public ApiException(Exception inner) : base(string.Empty, inner) { }
	}

	/// <summary>
	/// The receipt passed to the server is invalid.
	/// </summary>
	public sealed class InvalidReceiptException : Exception { }

	/// <summary>
	/// User has too little money.
	/// </summary>
	public sealed class PaymentRequiredException : Exception { }

	/// <summary>
	/// Manages user account.
	/// </summary>
	public sealed class UserAccountManager
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.Online.UserAccount");

		private readonly AccountManager _AccountManager = null;

		private Client Client
		{
			get { return this._AccountManager.AuthorizedClient; }
		}

		/// <summary>
		/// The event is raised when pack is successfully purchased.
		/// </summary>
		public event Action<UserAccountManager, Guid, string> PackPurchased;

		/// <summary>
		/// Gets the <see cref="AccountManager"/> bound to this object.
		/// </summary>
		public AccountManager AccountManager
		{
			get { return this._AccountManager; }
		}

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="manager"></param>
		public UserAccountManager(AccountManager manager)
		{
			Validate.Debug(() => manager, v => v.NotNull());

			this._AccountManager = manager;
		}

		/// <summary>
		/// Purchases some product.
		/// </summary>
		/// <param name="receipt"></param>
		/// <param name="store"></param>
		/// <param name="productType"></param>
		/// <param name="tier"></param>
		/// <returns><see cref="PurchaseResponse.ApiError"/>, <see cref="PurchaseResponse.InvalidReceipt"/>, <see cref="PurchaseResponse.Unauthorized"/> or <see cref="PurchaseResponse.Purchased"/>.</returns>
		public Task Purchase(string receipt, Store store, ProductType productType, int tier)
		{
			Logger.Info("Trying to purchase product from {0}.", store);
            return Task.CompletedTask;
			//HttpResponseMessage response = null;
			//try
			//{
			//	response = await this.AccountManager.AuthorizedClient.PostAsync("account/purchase",
			//		new PurchaseRequest
			//		{
			//			Store = store,
			//			ProductType = productType,
			//			Tier = tier,
			//			Receipt = receipt
			//		}, true);
			//	await this.AccountManager.RefreshUser();
			//}
			//catch (Exception ex)
			//{
			//	Logger.Error("Cannot purchase product.", ex);
			//	throw new ApiException(ex);
			//}

			//if (response.StatusCode == HttpStatusCode.BadRequest)
			//{
			//	Logger.Warn("The receipt is invalid.");
			//	throw new InvalidReceiptException();
			//}
			//else if (!response.IsSuccessStatusCode)
			//{
			//	Logger.Error("Cannot purchase product because of server error. Status code: {0}.", response.StatusCode);
			//	throw new ApiException(response.StatusCode);
			//}
		}

		/// <summary>
		/// Purchases pack.
		/// </summary>
		/// <param name="packId"></param>
		/// <returns><see cref="PurchaseResponse.ApiError"/>, <see cref="PurchaseResponse.PaymentRequired"/>, <see cref="PurchaseResponse.Unauthorized"/> or <see cref="PurchaseResponse.Purchased"/>.</returns>
		public Task PurchasePack(Guid packId)
		{
			Logger.Info("Trying to purchase pack {0}.", packId);
            return Task.CompletedTask;
			//HttpResponseMessage response = null;
			//try
			//{
			//	response = await this.AccountManager.AuthorizedClient.GetAsync("pack/{0}/purchase".FormatWith(packId), true);
			//}
			//catch (Exception ex)
			//{
			//	Logger.Warn("Cannot purchase pack {0}.".FormatWith(packId), ex);
			//	throw new ApiException(ex);
			//}

			//if (!response.IsSuccessStatusCode)
			//{
			//	Logger.Warn("Cannot download pack with id {0}. Server status: {1}.", packId, response.StatusCode);
			//	if (response.StatusCode == HttpStatusCode.PaymentRequired)
			//		throw new PaymentRequiredException();
			//	throw new ApiException(response.StatusCode);
			//}

			//if (response.StatusCode == HttpStatusCode.NoContent)
			//{
			//	Logger.Info("Pack {0} already purchased, ignoring.", packId);
			//	return;
			//}

			//string packData = null;
			//try
			//{
			//	packData = await this.AccountManager.AuthorizedClient.DeserializeString(response);
			//}
			//catch (Exception ex)
			//{
			//	Logger.Warn("Cannot read server response.", ex);
			//	this.PackPurchased.Raise(this, packId, null);
			//	throw new PackLoadException("Cannot parse pack.", ex);
			//}

			//Logger.Info("Pack {0} purchased and downloaded.", packId);
			//this.PackPurchased.Raise(this, packId, packData);
		}
	}
}
