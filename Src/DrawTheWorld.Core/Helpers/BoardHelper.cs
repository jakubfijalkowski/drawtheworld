using System.Collections.Generic;
using System.Linq;
using DrawTheWorld.Core.UserData;
using FLib;

namespace DrawTheWorld.Core.Helpers
{
	public static class BoardHelper
	{
		/// <summary>
		/// Gets fields at specified <paramref name="locations"/>
		/// </summary>
		/// <param name="board"></param>
		/// <param name="locations"></param>
		/// <returns></returns>
		public static Field[] GetFields(this IBoard board, IEnumerable<Point> locations)
		{
			Validate.Debug(() => locations, v => v.NotNull().ForAll<Point>()
				.That(p => p.X >= 0 && p.X < board.BoardInfo.Size.Width && p.Y >= 0 && p.Y < board.BoardInfo.Size.Height, "All locations should be correct"));
			return locations.Select(p => board.Fields[p.X + p.Y * board.BoardInfo.Size.Width]).ToArray();
		}

		/// <summary>
		/// Filter for empty boards.
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		public static bool FilterEmpty(MutableBoardData board)
		{
			return board.Data != null && board.Data.Any(c => c.HasValue);
		}
	}
}
