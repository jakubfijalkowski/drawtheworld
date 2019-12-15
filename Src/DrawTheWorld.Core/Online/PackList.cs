using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using FLib;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Web.Api.Public;
using System.Threading;
using System.Collections;
using Windows.Foundation;

#if WINRT
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Data;
using LoadMoreItemsAsyncResult = Windows.Foundation.IAsyncOperation<Windows.UI.Xaml.Data.LoadMoreItemsResult>;
#else
using LoadMoreItemsAsyncResult = System.Threading.Tasks.Task<DrawTheWorld.Core.LoadMoreItemsResult>;
#endif

namespace DrawTheWorld.Core.Online
{
	/// <summary>
	/// Provides access to the online packs.
	/// </summary>
	/// TODO: add ordering in next version
	public sealed class PackList
		: IReadOnlyObservableList<Pack>, ISupportIncrementalLoading, IList
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.Online.PackList");

		private readonly List<Pack> Packs = new List<Pack>();
		private readonly Client Client = null;

		private bool IsBusy = false;

		/// <inheritdoc />
		public int Count
		{
			get { return this.Packs.Count; }
		}

		/// <summary>
		/// Gets the real number of items.
		/// </summary>
		/// <remarks>
		/// If it is equal to -1, it means that no items were loaded and <see cref="LoadMoreItemsAsync"/> should be called.
		/// </remarks>
		public int RealCount { get; private set; }

		/// <inheritdoc />
		public bool HasMoreItems
		{
			get { return this.RealCount == -1 || this.Count < this.RealCount; }
		}

		/// <inheritdoc />
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <inheritdoc />
		public Pack this[int index]
		{
			get { return this.Packs[index]; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="client"></param>
		public PackList(Client client)
		{
			Validate.Debug(() => client, v => v.NotNull());

			this.Client = client;
			this.RealCount = -1;
		}

		/// <inheritdoc />
		public LoadMoreItemsAsyncResult LoadMoreItemsAsync(uint count)
		{
			if (this.IsBusy)
				throw new InvalidOperationException();

            return this.LoadItems(default, count).AsAsyncOperation();
		}

		/// <inheritdoc />
		public IEnumerator<Web.Api.Public.Pack> GetEnumerator()
		{
			return this.Packs.GetEnumerator();
		}

		/// <inheritdoc />
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.Packs.GetEnumerator();
		}

		private Task<LoadMoreItemsResult> LoadItems(CancellationToken token, uint count)
		{
			this.IsBusy = true;
			try
			{
                //if (this.RealCount == -1)
                //{
                //	Logger.Info("Loading count.");
                //	var result = await this.Client.Download(c => c.GetAsync<int>("packs/count"), ex => Logger.Warn("Cannot load pack count.", ex), token);

                //	if (token.IsCancellationRequested)
                //	{
                //		Logger.Debug("Cancelled");
                //		return new LoadMoreItemsResult { Count = 0 };
                //	}
                //	if (result.Item1)
                //	{
                //		Logger.Error("Cannot load pack count. Aborting.");
                //		return new LoadMoreItemsResult { Count = 0 };
                //	}
                //	this.RealCount = result.Item2;
                //}

                //count = Math.Min(count, (uint)(this.RealCount - this.Count));
                //if (count > 0)
                //{
                //	Logger.Info("Loading {0} more packs.", count);

                //	string request = "packs?language=" + LanguageHelper.CurrentLanguage + "&start=" + this.Count + "&count=" + count;
                //	var result = await this.Client.Download(c => c.GetAsync<IList<Pack>>(request), ex => Logger.Warn("Cannot load packs.", ex), token);

                //	if (token.IsCancellationRequested)
                //	{
                //		Logger.Debug("Cancelled");
                //		return new LoadMoreItemsResult { Count = 0 };
                //	}
                //	if (result.Item1)
                //	{
                //		Logger.Error("Cannot load packs. Aborting.");
                //		return new LoadMoreItemsResult { Count = 0 };
                //	}

                //	var oldCount = this.Count;
                //	this.Packs.AddRange(result.Item2);
                //	this.NotifyOfNewPacks(oldCount, result.Item2.Count);

                //	count = (uint)result.Item2.Count;
                //}
                //return new LoadMoreItemsResult { Count = count };
                return Task.FromResult(new LoadMoreItemsResult { Count = 0 });
			}
			finally
			{
				this.IsBusy = false;
			}
		}

		private void NotifyOfNewPacks(int start, int count)
		{
			var @event = this.CollectionChanged;
			if (@event != null)
			{
				for (int i = start; i < start + count; i++)
					@event(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.Packs[i], i));
			}
		}

		#region IList Implementation, not used
		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		object IList.this[int index]
		{
			get { return this.Packs[index]; }
			set { throw new InvalidOperationException(); }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { throw new InvalidOperationException(); }
		}

		int IList.Add(object value)
		{
			throw new InvalidOperationException();
		}

		void IList.Clear()
		{
			throw new InvalidOperationException();
		}

		bool IList.Contains(object value)
		{
			return value is Pack && this.Packs.Contains((Pack)value);
		}

		int IList.IndexOf(object value)
		{
			if (value is Pack)
				return this.Packs.IndexOf((Pack)value);
			return -1;
		}

		void IList.Insert(int index, object value)
		{
			throw new InvalidOperationException();
		}

		void IList.Remove(object value)
		{
			throw new InvalidOperationException();
		}

		void IList.RemoveAt(int index)
		{
			throw new InvalidOperationException();
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((IList)this.Packs).CopyTo(array, index);
		}
        #endregion
    }
}
