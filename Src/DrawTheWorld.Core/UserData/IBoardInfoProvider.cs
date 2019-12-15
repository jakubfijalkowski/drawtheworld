using System;

namespace DrawTheWorld.Core.UserData
{
	/// <summary>
	/// The classes that implement this interface provide information about the board.
	/// </summary>
	public interface IBoardInfoProvider
	{
		/// <summary>
		/// Gets the id if the pack which this board is in.
		/// </summary>
		Guid PackId { get; }

		/// <summary>
		/// Gets the id of the board.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// Gets the name that matches current language best.
		/// </summary>
		string MainName { get; }

		/// <summary>
		/// Gets the size of the board.
		/// </summary>
		Size Size { get; }
	}
}
