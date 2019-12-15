using System;
using System.Diagnostics;

namespace FLib
{
	/// <summary>
	/// Equality validators.
	/// </summary>
	public static class EqualValidation
	{
		/// <summary>
		/// Values must not be equal.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> NotEqual<T>(this Validator<T> validator, T value)
		{
			validator.Test(v => !v.Equals(value), n => new ArgumentException("Argument must not be equal to {0}.".FormatWith(value), n));
			return validator;
		}

		/// <summary>
		/// Values must be equal.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> Equal<T>(this Validator<T> validator, T value)
		{
			validator.Test(v => v.Equals(value), n => new ArgumentException("Parameter must be equal to {0}".FormatWith(value), n));
			return validator;
		}

		/// <summary>
		/// Value must be true.
		/// </summary>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<bool> True(this Validator<bool> validator)
		{
			validator.Test(v => v == true, n => new ArgumentException("Parameter must be true.", n));
			return validator;
		}

		/// <summary>
		/// Value must be false.
		/// </summary>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<bool> False(this Validator<bool> validator)
		{
			validator.Test(v => v == false, n => new ArgumentException("Parameter must be false.", n));
			return validator;
		}
	}
}
