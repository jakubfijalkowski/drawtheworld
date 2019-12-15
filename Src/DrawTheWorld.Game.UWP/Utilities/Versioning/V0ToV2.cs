using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UserData;
using FLib;
using Windows.Storage;

using BoardNameToId = System.Collections.Generic.Dictionary<string, System.Guid>;

namespace DrawTheWorld.Game.Utilities.Versioning
{
	using PackToBoards = Dictionary<Guid, BoardNameToId>;

	sealed class V0ToV2
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.Versioning.V0ToV1");

		private readonly IFeatureProvider Features = null;
		private readonly ICustomPackStore CustomStore = null;
		private readonly IDesignerStore DesignerStore = null;
		private readonly IStatisticsStore StatisticsStore = null;

		public V0ToV2(IFeatureProvider features, ICustomPackStore customStore, IDesignerStore designerStore, IStatisticsStore statsStore)
		{
			this.Features = features;
			this.CustomStore = customStore;
			this.DesignerStore = designerStore;
			this.StatisticsStore = statsStore;
		}

		public async Task Convert()
		{
			var store = ApplicationData.Current;
			await store.ClearAsync(ApplicationDataLocality.Roaming);

			#region Temporary directory
			try
			{
				Logger.Trace("Clearing temporary store.");
				await store.ClearAsync(ApplicationDataLocality.Temporary);
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot clear temporary store. Ignoring.", ex);
			}
			#endregion

			#region Thumbnails
			try
			{
				Logger.Trace("Clearing thumbnails.");
				var normalStore = await (await store.LocalFolder.GetFolderAsync("Packs")).GetFolderAsync("Thumbs");
				await Utils.ClearFolder(normalStore);

				var designerStore = await (await store.LocalFolder.GetFolderAsync("Designer packs")).GetFolderAsync("Thumbs");
				await Utils.ClearFolder(designerStore);

			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot clear thumbnails. Ignoring.", ex);
			}
			#endregion

			#region Pack stores
			if (this.Features.CheckFeature(Feature.PackInstall))
			{
				Logger.Info("Migrating custom packs.");
				await MigrateStoreFromV0("Packs", this.CustomStore, V0ToV2Data.DefaultPacks);
			}
			else
			{
				Logger.Info("PackInstall feature is not available. Ignoring packs.");
			}

			if (this.Features.CheckFeature(Feature.Designer))
			{
				Logger.Info("Migrating designer packs.");
				await MigrateStoreFromV0("Designer packs", this.DesignerStore);

				try
				{
					await Utils.ClearFolder(await store.LocalFolder.GetFolderAsync("Designer packs"));
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot delete designer store. Ignoring.", ex);
				}
			}
			else
			{
				Logger.Info("Designer feature is not available. Ignoring packs.");
			}
			#endregion

			await this.MigrateStatsFromV0();
		}

		private static async Task MigrateStoreFromV0(string storeName, IPackStore store, IEnumerable<Guid> packExcludeList = null)
		{
			IEnumerable<StorageFile> files = null;
			try
			{
				var packsStore = await ApplicationData.Current.LocalFolder.GetFolderAsync(storeName);
				files = (await packsStore.GetFilesAsync()).Where(f => f.Name.EndsWith(".dtw"));
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot migrate packs from store '{0}'. Ignoring.".FormatWith(storeName), ex);
				return;
			}

			foreach (var file in files)
			{
				try
				{
					var id = Utils.ParsePackId(file.Name);
					if (packExcludeList == null || !packExcludeList.Contains(id))
					{
						using (var stream = await file.OpenStreamForReadAsync())
						{
							var pack = await Task.Run(() => PackLoader.Load(stream, id));
							V0ToV2Data.AddPack(pack);
							await store.SavePack(id, s => PackLoader.Save(pack, s));
							Logger.Debug("Pack '{0}'({1}) imported.", pack.Name.DefaultTranslation, pack.Id);
						}
					}
					else
					{
						Logger.Debug("Ignoring pack with id {0}.", id);
					}
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot migrate pack {0}. Ignoring.".FormatWith(file.Name), ex);
				}

				try
				{
					await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot delete file {0}. Ignoring.".FormatWith(file.Name), ex);
				}
			}
		}

