using System;
using System.ComponentModel;
using System.Linq;
using FLib;

#if WINRT
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace DrawTheWorld.Core.UserData
{
	/// <summary>
	/// Mutable version of <see cref="BoardData"/> to allow easy edits.
	/// </summary>
	/// <remarks>
	/// If size needs to be changed, change the <see cref="Size"/> first, then change the <see cref="Data"/>.
	/// </remarks>
	public sealed class MutableBoardData
		: INotifyPropertyChanged, UI.IHaveThumbnail, IBoardInfoProvider
	{
		private Guid _PackId = default(Guid);
		private readonly Guid _Id = default(Guid);
		private readonly MutableTranslations _Name = null;
		private Size _Size = default(Size);
		private Color?[] _Data = new Color?[0];
		private BitmapSource _Thumbnail = null;

		/// <inheritdoc />
		/// <remarks>
		/// When pack is not assigned to any pack, it is equal to <see cref="Guid.Empty"/>.
		/// </remarks>
		public Guid PackId
		{
			get { return this._PackId; }
			internal set { this._PackId = value; }
		}

		/// <inheritdoc />
		public Guid Id
		{
			get { return this._Id; }
		}

		/// <summary>
		/// The name of the board.
		/// </summary>
		public MutableTranslations Name
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
			set
			{
				if (value != this._Size)
				{
					this._Size = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}

		/// <summary>
		/// The data of the board.
		/// 
		/// Value needs to be of the same size as <see cref="Size"/> is.
		/// </summary>
		public Color?[] Data
		{
			get { return this._Data; }
			set
			{
				Validate.Debug(() => value, v => v.NotNull().CountEquals(this.Size.Width * this.Size.Height));
				if (this._Data != value)
				{
					this._Data = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}

		/// <inheritdoc />
		/// <remarks>
		/// Because introducing new type that will contain only reference to <see cref="MutableBoardData"/> and <see cref="Thumbnail"/> would
		/// be quite complex and introduce a lot of overhead, I decided to break SRP(again) and sneak this property directly into the data.
		/// Custom management of the thumbnails is still required, though.
		/// </remarks>
		public BitmapSource Thumbnail
		{
			get { return this._Thumbnail; }
			internal set
			{
				if (value != this._Thumbnail)
				{
					this._Thumbnail = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes empty object.
		/// </summary>
		/// <param name="id"></param>
		public MutableBoardData(Guid id)
		{
			Validate.Debug(() => id, v => v.NotEqual(Guid.Empty));

			this._Id = id;
			this._Name = new MutableTranslations();
		}

		/// <summary>
		/// Initializes the object using specified <paramref name="data"/>.
		/// </summary>
		/// <param name="data"></param>
		public MutableBoardData(BoardData data)
		{
			Validate.Debug(() => data, v => v.NotNull());

			this._Id = data.Id;
			this._PackId = data.PackId;
			this._Name = new MutableTranslations(data.Name);
			this._Size = data.Size;
			this._Data = data.Data.ToArray();
		}

		/// <summary>
		/// Creates board with the default settings.
		/// </summary>
		/// <returns></returns>
		public static MutableBoardData CreateDefault()
		{
			return new MutableBoardData(Guid.NewGuid())
			{
				Size = Config.DefaultBoardSize,
				Data = new Color?[Config.DefaultBoardSize.Width * Config.DefaultBoardSize.Height]
			};
		}

		/// <summary>
		/// Returns immutable copy of this object.
		/// </summary>
		/// <remarks>
		/// If <see cref="Data"/> is not of correct size, the exception will be thrown.
		/// </remarks>
		/// <returns></returns>
		public BoardData ToReadOnly()
		{
			Validate.Debug(() => this.PackId, v => v.NotEqual(Guid.Empty));
			Validate.Debug(() => this.Data, v => v.CountEquals(this.Size.Width * this.Size.Height));

			return new BoardData(this.PackId, this.Id, this.Name.ToReadOnly(), this.Size, (Color?[])this.Data.Clone());
		}

		/// <summary>
		/// Resets properties of the object to <paramref name="data"/>.
		/// </summary>
		/// <param name="data"></param>
		public void Reset(BoardData data)
		{
			Validate.Debug(() => data, v => v.NotNull());

			this._Name.Reset(data.Name);
			this._Size = data.Size;
			this._Data = data.Data.ToArray();
		}
	}
}
