using System;

namespace FLib.Interfaces.Data
{
	/// <summary>
	/// Binding synchronization mode.
	/// </summary>
	public enum BindingMode
	{
		/// <summary>
		/// One way - from source to target.
		/// </summary>
		OneWay,

		/// <summary>
		/// Both ends are synchronized.
		/// </summary>
		TwoWay,

		/// <summary>
		/// Sets target value only once, just after binding.
		/// </summary>
		OneTime
	}

	/// <summary>
	/// Allows to connect two <see cref="FLib.Interfaces.Data.IPropertyPath"/> and synchronization of theirs values.
	/// Works just like WPF's bindings, but is independent from WPF.
	/// </summary>
	public interface IBinding
		: IDisposable
	{
		/// <summary>
		/// Binding mode.
		/// </summary>
		BindingMode Mode { get; }

		/// <summary>
		/// Source object.
		/// </summary>
		object Source { get; }

		/// <summary>
		/// Path to source.
		/// </summary>
		IPropertyPath SourcePath { get; }

		/// <summary>
		/// Target object.
		/// </summary>
		object Target { get; }

		/// <summary>
		/// Path to target.
		/// </summary>
		IPropertyPath TargetPath { get; }

		/// <summary>
		/// Converter that is used during synchronization.
		/// </summary>
		Type ConverterType { get; }
	}
}