		private async Task MigrateStatsFromV0()
		{
			ApplicationDataContainer container;
			try
			{
				container = ApplicationData.Current.LocalSettings.CreateContainer("Stats", ApplicationDataCreateDisposition.Existing);
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot open stats store statistics. Ignoring.", ex);
				return;
			}

			Logger.Trace("Migrating user statistics.");
			var list = await Task.Run(() =>
			{
				List<BoardStatistics> statsList = new List<BoardStatistics>();
				foreach (var item in container.Values)
				{
					Guid packId;
					if (!Guid.TryParse(item.Key, out packId))
					{
						Logger.Warn("Cannot parse statistics for item {0}.", item.Key);
						continue;
					}
					var composite = (ApplicationDataCompositeValue)item.Value;
					foreach (var boardItem in composite)
					{
						int cnt = 0;
						try
						{
							var boardId = V0ToV2Data.List[packId][boardItem.Key];
							var data = (byte[])boardItem.Value;
							for (int i = 0; i < data.Length; i += 8 + 4 + 1)
							{
								var date = DateTime.FromBinary(BitConverter.ToInt64(data, i)).ToUniversalTime();
								var time = BitConverter.ToInt32(data, i + 8);
								var reason = (FinishReason)data[i + 8 + 4];
								statsList.Add(new BoardStatistics(packId, boardId, time, date, reason));
								++cnt;
							}
						}
						catch (Exception ex)
						{
							Logger.Warn("Cannot decode stats for board '{0}' from pack '{1}'. Aborting.".FormatWith(boardItem.Key, packId), ex);
						}
						Logger.Debug("{0} stats for board '{1}' from pack {2} imported.", cnt, boardItem.Key, packId);
					}
				}
				return statsList;
			});
			foreach (var item in list)
				await this.StatisticsStore.Save(item);

			try
			{
				ApplicationData.Current.LocalSettings.DeleteContainer(container.Name);
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot delete old stats container. Ignoring.", ex);
			}
		}
	}

	static class V0ToV2Data
	{
		public static readonly Guid[] DefaultPacks = new Guid[]
		{
			Guid.ParseExact("45E46B0BE8DA40A293A2DE1D8A9568F5", "N"), Guid.ParseExact("24829A684FB240EEA086954FF401ABAC", "N"),
			Guid.ParseExact("63557A550AF94508844864761F79B68F", "N"), Guid.ParseExact("B4E2FA5CA6EC43F5BFD3CD9EA3E9A6B9", "N")
		};

		public static readonly PackToBoards List = new PackToBoards
		{
			{ Guid.ParseExact("b4e2fa5ca6ec43f5bfd3cd9ea3e9a6b9", "N"), new BoardNameToId
				{
					{ "Knife", Guid.ParseExact("f5560feb15034259a652b5409fc0f6b3", "N") },
					{ "Fork", Guid.ParseExact("2d9cc63d380d4993871fb25b98e16ed2", "N") },
					{ "Spoon", Guid.ParseExact("7c2a7ff29d98428da8ca119c04999f0c", "N") },
					{ "Key", Guid.ParseExact("a6f55e2c082d4cc5b6e2c5726542be22", "N") },
					{ "Umbrella", Guid.ParseExact("d107b63ab41a454793006e973dd9bd74", "N") },
					{ "Barbell", Guid.ParseExact("1e07a08af62749aaad7ddd32cd57b45a", "N") },
					{ "Heart", Guid.ParseExact("35994b741e794e78bf243fa99bbb04b6", "N") },
					{ "Cup of tea", Guid.ParseExact("a6f02545909949cea99eca615ebae2a0", "N") },
					{ "Small tree", Guid.ParseExact("4d9f472ceead4c96bfa94fe820fc5779", "N") },
					{ "Human", Guid.ParseExact("6f3299d9cabb49289cfb4f68ab636120", "N") },
					{ "Cloud", Guid.ParseExact("1ad32fc344c441b7a532fd2bdf8927ae", "N") },
					{ "House", Guid.ParseExact("336aaf61f9364cbca1557909cff076fa", "N") },
					{ "Note", Guid.ParseExact("cbeb29a8c4e04d25b54f11367f3bcd39", "N") },
					{ "Headphones", Guid.ParseExact("52da5a7ed17140d9b1146769dee61cb2", "N") },
				}
			},
			{ Guid.ParseExact("24829a684fb240eea086954ff401abac", "N"), new BoardNameToId
				{
					{ "Mushroom", Guid.ParseExact("d3df635588a94a769bc0a3db459723d3", "N") },
					{ "Apple", Guid.ParseExact("6fcac6517d2f4052942c99e2e9f6b338", "N") },
					{ "Tulip", Guid.ParseExact("4f3a5dde255d43c1a0b9160d092c9691", "N") },
					{ "Cactus", Guid.ParseExact("cf7722a411484a6a9ccfc2d43a27bde1", "N") },
					{ "Tree", Guid.ParseExact("458ef76d9c4946e39528b85fd6289143", "N") },
					{ "Christmas Tree", Guid.ParseExact("39341550eeea4a88b9f176bf8916ac6f", "N") },
					{ "Fish", Guid.ParseExact("4e7e6dcdfb1447d2821de1d444b041cb", "N") },
					{ "Crab", Guid.ParseExact("7dffea7a7af748f79aae7fa8960b9031", "N") },
					{ "Lemon", Guid.ParseExact("609c639acb7c42b19f1babaa0c5e567c", "N") },
					{ "Banana", Guid.ParseExact("b5d875669f00431a86ba95e5350fbee3", "N") },
					{ "Cherries", Guid.ParseExact("7aeba4f96bd34616a1173f513039da81", "N") },
					{ "Carrot", Guid.ParseExact("1fa5da7947db46b7b973467053e6dbe6", "N") },
					{ "Orange", Guid.ParseExact("0696ba3a83644336bc98c3cbff7ac22e", "N") },
					{ "Strawberry", Guid.ParseExact("c1117ffaf88b4d64bd4d2220bc30b470", "N") },
					{ "Tomato", Guid.ParseExact("5e0377af54464a7b90082d243066cd07", "N") },
					{ "Peach", Guid.ParseExact("ef02b560adeb46128c9051dbcff50820", "N") },
					{ "Turtle", Guid.ParseExact("f850f5d49e9e4471a9f8663dba611356", "N") },
				}
			},
			{ Guid.ParseExact("45e46b0be8da40a293a2de1d8a9568f5", "N"), new BoardNameToId
				{
					{ "Mario", Guid.ParseExact("3df73a33cf8e476eafbfd274d21fa1e8", "N") },
					{ "Luigi", Guid.ParseExact("93b1da35f7cf40ef814f9f8249728229", "N") },
					{ "Popeye", Guid.ParseExact("0f31b753be6d4a808c6a4769a1489139", "N") },
					{ "Bomberman", Guid.ParseExact("0baed372fe5b4f43872b24a60784642c", "N") },
					{ "Adventure Island", Guid.ParseExact("2925b5c979654264a7718c5209c3756d", "N") },
					{ "Excitebike", Guid.ParseExact("0c83460a7c5c4eeab58c491ded98f052", "N") },
					{ "Zelda - Link", Guid.ParseExact("472edb133f8149ea821f4c0181847b37", "N") },
					{ "PacMan", Guid.ParseExact("5473ea298ee449c78f589914d6737424", "N") },
					{ "Binary Land", Guid.ParseExact("14dbd67361734339a780c79a5d8cb794", "N") },
					{ "Duck", Guid.ParseExact("16dfd306e1e44defbfc7235636c73f7b", "N") },
					{ "Megaman", Guid.ParseExact("264330422ef2497fa5a3871f7de621ee", "N") },
					{ "Contra", Guid.ParseExact("a8d7bf6e4f7344ee9cbaea7595d01f53", "N") },
					{ "Dale", Guid.ParseExact("731a8da8ee4c424f8d49bec84225ca4c", "N") },
					{ "Chip", Guid.ParseExact("12492796ad284e1fbfd2d6148a65039a", "N") },
				}
			},
			{ Guid.ParseExact("63557a550af94508844864761f79b68f", "N"), new BoardNameToId
				{
					{ "Torch", Guid.ParseExact("4bb0b6b57a3640df88e971ae2c77296d", "N") },
					{ "Candle", Guid.ParseExact("13dd432e062e4cae9fd80d53d740bbf6", "N") },
					{ "Goblet", Guid.ParseExact("8cbf41bf4d324a6d9d91089921af5e31", "N") },
					{ "Potion", Guid.ParseExact("4a6ba15d68144d5c9604d8e80958cefb", "N") },
					{ "Sword", Guid.ParseExact("502eaad4a4bd4da6984abc9026fe365a", "N") },
					{ "Bow", Guid.ParseExact("f8e8ba42028d4417adfd166a0b5f813e", "N") },
					{ "Crossbow", Guid.ParseExact("c13dde63bc3a43d5840fe300364f1b87", "N") },
					{ "Catapult", Guid.ParseExact("2939d81a1bec47ecbda21b06c7d969b5", "N") },
					{ "Axe", Guid.ParseExact("684443782d1e437bb425aa7eded6ccc4", "N") },
					{ "Helmet", Guid.ParseExact("fe72f902d5d345c5961da9aa182a6b79", "N") },
					{ "Shield", Guid.ParseExact("ed12ec0e9c914988abcac49f329f85bb", "N") },
					{ "Hammer and Anvil", Guid.ParseExact("b79c503310bc49e9a72f41d4debaaafc", "N") },
					{ "Horse", Guid.ParseExact("74b2b53e52844bc192525ed869770677", "N") },
					{ "Castle", Guid.ParseExact("ce34c09868ad4db6a0e6a18a214cf238", "N") },
				}
			}
		};

		public static void AddPack(Pack pack)
		{
			var item = new BoardNameToId();
			foreach (var b in pack.Boards)
				item.Add(b.Name.DefaultTranslation, b.Id);
			List.Add(pack.Id, item);
		}
	}
}
