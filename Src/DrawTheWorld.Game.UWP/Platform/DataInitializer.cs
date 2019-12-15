using System.Threading.Tasks;
using DrawTheWorld.Core.Online;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Core.UserData;
using DrawTheWorld.Core.UserData.Repositories;

namespace DrawTheWorld.Game.Platform
{
	/// <summary>
	/// Wires the UI with Core modules.
	/// </summary>
	sealed class DataInitializer
	{
		private readonly UserStatistics Statistics = null;
		private readonly AccountManager AccountManager = null;

		private readonly OnlineRepository OnlineRepository = null;
		private readonly CustomPackRepository PackRepository = null;
		private readonly DesignerRepository DesignerRepository = null;
		private readonly DemoRepository DemoRepository = null;
		private readonly LinkedPackRepository LinkedRepository = null;

		private readonly GameDataManager GameDataManager = null;
		private readonly DesignerDataManager DesignerDataManager = null;

		/// <summary>
		/// Initializes some parts of the object, call <see cref="Initialize"/> in order to fully initialize the object.
		/// </summary>
		public DataInitializer(UserStatistics userStats, AccountManager accountManager,
			CustomPackRepository customPackRepo, OnlineRepository onlineRepo, DesignerRepository designerRepo, DemoRepository demoRepo, LinkedPackRepository linkedRepo,
			GameDataManager gameData, DesignerDataManager designerData)
		{
			this.Statistics = userStats;
			this.AccountManager = accountManager;

			this.PackRepository = customPackRepo;
			this.OnlineRepository = onlineRepo;
			this.DesignerRepository = designerRepo;
			this.DemoRepository = demoRepo;
			this.LinkedRepository = linkedRepo;

			this.GameDataManager = gameData;
			this.DesignerDataManager = designerData;
		}

		/// <summary>
		/// Loads data and initializes game components.
		/// </summary>
		/// <returns></returns>
		public async Task Initialize()
		{
			await this.PackRepository.Load();
			await this.DesignerRepository.Load();
			await this.DemoRepository.Load();
			await this.Statistics.Load();
			await this.LinkedRepository.Load();

			this.GameDataManager.Load();
			this.DesignerDataManager.Load();
		}
	}
}
