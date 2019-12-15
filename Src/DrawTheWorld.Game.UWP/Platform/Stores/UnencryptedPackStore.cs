using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using FLib;
using Windows.Storage;

namespace DrawTheWorld.Game.Platform.Stores
{
    /// <summary>
    /// Pack store that does not encrypts the pack content.
    /// </summary>
    sealed class UnencryptedPackStore
        : IDemoPackStore, ICustomPackStore, IDesignerStore
    {
        private readonly Guid[] AllowedDemoPacks = new Guid[]
        {
            Guid.ParseExact("05E27C5CEFEA4488AEAAC944D41AD059", "N"),
            Guid.ParseExact("24829A684FB240EEA086954FF401ABAC", "N"),
            Guid.ParseExact("45E46B0BE8DA40A293A2DE1D8A9568F5", "N"),
            Guid.ParseExact("63557A550AF94508844864761F79B68F", "N"),
            Guid.ParseExact("70D8911D00F64E19ABFAFDFEAA26FA22", "N"),
            Guid.ParseExact("B4E2FA5CA6EC43F5BFD3CD9EA3E9A6B9", "N"),
            Guid.ParseExact("D4A07C50548C449D9DAE473E70CD00FB", "N"),
        };

        private const int RetryCount = 5;
        private const int RetryDelay = 500;

        private readonly MetroLog.ILogger Logger;
        private readonly StoreType StoreType;
        private StorageFolder Folder = null;

        private readonly IFeatureProvider FeatureProvider = null;

        /// <summary>
        /// Initializes the store.
        /// </summary>
        /// <param name="storeType"></param>
        public UnencryptedPackStore(StoreType storeType, IFeatureProvider featureProvider)
        {
            this.StoreType = storeType;
            this.FeatureProvider = featureProvider;

            Logger = LogManager.GetLogger("Game.UnencryptedPackStore." + this.StoreType.ToString());
        }

        /// <inheritdoc />
        public async Task LoadPacks(Func<Stream, Guid, Task> installCallback)
        {
            if (!this.CheckPreconditions())
                return;

            await this.LoadFolder();

            var files = (await this.Folder.GetFilesAsync()).Where(f => f.Name.EndsWith(".dtw"));
            foreach (var p in files)
            {
                bool remove = false;

                try
                {
                    var packId = Guid.ParseExact(p.Name.Remove(p.Name.Length - 4), "N");
                    if (this.StoreType == Stores.StoreType.Demo && !AllowedDemoPacks.Contains(packId))
                        throw new InvalidOperationException("Invalid pack in the demo store!");

                    using (var stream = await p.OpenStreamForReadAsync())
                    {
                        if (stream.Length > Core.Config.MaxPackFileSize)
                            throw new InvalidDataException("Pack size is greater than Config.MaxPackFileSize.");

                        await installCallback(stream, packId);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Cannot load pack(file: {0}).".FormatWith(p.Name), ex);
                    remove = true;
                }

                if (remove)
                {
                    try
                    {
                        await p.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Cannot delete invalid pack(file: {0}).".FormatWith(p.Name), ex);
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task RemovePack(Guid id)
        {
            if (!this.CheckPreconditions())
                return;

            await this.LoadFolder();

            try
            {
                await (await this.Folder.GetFileAsync(Settings.PackName(id))).DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex)
            {
                Logger.Warn("Cannot remove pack {0}.".FormatWith(id), ex);
            }
        }

        /// <inheritdoc />
        public async Task SavePack(Guid id, Action<Stream> saveCallback)
        {
            if (!this.CheckPreconditions())
                return;

            await this.LoadFolder();

            var file = await this.OpenPack(id);
            using (var transaction = await file.OpenTransactedWriteAsync())
            {
                transaction.Stream.Size = 0;
                await Task.Run(() => saveCallback(transaction.Stream.AsStreamForWrite()));
                await transaction.CommitAsync();
            }
        }

        private async Task LoadFolder()
        {
            if (this.Folder == null)
                this.Folder = await Settings.OpenPackStore(this.StoreType);
        }

        private async Task<StorageFile> OpenPack(Guid id)
        {
            StorageFile file = null;
            string filename = Settings.PackName(id);
            if (this.StoreType == StoreType.Designer)
            {
                int retries = 0;
                while (file == null)
                {
                    if (retries > 0)
                        await Task.Delay(RetryDelay);
                    try
                    {
                        file = await this.Folder.CreateFileAsync(filename, this.StoreType == Stores.StoreType.Custom ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Cannot open pack {0}.".FormatWith(id), ex);
                        if (retries++ == RetryCount)
                            throw;
                    }
                }
            }
            else
                file = await this.Folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            return file;
        }

        private bool CheckPreconditions()
        {
            switch (this.StoreType)
            {
                case StoreType.Designer:
                    return this.FeatureProvider.CheckFeature(Feature.Designer);
                case StoreType.Custom:
                    return this.FeatureProvider.CheckFeature(Feature.PackInstall);
            }
            return true;
        }
    }
}
