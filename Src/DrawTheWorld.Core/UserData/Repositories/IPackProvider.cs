namespace DrawTheWorld.Core.UserData
{
	/// <summary>
	/// Denotes pack type.
	/// </summary>
	public enum PackType
		: int
	{
		/// <summary>
		/// Demo pack, installed along the game.
		/// </summary>
		Demo,

		/// <summary>
		/// A pack was purchased from the online store.
		/// </summary>
		UserPurchase,

		/// <summary>
		/// A pack was installed using custom procedure.
		/// </summary>
		CustomInstallation,
		
		/// <summary>
		/// A pack was linked from <see cref="DesignerRepository"/>.
		/// </summary>
		Linked
	}

	/// <summary>
	/// Interfaces that implement this interface provide packs.
	/// </summary>
	public interface IPackProvider
		: IReadOnlyObservableList<Pack>
	{
		/// <summary>
		/// Gets the type of the packs that are stored in this provider.
		/// </summary>
		PackType PackType { get; }
	}
}
