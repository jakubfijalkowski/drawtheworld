using DrawTheWorld.Core.Platform;
using Windows.Foundation;
using Windows.Storage;

namespace DrawTheWorld.Game.Utilities.Versioning
{
	/// <summary>
	/// MAnages the verion of the <see cref="ApplicationData"/>.
	/// </summary>
	sealed class ApplicationDataVersioner
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.ApplicationDataVersioner");
		private const uint CurrentVersion = 2;

		private readonly IFeatureProvider Features = null;
		private readonly ICustomPackStore CustomStore = null;
		private readonly IDesignerStore DesignerStore = null;
		private readonly IStatisticsStore StatisticsStore = null;

		public ApplicationDataVersioner(IFeatureProvider features, ICustomPackStore customStore, IDesignerStore designerStore, IStatisticsStore statsStore)
		{
			this.Features = features;
			this.CustomStore = customStore;
			this.DesignerStore = designerStore;
			this.StatisticsStore = statsStore;
		}

		/// <summary>
		/// Initializes the versioner and updates <see cref="ApplicationData"/> to current version.
		/// </summary>
		/// <returns></returns>
		public IAsyncAction Initialize()
		{
			var currVer = ApplicationData.Current.Version;
			return ApplicationData.Current.SetVersionAsync(2, this.VersionHandler);
		}

		private async void VersionHandler(SetVersionRequest request)
		{
			if (request.CurrentVersion != CurrentVersion)
			{
				Logger.Info("Updating application data from version {0} to {1}.", request.CurrentVersion, request.DesiredVersion);
				var deferral = request.GetDeferral();
				if (request.CurrentVersion == 0 || request.CurrentVersion == 1)
					await new V0ToV2(this.Features, this.CustomStore, this.DesignerStore, this.StatisticsStore).Convert();
				deferral.Complete();
			}
		}
	}
}
