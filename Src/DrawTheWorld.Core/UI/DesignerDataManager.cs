using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UserData;
using DrawTheWorld.Core.UserData.Repositories;
using FLib;

namespace DrawTheWorld.Core.UI
{
	/// <summary>
	/// Manages generation of the thumbnails for designer.
	/// </summary>
	public sealed class DesignerDataManager
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.DesignerDataManager");
		private readonly DesignerRepository Repository = null;
		private readonly IThumbnailStore<MutableBoardData> Thumbnails = null;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="thumbnails"></param>
		public DesignerDataManager(DesignerRepository repository, IThumbnailStore<MutableBoardData> thumbnails)
		{
			Validate.Debug(() => repository, v => v.NotNull());
			Validate.Debug(() => thumbnails, v => v.NotNull());

			this.Repository = repository;
			this.Thumbnails = thumbnails;
		}

		/// <summary>
		/// Loads all packs from <see cref="DesignerRepository"/> to the manager.
		/// </summary>
		public void Load()
		{
			this.Repository.Packs.ForEach(this.OnPackAdded);
			this.Repository.Packs.CollectionChanged += this.OnPackCollectionChanged;
		}

		private async void GenerateThumbnail(MutableBoardData board, bool forceRegeneration = false)
		{
			if (forceRegeneration)
				board.Thumbnail = await this.Thumbnails.RegenerateThumbnail(board);
			else
				board.Thumbnail = await this.Thumbnails.GetThumbnail(board) ?? await this.Thumbnails.RegenerateThumbnail(board);
		}

		private void OnPackCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					e.NewItems.Cast<MutablePack>().ForEach(this.OnPackAdded);
					break;
				case NotifyCollectionChangedAction.Remove:
					e.OldItems.Cast<MutablePack>().ForEach(this.OnPackRemoved);
					break;

				default:
					throw new NotSupportedException();
			}
		}

		private void OnBoardsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					e.NewItems.Cast<MutableBoardData>().ForEach(this.OnBoardAdded);
					break;
				case NotifyCollectionChangedAction.Remove:
					e.OldItems.Cast<MutableBoardData>().ForEach(this.OnBoardRemoved);
					break;

				default:
					throw new NotSupportedException();
			}
		}

		private void OnBoardDataChanged(object sender, PropertyChangedEventArgs e)
		{
			var board = sender as MutableBoardData;
			if (e.PropertyName == board.NameOf(_ => _.Data))
				this.GenerateThumbnail(board, true);
		}

		private void OnPackAdded(MutablePack pack)
		{
			Logger.Debug("Generating thumbnails for pack '{0}'({1}).", pack.Name.DefaultTranslation, pack.Id);
			pack.Boards.CollectionChanged += this.OnBoardsChanged;
			foreach (var board in pack.Boards)
				this.OnBoardAdded(board);
		}

		private void OnPackRemoved(MutablePack pack)
		{
			Logger.Debug("Cleaning thumbnails for pack '{0}'({1}).", pack.Name.DefaultTranslation, pack.Id);
			pack.Boards.CollectionChanged -= this.OnBoardsChanged;
			foreach (var board in pack.Boards)
				this.OnBoardRemoved(board);
		}

		private void OnBoardAdded(MutableBoardData board)
		{
			Logger.Debug("Generating thumbnail for board '{0}'({1}).", board.Name.DefaultTranslation, board.Id);
			board.PropertyChanged += this.OnBoardDataChanged;
			this.GenerateThumbnail(board);
		}

		private void OnBoardRemoved(MutableBoardData board)
		{
			Logger.Debug("Cleaning thumbnail for board '{0}'({1}).", board.Name.DefaultTranslation, board.Id);
			board.PropertyChanged -= this.OnBoardDataChanged;
			Task.Run(() => this.Thumbnails.Clean(board));
		}
	}
}
