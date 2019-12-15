using System;
using System.Reflection;
using FLib;

namespace DrawTheWorld.Core.Attributes
{
	/// <summary>
	/// Binds tool to specified behavior.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class BindBehaviorAttribute
		: Attribute
	{
		private readonly Type _Behavior;

		/// <summary>
		/// Behavior.
		/// </summary>
		public Type Behavior
		{
			get { return this._Behavior; }
		}

		/// <summary>
		/// Initializes attribute.
		/// </summary>
		/// <param name="behavior"></param>
		public BindBehaviorAttribute(Type behavior)
		{
			Validate.Debug(() => behavior, v => v.That(t => t.Implements(typeof(IToolBehavior))));
			this._Behavior = behavior;
		}

		/// <summary>
		/// Gets the behavior from selected type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type GetBehaviorType(Type type)
		{
			var attrib = type.GetCustomAttribute<BindBehaviorAttribute>(false);
			return attrib != null ? attrib.Behavior : null;
		}
	}
}
