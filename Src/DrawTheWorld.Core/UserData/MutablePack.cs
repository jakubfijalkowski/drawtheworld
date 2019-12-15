using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using FLib;

namespace DrawTheWorld.Core.UserData
{
	/// <summary>
	/// Mutable version of <see cref="Pack"/> to allow easy edits.
	/// </summary>
	/// <remarks>
	/// Even though most of the class is mutable, <see cref="Id"/> is immutable to prevent from duplicating packs.
	/// </remarks>
	public sealed class MutablePack
		: INotifyPropertyChanged
	{
		private readonly Guid _Id = default(Guid);
		private readonly MutableTranslations _Name = null;
		private readonly MutableTranslations _Description = null;
		private readonly MutableBoardObservableList _Boards = null;

		private string _Author = null;
		private string _AuthorsPage = null;

		/// <summary>
		/// Mutable the id.
		/// </summary>
		public Guid Id
		{
			get { return this._Id; }
		}

		/// <summary>
		/// The list of boards in pack.
		/// </summary>
		public IObservableList<MutableBoardData> Boards
		{
			get { return this._Boards; }
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		public MutableTranslations Name
		{
			get { return this._Name; }
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		public MutableTranslations Description
		{
			get { return this._Description; }
		}

		/// <summary>
		/// Gets the author.
		/// </summary>
		public string Author
		{
			get { return this._Author; }
			set
			{
				if (value != this._Author)
				{
					this._Author = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}

		/// <summary>
		/// Gets the author's page.
		/// </summary>
		public string AuthorsPage
		{
			get { return this._AuthorsPage; }
			set
			{
				if (value != this._AuthorsPage)
				{
					this._AuthorsPage = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes the object with the <paramref name="id"/>.
		/// </summary>
		/// <param name="id"></param>
		public MutablePack(Guid id)
		{
			Validate.Debug(() => id, v => v.NotEqual(Guid.Empty));

			this._Id = id;
			this._Name = new MutableTranslations();
			this._Description = new MutableTranslations();
			this._Boards = new MutableBoardObservableList(id);
		}

		/// <summary>
		/// Initializes the object with specified <paramref name="data"/>.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="idOverride"></param>
		public MutablePack(Pack data, Guid idOverride = default(Guid))
		{
			Validate.Debug(() => data, v => v.NotNull());

			this._Id = idOverride != default(Guid) ? idOverride : data.Id;
			this._Name = new MutableTranslations(data.Name);
			this._Description = new MutableTranslations(data.Description);
			this._Boards = new MutableBoardObservableList(this._Id, data.Boards.Select(b => new MutableBoardData(b)));
			this._Author = data.Author;
			this._AuthorsPage = data.AuthorsPage;
		}

		/// <summary>
		/// Returns immutable copy of this object.
		/// </summary>
		/// <returns></returns>
		public Pack ToReadOnly()
		{
			return new Pack(this.Id, this.Boards.Select(b => b.ToReadOnly()).ToList(), this.Name.ToReadOnly(), this.Description.ToReadOnly(), this.Author, this.AuthorsPage);
		}

		/// <summary>
		/// Returns immutable copy of this object with filtered boards.
		/// </summary>
		/// <param name="boardPredicate"></param>
		/// <returns></returns>
		public Pack ToReadOnly(Func<MutableBoardData, bool> boardPredicate)
		{
			return new Pack(this.Id, this.Boards.Where(boardPredicate).Select(b => b.ToReadOnly()).ToList(), this.Name.ToReadOnly(), this.Description.ToReadOnly(), this.Author, this.AuthorsPage);
		}

		private sealed class MutableBoardObservableList
			: ObservableCollection<MutableBoardData>, IObservableList<MutableBoardData>
		{
			private readonly Guid PackId = default(Guid);

			public MutableBoardObservableList(Guid packId)
			{
				Validate.Debug(() => packId, v => v.NotEqual(Guid.Empty));
				this.PackId = packId;
			}

			public MutableBoardObservableList(Guid packId, IEnumerable<MutableBoardData> source)
				: base(source)
			{
				Validate.Debug(() => packId, v => v.NotEqual(Guid.Empty));
				this.PackId = packId;
			}

			protected override void SetItem(int index, MutableBoardData item)
			{
				Validate.Debug(() => item, v => v.That(p => p.PackId, v2 => v2.Equal(Guid.Empty)));
				item.PackId = this.PackId;
				base.SetItem(index, item);
			}

			protected override void InsertItem(int index, MutableBoardData item)
			{
				Validate.Debug(() => item, v => v.That(p => p.PackId, v2 => v2.Equal(Guid.Empty)));
				item.PackId = this.PackId;
				base.InsertItem(index, item);
			}

			protected override void RemoveItem(int index)
			{
				var item = this[index];
				item.PackId = Guid.Empty;
				base.RemoveItem(index);
			}
		}
	}
}
