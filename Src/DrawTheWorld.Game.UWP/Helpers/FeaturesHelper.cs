using System;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;

namespace DrawTheWorld.Game.Helpers
{
	static class FeaturesHelper
	{
		/// <summary>
		/// Checks for feature and if something went wrong handles the error.
		/// </summary>
		/// <param name="featureProvider"></param>
		/// <param name="testFunc"></param>
		/// <param name="notifications"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		public static async Task<bool> TestForFeature(this IFeatureProvider featureProvider, Func<IFeatureProvider, Task<bool>> testFunc, Notifications notifications, MetroLog.ILogger logger)
		{
			try
			{
				if (!await testFunc(featureProvider))
				{
					logger.Debug("Aborted.");
					return false;
				}
			}
			catch (Exception ex)
			{
				logger.Error("Cannot buy feature, aborting.", ex);
				notifications.Notify(new Core.UI.Messages.ErrorMessage());
				return false;
			}
			return true;
		}
	}
}
