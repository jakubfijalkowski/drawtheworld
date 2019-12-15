using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using FLib;

namespace DrawTheWorld.Core.UserData.Repositories
{
	/// <summary>
	/// Repository with demo packs.
	/// </summary>
	public sealed class DemoRepository
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.DemoRepository");
		private readonly PacksCollection _Provider = new PacksCollection();
		private readonly IDemoPackStore Store = null;

		/// <summary>
		/// Gets the provider of packs for this repository.
		/// </summary>
		public IPackProvider Provider
		{
			get { return this._Provider; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="store"></param>
		public DemoRepository(IDemoPackStore store)
		{
			Validate.Debug(() => store, v => v.NotNull());

			this.Store = store;
		}

		/// <summary>
		/// Loads packs from the demo store.
		/// </summary>
		/// <returns></returns>
		public async Task Load()
		{
			Logger.Info("Loading packs from the demo store.");

			await this.Store.LoadPacks(async (s, id) =>
			{
				Logger.Trace("Loading pack {0}.", id);
				var pack = await Task.Run(() => PackLoader.Load(s, id));
				this._Provider.Add(pack);
				Logger.Info("Pack {0}({1}) loaded.", pack.Name.DefaultTranslation, pack.Id);
			});
		}

		private sealed class PacksCollection
			: ObservableCollection<Pack>, IPackProvider
		{
			public PackType PackType
			{
				get { return PackType.Demo; }
			}
		}
	}
}
