using System;
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
	/// Represents read-only board data.
	/// </summary>
	public sealed class BoardData
		: IBoardInfoProvider
	{
		private readonly Guid _PackId = default(Guid);
		private readonly Guid _Id = default(Guid);
		private readonly Translations _Name = null;
		private readonly Size _Size = default(Size);
		private readonly Color?[] _Data = null;

		/// <inheritdoc />
		public Guid PackId
		{
			get { return this._PackId; }
		}

		/// <inheritdoc />
		public Guid Id
		{
			get { return this._Id; }
		}

		/// <summary>
		/// The name of the board.
		/// </summary>
		public Translations Name
		{
			get { return this._Name; }
		}

		/// <inheritdoc />
		public string MainName
		{
			get { return this.Name.MainTranslation; }
		}

		/// <inheritdoc />
		public Size Size
		{
			get { return this._Size; }
		}

		/// <summary>
		/// The data of the board.
		/// </summary>
		public IReadOnlyList<Color?> Data
		{
			get { return this._Data; }
		}

		/// <summary>
		/// Initializes the board data.
		/// </summary>
		/// <param name="packId"></param>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="size"></param>
		/// <param name="data"></param>
		public BoardData(Guid packId, Guid id, Translations name, Size size, Color?[] data)
		{
			Validate.Debug(() => packId, v => v.NotEqual(Guid.Empty));
			Validate.Debug(() => id, v => v.NotEqual(Guid.Empty));
			Validate.Debug(() => name, v => v.NotNull());
			Validate.Debug(() => size, v => v.IsValidSize());
			Validate.Debug(() => data, v => v.NotNull().CountEquals(size.Width * size.Height));

			this._PackId = packId;
			this._Id = id;
			this._Name = name;
			this._Size = size;
			this._Data = data;
		}
	}
}
