namespace DrawTheWorld.Core.UI.Messages
{
	/// <summary>
	/// Notification that asks user to buy more coins.
	/// </summary>
	public sealed class NotEnoughMoneyMessage
		: IMessage
	{
		/// <summary>
		/// Gets or sets the result of the operation.
		/// </summary>
		public bool Result { get; set; }
	}

	/// <summary>
	/// Notifies user about problem with the online service.
	/// </summary>
	public sealed class CannotAccessApiMessage
		: IMessage
	{
		/// <summary>
		/// Gets or sets value that specifies who sent this message(background/foreground operation).
		/// </summary>
		public bool IsBackground { get; set; }
	}

	/// <summary>
	/// Notifies user that the synchronization process has completed.
	/// </summary>
	public sealed class PacksSynchronizedMessage
		: IMessage
	{
		/// <summary>
		/// Gets or sets the number of successfully synced packs. 
		/// </summary>
		public int Successes { get; set; }

		/// <summary>
		/// Gets or sets the number of failures.
		/// </summary>
		public int Failures { get; set; }
	}

	/// <summary>
	/// Notifies user of successful pack purchase.
	/// </summary>
	public sealed class PackPurchasedMessage
		: IMessage
	{
		/// <summary>
		/// Gets or sets the pack that has been purchased.
		/// </summary>
		public UserData.Pack Pack { get; set; }
	}

	/// <summary>
	/// Notifies user of error in downloaded pack.
	/// </summary>
	public sealed class PackDataErrorMessage
		: IMessage
	{ }

	/// <summary>
	/// Notifies user of error during load of the pack.
	/// </summary>
	public sealed class PackLoadErrorMessage
		: IMessage
	{
		/// <summary>
		/// The type of a <see cref="PackLoadErrorMessage"/>.
		/// </summary>
		public enum MessageType
		{
			/// <summary>
			/// Single pack being loaded, single failure.
			/// </summary>
			SingleFailure,

			/// <summary>
			/// Multiple packs being loaded, multiple(but not all) failures.
			/// </summary>
			PartialFailure,

			/// <summary>
			/// Multiple packs being loaded, all failed.
			/// </summary>
			MultipleFailure
		}

		/// <summary>
		/// Gets or sets the type of the message.
		/// The default is <see cref="MessageType.SingleFailure"/>.
		/// </summary>
		public MessageType Type { get; set; }

		public PackLoadErrorMessage()
		{
			this.Type = MessageType.SingleFailure;
		}
	}

	/// <summary>
	/// Notifies user that the pack(in designer) could not be saved.
	/// </summary>
	public sealed class PackSaveErrorMessage
		: IMessage
	{
		/// <summary>
		/// Gets or sets the pack that cannot be saved.
		/// </summary>
		public UserData.MutablePack Pack { get; set; }
	}

	/// <summary>
	/// Notifies user of problem with the storage.
	/// </summary>
	public sealed class StorageProblemMessage
		: IMessage
	{ }

	/// <summary>
	/// Notifies user that he needs to sign in in.
	/// </summary>
	public sealed class SignInRequiredMessage
		: IMessage
	{
		/// <summary>
		/// Gets or sets the result of the operation.
		/// </summary>
		public bool Result { get; set; }
	}

	/// <summary>
	/// Notifies user that some error occured.
	/// </summary>
	public sealed class ErrorMessage
		: IMessage
	{ }

	/// <summary>
	/// Notifies user that some boards could not be imported.
	/// </summary>
	public sealed class BoardImportErrorMessage
		: IMessage
	{
		/// <summary>
		/// Gets or sets the count of errors.
		/// </summary>
		public int ErrorCount { get; set; }
	}

	/// <summary>
	/// Notifies user that the pack he/she tries to export is invalid.
	/// </summary>
	public sealed class PackNotValidMessage
		: IMessage
	{
		/// <summary>
		/// Gets or sets the pack that cannot be exported.
		/// </summary>
		public UserData.MutablePack Pack { get; set; }
	}

	/// <summary>
	/// Notifies user that the pack she/he tries to link with game is empty.
	/// </summary>
	public sealed class PackIsEmptyMessage
		: IMessage
	{ }
}
