using System;
using System.ComponentModel;
using System.Reflection;

namespace FLib.Data.Internals
{
	/// <summary>
	/// Internal interface that represents single level in <see cref="PropertyPath"/>
	/// </summary>
	internal interface IPropertyLevel
	{
		#region Properties
		/// <summary>
		/// Member that this level points on.
		/// </summary>
		MemberInfo TargetMember { get; }

		/// <summary>
		/// Level number.
		/// </summary>
		int Level { get; }

		/// <summary>
		/// Name of current level.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Value type.
		/// </summary>
		Type Type { get; }

		/// <summary>
		/// Current value.
		/// </summary>
		object Value { get; }

		/// <summary>
		/// Method that will be called when <see cref="Value"/> changes.
		/// Parameter: Level.
		/// </summary>
		Action<int> ValueChanged { get; }
		#endregion

		#region Methods
		/// <summary>
		/// Updates value.
		/// </summary>
		/// <param name="root">Root object.</param>
		void UpdateValue(object root);

		/// <summary>
		/// Changes value in object.
		/// </summary>
		/// <param name="to">Object.</param>
		/// <param name="value">New value.</param>
		void SetValue(object to, object value);

		/// <summary>
		/// Registers, if it is possible, Value.PropertyChanged event.
		/// </summary>
		/// <param name="root">Root object.</param>
		void RegisterPropertyChanged(object root);

		/// <summary>
		/// Unregisters prviously registered Value.PropertyChanged event.
		/// </summary>
		/// <param name="root">Root object.</param>
		void UnregisterPropertyChanged(object root);

		/// <summary>
		/// Gets type converter.
		/// </summary>
		/// <returns></returns>
		TypeConverter GetTypeConverter();
		#endregion
	}
}
