using FLib;
using MetroLog;
// using MetroLog.Layouts;
// using MetroLog.Targets;

namespace DrawTheWorld.Game
{
	/// <summary>
	/// Enables logger in designer.
	/// </summary>
	static class LogManager
	{
//		static LogManager()
//		{
//			var cfg = new LoggingConfiguration();

//			var fileTarget = new FileStreamingTarget(new LoggerLayout());
//			fileTarget.FileNamingParameters.IncludeSession = true;
//			fileTarget.RetainDays = 5;

//#if DEBUG
//			cfg.AddTarget(LogLevel.Trace, LogLevel.Fatal, fileTarget);
//			cfg.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());
//#else
//			cfg.AddTarget(LogLevel.Info, LogLevel.Fatal, fileTarget);
//#endif

//			cfg.AddTarget(LogLevel.Error, LogLevel.Fatal, new Utilities.BugReportFileTarget(new LoggerLayout()));

//			LogManagerFactory.DefaultConfiguration = cfg;
//		}

		/// <summary>
		/// Returns logger with specified name, or null if in design mode(designer works properly).
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MetroLog.ILogger GetLogger(string name)
		{
#if DEBUG
			if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
				return null;
#endif
			return LogManagerFactory.DefaultLogManager.GetLogger(name);
		}

		//sealed class LoggerLayout
		//	: Layout
		//{
		//	/// <inheritdoc />
		//	public override string GetFormattedString(LogWriteContext context, LogEventInfo info)
		//	{
		//		var str = "[{0:HH\\:mm\\:ss}][{1}][{2}] {3}".FormatWith(info.TimeStamp, info.Logger, info.Level, info.Message);
		//		if (info.Exception != null)
		//			str += "\r\n   Exception: " + info.ExceptionWrapper.AsString;
		//		return str;
		//	}
		//}
	}
}
