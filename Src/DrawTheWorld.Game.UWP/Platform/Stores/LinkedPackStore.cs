using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DrawTheWorld.Core.Platform;
using Windows.Storage;

namespace DrawTheWorld.Game.Platform.Stores
{
	/// <summary>
	/// <see cref="ILinkedPackStore"/> implementation that uses local settings.
	/// </summary>
	sealed class LinkedPackStore
		: ILinkedPackStore
	{
		private const string ContainerName = "LinkedPacks";

		internal static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.LinkedPackStore");

		private ThreadLocal<ApplicationDataContainer> Container = new ThreadLocal<ApplicationDataContainer>(() =>
			ApplicationData.Current.LocalSettings.CreateContainer(ContainerName, ApplicationDataCreateDisposition.Always), false);

		public void Add(Guid packId)
		{
			this.Container.Value.Values.Add(packId.ToString("N"), string.Empty);
		}

		public void Remove(Guid packId)
		{
			this.Container.Value.Values.Remove(packId.ToString("N"));
		}

		public IEnumerable<Guid> Load()
		{
			return this.Container.Value.Values.Keys
				.Select(p =>
				{
					Guid output;
					if (!Guid.TryParseExact(p, "N", out output))
						return (Guid?)null;
					return (Guid?)output;
				})
				.Where(g => g.HasValue)
				.Select(g => g.Value);
		}
	}
}
