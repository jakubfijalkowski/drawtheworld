using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MetroLog;
//using MetroLog.Layouts;
//using MetroLog.Targets;
using Windows.Storage;

namespace DrawTheWorld.Game.Utilities
{
    /// <summary>
    /// Custom file target for MetroLog that should be used for bug reporting.
    /// </summary>
    //sealed class BugReportFileTarget
    //	: FileTargetBase
    //{
    //	private const string LogFileName = "Bugs.log";
    //	private static readonly TimeSpan RetainTime = TimeSpan.FromDays(5);

    //	private static SemaphoreSlim Lock = new SemaphoreSlim(1);
    //	private static StorageFile LogFile = null;

    //	public BugReportFileTarget(Layout layout)
    //		: base(layout)
    //	{ }

    //	/// <summary>
    //	/// Gets the content of the log.
    //	/// </summary>
    //	/// <returns></returns>
    //	public static async Task<string> GetBugReportContent()
    //	{
    //		using (await AcquireLock())
    //		{
    //			await EnsureInitializedStaticNoLock();
    //			return await FileIO.ReadTextAsync(LogFile);
    //		}
    //	}

    //	/// <summary>
    //	/// Resets the content after successful bug report.
    //	/// </summary>
    //	/// <returns></returns>
    //	public static async Task ResetContent()
    //	{
    //		using (await AcquireLock())
    //		{
    //			await EnsureInitializedStaticNoLock();
    //			await ResetContentInternal();
    //		}
    //	}

    //	protected override Task EnsureInitialized()
    //	{
    //		return EnsureInitializedStatic();
    //	}

    //	protected override async Task DoCleanup(System.Text.RegularExpressions.Regex pattern, DateTime threshold)
    //	{
    //		using (await AcquireLock())
    //		{
    //			var dateModified = (await LogFile.GetBasicPropertiesAsync()).DateModified;
    //			if (DateTime.Now - dateModified >= RetainTime)
    //				await ResetContentInternal();
    //		}
    //	}

    //	private static async Task<StorageFile> EnsureInitializedStatic()
    //	{
    //		using (await AcquireLock())
    //			return await EnsureInitializedStaticNoLock();
    //	}

    //	private static async Task<StorageFile> EnsureInitializedStaticNoLock()
    //	{
    //		if (LogFile == null)
    //		{
    //			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(LogFolderName, CreationCollisionOption.OpenIfExists);
    //			LogFile = await folder.CreateFileAsync(LogFileName, CreationCollisionOption.OpenIfExists);
    //		}
    //		return LogFile;
    //	}

    //	protected override async Task<LogWriteOperation> DoWriteAsync(string fileName, string contents, LogEventInfo entry)
    //	{
    //		using (await AcquireLock())
    //		{
    //			await FileIO.AppendTextAsync(LogFile, contents + Environment.NewLine);
    //			return new LogWriteOperation(this, entry, true);
    //		}
    //	}

    //	protected override async Task<System.IO.Stream> GetCompressedLogsInternal()
    //	{
    //		using (await AcquireLock())
    //		{
    //			var ms = new MemoryStream();
    //			using (var stream = await LogFile.OpenStreamForReadAsync())
    //				await stream.CopyToAsync(ms);
    //			return ms;
    //		}
    //	}

    //	private static async Task ResetContentInternal()
    //	{
    //		using (var stream = await LogFile.OpenAsync(FileAccessMode.ReadWrite))
    //			stream.Size = 0;
    //	}

    //	private static async Task<LockObject> AcquireLock()
    //	{
    //		await Lock.WaitAsync();
    //		return new LockObject();
    //	}

    //	private struct LockObject
    //		: IDisposable
    //	{
    //		public void Dispose()
    //		{
    //			Lock.Release();
    //		}
    //	}
    //}
}
