using System;
using System.Threading.Tasks;
using FLib;
using MetroLog;

namespace DrawTheWorld.Core.Helpers
{
	/// <summary>
	/// MetroLog helper.
	/// </summary>
	public static class MetroLogHelper
	{
		/// <summary>
		/// Missing method from NLog.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="msgCreator"></param>
		public static void Trace(this ILogger logger, Func<string> msgCreator)
		{
			if (logger.IsTraceEnabled)
				logger.Trace(msgCreator());
		}

		/// <summary>
		/// Logs fatal message and waits for finishing the write.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="ex"></param>
		public static void FatalSync(this ILogger logger, string message, Exception ex)
		{
			Validate.Debug(() => logger, v => v.Implements(typeof(ILoggerAsync)));
			Task.Run(async () => await ((ILoggerAsync)logger).FatalAsync(message, ex)).Wait();
		}
	}
}
