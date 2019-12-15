using System;
using System.Reflection;

#if WINRT
using FLib;
#endif

namespace DrawTheWorld.Core.Attributes
{
	/// <summary>
	/// Assigns specified tool a predefined name(mode-independent).
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class ToolNameAttribute
		: Attribute
	{
		private readonly string _Name = string.Empty;

		/// <summary>
		/// Name.
		/// </summary>
		public string Name
		{
			get { return this._Name; }
		}

		/// <summary>
		/// Initializes attribute.
		/// </summary>
		/// <param name="name"></param>
		public ToolNameAttribute(string name)
		{
			this._Name = name;
		}

		/// <summary>
		/// Gets name from selected type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string GetToolName(Type type)
		{
			var attrib = type.GetCustomAttribute<ToolNameAttribute>(false);
			return attrib != null ? attrib.Name : string.Empty;
		}
	}
}
