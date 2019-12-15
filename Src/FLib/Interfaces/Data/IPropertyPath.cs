using System;
using System.ComponentModel;
using System.Reflection;

namespace FLib.Interfaces.Data
{
	/// <summary>
	/// Path to field or property represented in text.
	/// </summary>
	/// <remarks>
	/// Allows access to field/property in more 'extednal data-friendly' way:
	/// <code>root_object.field.property.indexer[1, 2].(...).property</code>
	/// </remarks>
	public interface IPropertyPath
		: INotifyPropertyChanged, ISupportInitialize, IDisposable
	{
		/// <summary>
		/// Target(last) member of path.
		/// </summary>
		MemberInfo TargetMember { get; }

		/// <summary>
		/// Path as text.
		/// </summary>
		string Path { get; }

		/// <summary>
		/// Root object.
		/// </summary>
		object Root { get; set; }

		/// <summary>
		/// Root type.
		/// </summary>
		Type RootType { get; }

		/// <summary>
		/// Value type.
		/// </summary>
		Type ValueType { get; }

		/// <summary>
		/// Value's type converter.
		/// </summary>
		TypeConverter ValueConverter { get; }

		/// <summary>
		/// Current value.
		/// </summary>
		object Value { get; set; }

		/// <summary>
		/// Indicates if path is initialized.
		/// </summary>
		bool Initialized { get; }
	}
}
