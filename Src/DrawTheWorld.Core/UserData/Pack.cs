using System;
using System.Collections.Generic;
using FLib;

namespace DrawTheWorld.Core.UserData
{
	/// <summary>
	/// Represents read-only pack data.
	/// </summary>
	public sealed class Pack
	{
		private readonly Guid _Id = default(Guid);
		private readonly IReadOnlyList<BoardData> _Boards = null;
		private readonly Translations _Name = null;
		private readonly Translations _Description = null;
		private readonly string _Author = null;
		private readonly string _AuthorsPage = null;

		/// <summary>
		/// The id.
		/// </summary>
		public Guid Id
		{
			get { return this._Id; }
		}

		/// <summary>
		/// The list of boards in pack.
		/// </summary>
		public IReadOnlyList<BoardData> Boards
		{
			get { return this._Boards; }
		}

		/// <summary>
		/// Gets the nam.
		/// </summary>
		public Translations Name
		{
			get { return this._Name; }
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		public Translations Description
		{
			get { return this._Description; }
		}

		/// <summary>
		/// Gets the author.
		/// </summary>
		public string Author
		{
			get { return this._Author; }
		}

		/// <summary>
		/// Gets the author's page.
		/// </summary>
		public string AuthorsPage
		{
			get { return this._AuthorsPage; }
		}

		/// <summary>
		/// Initializes the pack.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="boards"></param>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="author"></param>
		/// <param name="authorsPage"></param>
		public Pack(Guid id, IReadOnlyList<BoardData> boards, Translations name, Translations description, string author, string authorsPage)
		{
			Validate.Debug(() => boards, v => v.NotNull());
			Validate.Debug(() => name, v => v.NotNull());
			Validate.Debug(() => description, v => v.NotNull());

			this._Id = id;
			this._Boards = boards;
			this._Name = name;
			this._Description = description;
			this._Author = author ?? string.Empty;
			this._AuthorsPage = authorsPage ?? string.Empty;
		}
	}
}
