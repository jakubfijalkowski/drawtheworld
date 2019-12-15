using System;
using System.Diagnostics;

namespace FLib
{
	/// <summary>
	/// Validators for <see cref="Type"/> and object's type.
	/// </summary>
	public static class TypeValidation
	{
		/// <summary>
		/// Type must derive from specified class.
		/// </summary>
		/// <param name="validator"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Type> HasInBase(this Validator<Type> validator, Type type)
		{
			validator.Test(v => v.HasInBase(type), n => new ArgumentException("Type must derive from {0}.".FormatWith(type.FullName), n));
			return validator;
		}

		/// <summary>
		/// Type must implement specified interface.
		/// </summary>
		/// <param name="validator"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Type> Implements(this Validator<Type> validator, Type type)
		{
			validator.Test(v => v.Implements(type), n => new ArgumentException("Type must implement {0}.".FormatWith(type.FullName), n));
			return validator;
		}

		/// <summary>
		/// Object's type must derive from specified class.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> IsOfType<T>(this Validator<T> validator, Type type)
		{
			validator.Test(v => v.GetType().HasInBase(type), n => new ArgumentException("Object must derive from {0}.".FormatWith(type.FullName), n));
			return validator;
		}

		/// <summary>
		/// Object's type must implement specified interface.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> Implements<T>(this Validator<T> validator, Type type)
		{
			validator.Test(v => v.GetType().Implements(type), n => new ArgumentException("Type must implement {0}.".FormatWith(type.FullName), n));
			return validator;
		}
	}
}
