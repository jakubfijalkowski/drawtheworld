using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace FLib
{
	/// <summary>
	/// User specified validators.
	/// </summary>
	public static class CustomValidation
	{
		#region Custom Validators
		/// <summary>
		/// Custom validator.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="condition"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> That<T>(this Validator<T> validator, Func<T, bool> condition)
		{
			validator.Test(condition, n => new ArgumentException("Argument doesn't satisfy condition.", n));
			return validator;
		}

		/// <summary>
		/// Custom validator.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="condition"></param>
		/// <param name="errorMsg"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> That<T>(this Validator<T> validator, Func<T, bool> condition, string errorMsg)
		{
			validator.Test(condition, n => new ArgumentException(errorMsg, n));
			return validator;
		}

		/// <summary>
		/// Custom validator.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="validator"></param>
		/// <param name="condition"></param>
		/// <param name="exceptionCreator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> That<T>(this Validator<T> validator, Func<T, bool> condition, Func<string, Exception> exceptionCreator)
		{
			validator.Test(condition, exceptionCreator);
			return validator;
		}
		#endregion

		#region Nested
		/// <summary>
		/// Allows to validate property using <see cref="Validator{K}"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <typeparam name="K">Object type</typeparam>
		/// <param name="validator"></param>
		/// <param name="arg"></param>
		/// <param name="validationCode"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> That<T, K>(this Validator<T> validator, Expression<Func<T, K>> arg, Action<Validator<K>> validationCode)
		{
			var v2 = validator.Nest(arg);
			validationCode(v2);
			return validator;
		}

		/// <summary>
		/// Allows to validate property using <see cref="Validator{K}"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <typeparam name="K">Object type</typeparam>
		/// <param name="validator"></param>
		/// <param name="argName">Argument name.</param>
		/// <param name="arg">Argument getter.</param>
		/// <param name="validationCode">Validation code.</param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> That<T, K>(this Validator<T> validator, string argName, Func<T, K> arg, Action<Validator<K>> validationCode)
		{
			validator.Test(v => validationCode(new Validator<K>(validator.Name + "." + argName, arg(v))));
			return validator;
		}

		/// <summary>
		/// Allows to validate property using <see cref="Validator{K}"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <typeparam name="K">Object type</typeparam>
		/// <param name="validator"></param>
		/// <param name="argName">Argument name.</param>
		/// <param name="arg">Object to be validated.</param>
		/// <param name="validationCode">Validation code.</param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> That<T, K>(this Validator<T> validator, string argName, K arg, Action<Validator<K>> validationCode)
		{
			validationCode(new Validator<K>(validator.Name + "." + argName, arg));
			return validator;
		}

		/// <summary>
		/// Allows to validate property using <see cref="Validator{T}"/> of <see cref="IEnumerable{K}"/>.
		/// Returned validator is of type <see cref="IEnumerable{K}"/> which allows particular methods be used easier.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <typeparam name="K">Object type</typeparam>
		/// <param name="validator"></param>
		/// <param name="arg"></param>
		/// <param name="validationCode"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> EThat<T, K>(this Validator<T> validator, Expression<Func<T, IEnumerable<K>>> arg, Action<Validator<IEnumerable<K>>> validationCode)
		{
			var v2 = validator.Nest(arg);
			validationCode(v2);
			return validator;
		}

		/// <summary>
		/// Allows to validate property using <see cref="Validator{T}"/> of <see cref="IEnumerable{K}"/>.
		/// Returned validator is of type <see cref="IEnumerable{K}"/> which allows particular methods be used easier.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <typeparam name="K">Object type</typeparam>
		/// <param name="validator"></param>
		/// <param name="argName">Argument name.</param>
		/// <param name="arg">Argument getter.</param>
		/// <param name="validationCode">Validation code.</param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> EThat<T, K>(this Validator<T> validator, string argName, Func<T, IEnumerable<K>> arg, Action<Validator<IEnumerable<K>>> validationCode)
		{
			validator.Test(v => validationCode(new Validator<IEnumerable<K>>(validator.Name + "." + argName, arg(v))));
			return validator;
		}

		/// <summary>
		/// Allows to validate property using <see cref="Validator{T}"/> of <see cref="IEnumerable{K}"/>.
		/// Returned validator is of type <see cref="IEnumerable{K}"/> which allows particular methods be used easier.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <typeparam name="K">Object type</typeparam>
		/// <param name="validator"></param>
		/// <param name="argName">Argument name.</param>
		/// <param name="arg">Object to be validated.</param>
		/// <param name="validationCode">Validation code.</param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<T> EThat<T, K>(this Validator<T> validator, string argName, IEnumerable<K> arg, Action<Validator<IEnumerable<K>>> validationCode)
		{
			validationCode(new Validator<IEnumerable<K>>(validator.Name + "." + argName, arg));
			return validator;
		}
		#endregion
	}
}
