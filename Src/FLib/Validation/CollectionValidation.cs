using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FLib
{
	/// <summary>
	/// Validators for collections.
	/// </summary>
	public static class CollectionValidation
	{
		/// <summary>
		/// Value must be in collection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> IsIn<T, Collection>(this Validator<T> validator, Collection collection)
			where Collection : IEnumerable<T>
		{
			validator.Test(v => collection.Contains(v), n => new ArgumentException("Value is not in collection.", n));
			return validator;
		}

		/// <summary>
		/// Value must be in collection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> IsNotIn<T, Collection>(this Validator<T> validator, Collection collection)
			where Collection : IEnumerable<T>
		{
			validator.Test(v => !collection.Contains(v), n => new ArgumentException("Value is in collection.", n));
			return validator;
		}

		/// <summary>
		/// Value must be collection with specified element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> Contains<T, Collection>(this Validator<Collection> validator, T value)
			where Collection : IEnumerable<T>
		{
			validator.Test(v => v.Contains(value), n => new ArgumentException("Collection must contain {0}.".FormatWith(value), n));
			return validator;
		}

		/// <summary>
		/// Value must be collection without specified element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="Collection"></typeparam>
		/// <param name="validator"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<Collection> NotContain<T, Collection>(this Validator<Collection> validator, T value)
			where Collection : IEnumerable<T>
		{
			validator.Test(v => !v.Contains(value), n => new ArgumentException("Collection must not contain {0}.".FormatWith(value), n));
			return validator;
		}

		/// <summary>
		/// Value must be collection with element that matches predicate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<IEnumerable<T>> Contains<T>(this Validator<IEnumerable<T>> validator, Predicate<T> predicate)
		{
			validator.Test(v => v.Contains(predicate), n => new ArgumentException("Collection must contain element that matches specified predicate.", n));
			return validator;
		}

		/// <summary>
		/// Value must be collection without element that matches predicate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<IEnumerable<T>> NotContain<T>(this Validator<IEnumerable<T>> validator, Predicate<T> predicate)
		{
			validator.Test(v => v.NotContain(predicate), n => new ArgumentException("Collection must not contain element that matches specified predicate.", n));
			return validator;
		}
	}
}
