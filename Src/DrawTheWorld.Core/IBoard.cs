using System.Collections.Generic;
using DrawTheWorld.Core.UserData;

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Describes board in game.
	/// </summary>
	public interface IBoard
	{
		/// <summary>
		/// Gets the common information about the board data.
		/// </summary>
		IBoardInfoProvider BoardInfo { get; }

		/// <summary>
		/// Fields data.
		/// </summary>
		/// <remarks>
		/// Is of the same size as <see cref="BoardData.Data"/> and in the same order.
		/// Should not be changed manually, only <see cref="ITool"/> is allowed to change.
		/// </remarks>
		Field[] Fields { get; }

		/// <summary>
		/// Describes rows.
		/// </summary>
		/// <remarks>
		/// First dimension size is equal to <see cref="BoardData.Size"/>.Height, second varies.
		/// </remarks>
		IEnumerable<Block>[] Rows { get; }

		/// <summary>
		/// Describes columns.
		/// </summary>
		/// <remarks>
		/// First dimension size is equal to <see cref="BoardData.Size"/>.Width, second varies.
		/// </remarks>
		IEnumerable<Block>[] Columns { get; }
	}
}
