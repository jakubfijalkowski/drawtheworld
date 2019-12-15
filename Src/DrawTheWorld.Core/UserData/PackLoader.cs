using System;
using System.IO;
using FLib;

namespace DrawTheWorld.Core.UserData
{
	/// <summary>
	/// Provides way to load/save pack despite of its version.
	/// </summary>
	/// <remarks>
	/// Loaders should not validate data - it will be validated separately. If data is invalid(eg. missing), it should be replaced with reasonable default.
	/// </remarks>
	public static class PackLoader
	{
		/// <summary>
		/// Loads pack from the stream.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Pack Load(Stream input, Guid id = default(Guid))
		{
			Validate.Debug(() => input, v => v.NotNull().That(d => d.CanRead, v2 => v2.True()));

			return Loaders.XmlLoader.Load(new StreamReader(input), id);
		}

		/// <summary>
		/// Loads pack from the stream.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Pack Load(TextReader input, Guid id = default(Guid))
		{
			Validate.Debug(() => input, v => v.NotNull());

			return Loaders.XmlLoader.Load(input, id);
		}

		/// <summary>
		/// Saves pack to the stream using latest format.
		/// </summary>
		/// <param name="pack"></param>
		/// <param name="output"></param>
		public static void Save(Pack pack, Stream output)
		{
			Validate.Debug(() => pack, v => v.NotNull());
			Validate.Debug(() => output, v => v.NotNull().That(d => d.CanWrite, v2 => v2.True()));

			Loaders.XmlLoader.Save(pack, output);
		}
	}

	/// <summary>
	/// Represents error that occured during pack load.
	/// </summary>
	public class PackLoadException
		: Exception
	{
		/// <summary>
		/// 
		/// </summary>
		public PackLoadException()
		{ }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		public PackLoadException(string msg)
			: base(msg)
		{ }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="inner"></param>
		public PackLoadException(string msg, Exception inner)
			: base(msg, inner)
		{ }
	}
}
