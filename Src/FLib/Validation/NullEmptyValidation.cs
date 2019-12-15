using System;
using System.Collections;
using System.Diagnostics;

namespace FLib
{
	/// <summary>
	/// Null/Empty validators.
	/// </summary>
	public static class NullEmptyValidation
	{
		/// <summary>
		/// Argument must not be null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> NotNull<T>(this Validator<T> validator)
		{
			validator.Test(v => v != null, n => new ArgumentNullException(n));
			return validator;
		}

		/// <summary>
		/// Argument must be null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> Null<T>(this Validator<T> validator)
		{
			validator.Test(v => v == null, n => new ArgumentNullException(n));
			return validator;
		}

		/// <summary>
		/// Value must be empty collection.
		/// </summary>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> Empty<Collection>(this Validator<Collection> validator)
			where Collection : IEnumerable
		{
			//Hacky
			validator.Test(v => !v.GetEnumerator().MoveNext(), n => new ArgumentNullException(n, "Collection must be empty."));
			return validator;
		}

		/// <summary>
		/// Value must be empty not collection.
		/// </summary>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> NotEmpty<Collection>(this Validator<Collection> validator)
			where Collection : IEnumerable
		{
			validator.Test(v => v.GetEnumerator().MoveNext(), n => new ArgumentNullException(n, "Collection must not be empty."));
			return validator;
		}

		/// <summary>
		/// Argument must be null or empty.
		/// </summary>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> NullOrEmpty<Collection>(this Validator<Collection> validator)
			where Collection : IEnumerable
		{
			validator.Test(v => v == null || !v.GetEnumerator().MoveNext(), n => new ArgumentNullException(n, "Collection must be null or empty"));
			return validator;
		}

		/// <summary>
		/// Argument must not be null or empty.
		/// </summary>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> NotNullAndNotEmpty<Collection>(this Validator<Collection> validator)
			where Collection : IEnumerable
		{
			validator.Test(v => v != null && v.GetEnumerator().MoveNext(), n => new ArgumentNullException(n, "Collection must be neither null nor empty."));
			return validator;
		}

		/// <summary>
		/// Argument must not be null or whitespace string.
		/// </summary>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<string> NotNullAndNotWhiteSpace(this Validator<string> validator)
		{
			validator.Test(v => !string.IsNullOrWhiteSpace(v), n => new ArgumentNullException(n, "String must be null or whitespace."));
			return validator;
		}


		/// <summary>
		/// Argument must not be null or whitespace string.
		/// </summary>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<string> NullOrWhiteSpace(this Validator<string> validator)
		{
			validator.Test(v => string.IsNullOrWhiteSpace(v), n => new ArgumentNullException(n, "String must be neither null nor whitespace."));
			return validator;
		}
	}
}
