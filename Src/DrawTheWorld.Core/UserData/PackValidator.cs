using System.Linq;
using System.Collections.Generic;
using FLib;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core.UserData
{
	/// <summary>
	/// Utility class for packs validation.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Board, before export, must satisfy these conditions:
	/// </para>
	/// <para>
	/// <list type="number">
	///	  <item>
	///	    <description>
	///	    Default name must be specified for pack and for every board
	///	    </description>
	///	  </item>
	///	  <item>
	///	    <description>
	///	    Board's id must be unique within the pack.
	///	    </description>
	///	  </item>
	///	  <item>
	///	    <description>
	///	    Board's area must be at least equal to <see cref="Config.MinBoardArea"/>.
	///	    </description>
	///	  </item>
	///	  <item>
	///	    <description>
	///	    Data must be present.
	///	    </description>
	///	  </item>
	///	  <item>
	///	    <description>
	///	    Board's data must not be empty - at least one field must have fill value.
	///	    </description>
	///	  </item>
	///	  <item>
	///	    <description>
	///	    Board's must not use more colors than specified by <see cref="Config.MaxPaletteSize"/>.
	///	    </description>
	///	  </item>
	/// </list>
	/// </para>
	/// <para>
	/// In designer, conditions 1 and 2 does not have to be satisfied. 
	/// </para>
	/// </remarks>
	public static class PackValidator
	{
		/// <summary>
		/// Validates pack and throws exception if it is not valid.
		/// </summary>
		/// <param name="pack"></param>
		public static void ValidatePack(Pack pack)
		{
			FLib.Validate.Debug(() => pack, v => v.NotNull());

			var defaultName = pack.Name.DefaultTranslation;
			FLib.Validate.All(() => defaultName, v => v.NotNullAndNotWhiteSpace());

			FLib.Validate.All(() => pack.Boards, v => v
				.NotEmpty()
				.ForAll<BoardData>()
				.NotNull()
				.InternalValidateBoard(pack)
				);
		}

		/// <summary>
		/// Validates board for export and throws exception if it is not valid.
		/// </summary>
		/// <param name="board"></param>
		public static void ValidateBoard(BoardData board)
		{
			FLib.Validate.All(() => board, v => v.NotNull().InternalValidateBoard(null));
		}

		/// <summary>
		/// Validates imported image if it is not valid.
		/// </summary>
		/// <param name="board"></param>
		public static void ValidateImportedBoard(MutableBoardData board)
		{
			FLib.Validate.All(() => board, v => v
				.NotNull()
				.That(b => (b.Size.Width * b.Size.Height) >= Config.MinBoardArea, "Board is too small.")
				.That(b => ValidatePalette(b.Data), "Board's palette is too big.")
				);
		}

		private static void InternalValidateBoard(this Validator<BoardData> v, Pack pack)
		{
			v
				.That(p => !string.IsNullOrWhiteSpace(p.Name.DefaultTranslation), "Default name must be present.")
				.That(p => (p.Size.Width * p.Size.Height) >= Config.MinBoardArea, "Board is too small.")
				.That(p => p.Data != null && p.Data.Count == p.Size.Width * p.Size.Height, "Insufficient board data.")
				.That(p => !p.Data.All(f => !f.HasValue), "Pack cannot contain empty boards.")
				.That(p => ValidatePalette(p.Data), "Board's palette is too big.");
			if (pack != null)
				v.That(p => !pack.Boards.Where(p2 => p2 != p).Any(p2 => p2.Id == p.Id), "Every board must have unique Id.");
		}

		private static bool ValidatePalette(IReadOnlyList<Color?> data)
		{
			HashSet<Color> usedColors = new HashSet<Color>();
			for (int i = 0; i < data.Count; i++)
			{
				if (data[i].HasValue)
				{
					usedColors.Add(data[i].Value);
					if (usedColors.Count > Config.MaxPaletteSize)
						return false;
				}
			}
			return true;
		}
	}
}
