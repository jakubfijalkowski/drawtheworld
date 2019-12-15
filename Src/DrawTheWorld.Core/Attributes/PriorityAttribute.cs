using System;
using System.Reflection;

#if WINRT
using FLib;
#endif

namespace DrawTheWorld.Core.Attributes
{
	/// <summary>
	/// Attribute used to set priority on classes(for displaying in UI).
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class PriorityAttribute
		: Attribute
	{
		private readonly int _Priority = 0;

		/// <summary>
		/// Assigned priority.
		/// </summary>
		public int Priority
		{
			get { return this._Priority; }
		}

		/// <summary>
		/// Initializes attribute.
		/// </summary>
		/// <param name="priority"></param>
		public PriorityAttribute(int priority)
		{
			this._Priority = priority;
		}

		/// <summary>
		/// Gets the priority from selected type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static int GetPriority(Type type)
		{
			var attrib = type.GetCustomAttribute<PriorityAttribute>(false);
			return attrib != null ? attrib.Priority : int.MaxValue;
		}
	}
}
