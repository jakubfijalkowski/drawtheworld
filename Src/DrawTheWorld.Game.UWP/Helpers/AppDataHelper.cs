using System;
using DrawTheWorld.Core;
using DrawTheWorld.Core.UserData;
using FLib;
using Windows.Storage;

namespace DrawTheWorld.Game.Helpers
{
	static class AppDataHelper
	{
		/// <summary>
		/// Loads particular value from container and tries to cast it to particular type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="container"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static T Get<T>(this ApplicationDataContainer container, string key, T def = default(T))
		{
			object val;
			if (!container.Values.TryGetValue(key, out val) || !(val is T))
				return def;
			return (T)val;
		}

		/// <summary>
		/// Creates <see cref="BoardStatistics"/> from <see cref="IGame"/> object.
		/// </summary>
		/// <param name="game"></param>
		/// <param name="customReason"></param>
		/// <returns></returns>
		public static BoardStatistics CreateStatistics(this IGame game, FinishReason? customReason = null, bool useNow = false)
		{
			var bi = game.Board.BoardInfo;
			var finishDate = useNow ? DateTime.UtcNow : game.FinishDate;
			int time = (int)(finishDate - game.StartDate).TotalSeconds + game.Fine;
			var finishReason = customReason ?? game.FinishReason;
			return new BoardStatistics(bi.PackId, bi.Id, time, DateTime.UtcNow, finishReason);
		}

		/// <summary>
		/// Dumps <see cref="BoardStatistics"/> into <see cref="ApplicationDataCompositeValue"/>.
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		public static ApplicationDataCompositeValue Dump(this BoardStatistics board)
		{
			var record = new ApplicationDataCompositeValue();
			record.Add(board.NameOf(_ => _.PackId), board.PackId);
			record.Add(board.NameOf(_ => _.BoardId), board.BoardId);
			record.Add(board.NameOf(_ => _.Time), board.Time);
			record.Add(board.NameOf(_ => _.Date), board.Date.ToBinary());
			record.Add(board.NameOf(_ => _.Result), (byte)board.Result);
			return record;
		}

		/// <summary>
		/// Restores <see cref="BoardStatistics"/> from <see cref="ApplicationDataCompositeValue"/> created by <see cref="Dump"/>.
		/// </summary>
		/// <param name="stats"></param>
		/// <returns></returns>
		public static BoardStatistics RestoreStatistics(this ApplicationDataCompositeValue stats)
		{
			const BoardStatistics Empty = null;
			Guid packId = (Guid)stats[Empty.NameOf(_ => _.PackId)];
			Guid boardId = (Guid)stats[Empty.NameOf(_ => _.BoardId)];
			int time = (int)stats[Empty.NameOf(_ => _.Time)];
			DateTime date = DateTime.FromBinary((long)stats[Empty.NameOf(_ => _.Date)]);
			FinishReason result = (FinishReason)(byte)stats[Empty.NameOf(_ => _.Result)];
			return new BoardStatistics(packId, boardId, time, date, result);
		}
	}
}
