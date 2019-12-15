using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// Interface that provides access to the persistent packs store.
	/// </summary>
	public interface IPackStore
	{
		/// <summary>
		/// Loads packs from the store.
		/// </summary>
		/// <param name="installCallback">Method that should be called for every pack in the repository.</param>
		/// <returns></returns>
		Task LoadPacks(Func<Stream, Guid, Task> installCallback);

		/// <summary>
		/// Removes pack from the store.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task RemovePack(Guid id);

		/// <summary>
		/// Saves a pack with specified <paramref name="id"/> to the stream specified in the <paramref name="saveCallback"/>.
		/// </summary>
		/// <remarks>
		/// The callback may be invoked asynchronously.
		/// </remarks>
		/// <param name="id"></param>
		/// <param name="saveCallback"></param>
		/// <returns></returns>
		Task SavePack(Guid id, Action<Stream> saveCallback);
	}

	/// <summary>
	/// <see cref="IPackStore"/> for demo packs.
	/// </summary>
	/// <remarks>
	/// Does not need to provide remove/save functionality.
	/// </remarks>
	public interface IDemoPackStore
		: IPackStore
	{ }

	/// <summary>
	/// <see cref="IPackStore"/> for custom packs.
	/// </summary>
	public interface ICustomPackStore
		: IPackStore
	{ }

	/// <summary>
	/// <see cref="IPackStore"/> for user-purchased packs.
	/// </summary>
	/// <remarks>
	/// When user is signed in, <see cref="UserId"/> will be set to valid value.
	/// When user signs out, <see cref="UserId"/> will be set to null and <see cref="Clear"/> will be called.
	/// If app is closed, nothing will change.
	/// </remarks>
	public interface IUserPackStore
		: IPackStore
	{
		/// <summary>
		/// Gets or sets the currently signed in user's id.
		/// </summary>
		string UserId { get; set; }

		/// <summary>
		/// Clears the repository for current user, because he signs out.
		/// </summary>
		Task Clear();
	}

	/// <summary>
	/// <see cref="IPackStore"/> for designer packs.
	/// </summary>
	/// <remarks>
	/// <see cref="SavePack"/> shoud retry the save if the pack cannot be accessed.
	/// </remarks>
	public interface IDesignerStore
		: IPackStore
	{ }

	/// <summary>
	/// Packs store for <see cref="LinkedPackRepository"/>
	/// </summary>
	public interface ILinkedPackStore
	{
		/// <summary>
		/// Adds pack with particular <paramref name="packId"/> to the store.
		/// </summary>
		/// <param name="packId"></param>
		void Add(Guid packId);

		/// <summary>
		/// Removes pack with particular <paramref name="packId"/> from the store.
		/// </summary>
		/// <param name="packId"></param>
		void Remove(Guid packId);

		/// <summary>
		/// Loads saved pack ids.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Guid> Load();
	}
}
