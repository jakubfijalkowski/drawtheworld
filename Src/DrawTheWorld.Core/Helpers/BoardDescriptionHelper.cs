using System;
using System.Collections.Generic;
using FLib;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core.Helpers
{
	/// <summary>
	/// Helps managing board description(<see cref="IBoard.Rows"/> and <see cref="IBoard.Colums"/>).
	/// </summary>
	public static class BoardDescriptionHelper
	{
		/// <summary>
		/// Build whole description(with new lists).
		/// </summary>
		/// <param name="board"></param>
		/// <param name="rows"></param>
		/// <param name="columns"></param>
		/// <param name="useFields"></param>
		/// <param name="typeCreator"></param>
		public static void BuildDescriptions(IBoard board, out IList<Block>[] rows, out IList<Block>[] columns, bool useFields, Func<IList<Block>> typeCreator)
		{
			rows = new IList<Block>[board.BoardInfo.Size.Height];
			for (int i = 0; i < board.BoardInfo.Size.Height; i++)
			{
				rows[i] = typeCreator();
				BuildDescriptionForRow(board, rows[i], i, useFields);
			}

			columns = new IList<Block>[board.BoardInfo.Size.Width];
			for (int i = 0; i < board.BoardInfo.Size.Width; i++)
			{
				columns[i] = typeCreator();
				BuildDescriptionForColumn(board, columns[i], i, useFields);
			}
		}

		/// <summary>
		/// Helper method for creating description of single row.
		/// </summary>
		/// <param name="board"></param>
		/// <param name="row"></param>
		/// <param name="i"></param>
		/// <param name="useFill"></param>
		public static void BuildDescriptionForRow(IBoard board, IList<Block> row, int i, bool useFill)
		{
			BuildSingleLine(row, board, i * board.BoardInfo.Size.Width, (i + 1) * board.BoardInfo.Size.Width, true, useFill);
		}

		/// <summary>
		/// Helper method for creating description of single column.
		/// </summary>
		/// <param name="board"></param>
		/// <param name="column"></param>
		/// <param name="i"></param>
		/// <param name="useFill"></param>
		public static void BuildDescriptionForColumn(IBoard board, IList<Block> column, int i, bool useFill)
		{
			BuildSingleLine(column, board, i, board.BoardInfo.Size.Width * (board.BoardInfo.Size.Height - 1) + i + 1, false, useFill);
		}

		/// <summary>
		/// Builds description of single line.
		/// </summary>
		/// <param name="blocks">Output list.</param>
		/// <param name="board"></param>
		/// <param name="start">Start index, inclusive.</param>
		/// <param name="end">End index, exclusive.</param>
		/// <param name="isRow"></param>
		/// <param name="useFill"></param>
		public static void BuildSingleLine(IList<Block> blocks, IBoard board, int start, int end, bool isRow, bool useFill)
		{
			Validate.Debug(() => blocks, v => v.NotNull());
			Validate.Debug(() => board, v => v.NotNull());
			Validate.Debug(() => start, v => v.InRange(0, board.BoardInfo.Size.Width * board.BoardInfo.Size.Height - 1));
			Validate.Debug(() => end, v => v.InRange(0, board.BoardInfo.Size.Width * board.BoardInfo.Size.Height));

			blocks.Clear();

			int step = isRow ? 1 : board.BoardInfo.Size.Width;

			Block lastBlock = null;
			Color? lastColor = null;

			for (int i = start; i < end; i += step)
			{
				var field = board.Fields[i];
				var c = useFill ? board.Fields[i].Fill : board.Fields[i].CorrectFill;
				if (c != lastColor)
				{
					if (lastBlock != null)
					{
						blocks.Add(lastBlock);
						lastBlock = null;
					}
					if (lastBlock == null && c.HasValue)
					{
						lastBlock = new Block(c.Value, 1);
						if (!useFill)
						{
							lastBlock.AssignedFields = new List<Field>();
							lastBlock.AssignedFields.Add(field);
						}
					}
					lastColor = c;
				}
				else if (lastBlock != null)
				{
					lastBlock.Count++;
					if (!useFill)
						lastBlock.AssignedFields.Add(field);
				}
			}
			if (lastBlock != null)
				blocks.Add(lastBlock);
		}
	}
}
