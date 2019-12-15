using System;
using System.Collections;
using System.Diagnostics;

namespace FLib
{
	/// <summary>
	/// String's length/collection elements' count validators.
	/// </summary>
	public static class LengthValidation
	{
		/// <summary>
		/// Minimum number of elements. Inclusive.
		/// </summary>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> MinCount<Collection>(this Validator<Collection> validator, int len)
			where Collection : ICollection
		{
			validator.Test(v => v.Count >= len, n => new ArgumentOutOfRangeException(n, "Collection must have at least {0} elements.".FormatWith(len)));
			return validator;
		}

		/// <summary>
		/// Maximum number of elements. Inclusive.
		/// </summary>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> MaxCount<Collection>(this Validator<Collection> validator, int len)
			where Collection : ICollection
		{
			validator.Test(v => v.Count <= len, n => new ArgumentOutOfRangeException(n, "Collection must have at most {0} elements.".FormatWith(len)));
			return validator;
		}

		/// <summary>
		/// Elements' count is equal to specified.
		/// </summary>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> CountEquals<Collection>(this Validator<Collection> validator, int len)
			where Collection : ICollection
		{
			validator.Test(v => v.Count == len, n => new ArgumentOutOfRangeException(n, "Collection must have exactly {0} elements.".FormatWith(len)));
			return validator;
		}

		/// <summary>
		/// Elements' count is in range. Inclusive.
		/// </summary>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> CountInRange<Collection>(this Validator<Collection> validator, int min, int max)
			where Collection : ICollection
		{
			validator.Test(v => v.Count >= min && v.Count <= max, n => new ArgumentOutOfRangeException(n, "Collection must have at least {0} and at most {1} elements.".FormatWith(min, max)));
			return validator;
		}

		/// <summary>
		/// Minimum length. Inclusive.
		/// </summary>
		/// <param name="validator"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<string> MinLength(this Validator<string> validator, int len)
		{
			validator.Test(v => v.Length >= len, n => new ArgumentOutOfRangeException(n, "String must be at least {0} characters long.".FormatWith(len)));
			return validator;
		}

		/// <summary>
		/// Maximum length. Inclusive.
		/// </summary>
		/// <param name="validator"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<string> MaxLength(this Validator<string> validator, int len)
		{
			validator.Test(v => v.Length <= len, n => new ArgumentOutOfRangeException(n, "String must be at most {0} characters long.".FormatWith(len)));
			return validator;
		}

		/// <summary>
		/// String's length is equal to specified.
		/// </summary>
		/// <param name="validator"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<string> LengthEquals(this Validator<string> validator, int len)
		{
			validator.Test(v => v.Length == len, n => new ArgumentOutOfRangeException(n, "String must be exactly {0} characters long.".FormatWith(len)));
			return validator;
		}

		/// <summary>
		/// String's length is in range. Inclusive.
		/// </summary>
		/// <param name="validator"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<string> LengthInRange(this Validator<string> validator, int min, int max)
		{
			validator.Test(v => v.Length >= min && v.Length <= max, n => new ArgumentOutOfRangeException(n, "String must have at least {0} and at most {1} characters.".FormatWith(min, max)));
			return validator;
		}
	}
}
