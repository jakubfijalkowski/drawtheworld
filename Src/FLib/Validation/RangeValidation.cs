using System;
using System.Diagnostics;

namespace FLib
{
	/// <summary>
	/// Validators for <see cref="IComparable{T}"/> that checks if object is in range.
	/// </summary>
	public static class RangeValidation
	{
		/// <summary>
		/// Value must be at least equal to specified.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="min"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> Min<T>(this Validator<T> validator, T min)
			where T : IComparable<T>
		{
			validator.Test(v => v.CompareTo(min) >= 0, n => new ArgumentOutOfRangeException(n, "Value must be at least equal {0}.".FormatWith(min)));
			return validator;
		}

		/// <summary>
		/// Value must be in range. Inclusive.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> Max<T>(this Validator<T> validator, T max)
			where T : IComparable<T>
		{
			validator.Test(v => v.CompareTo(max) <= 0, n => new ArgumentOutOfRangeException(n, "Value must be at most equal {0}.".FormatWith(max)));
			return validator;
		}

		/// <summary>
		/// Value must be in range. Inclusive.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> InRange<T>(this Validator<T> validator, T min, T max)
			where T : IComparable<T>
		{
			validator.Test(v => v.CompareTo(min) >= 0 && v.CompareTo(max) <= 0, n => new ArgumentOutOfRangeException(n, "Value must be in range {0}..{1}.".FormatWith(min, max)));
			return validator;
		}
	}
}
