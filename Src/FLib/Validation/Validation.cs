using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace FLib
{
	/// <summary>
	/// Methods for conditional validation.
	/// </summary>
	/// <remarks>
	/// Because of strong use of generic types, compilator is unable to match generic parameters on some validators(collections), so we have to use interface as generic parameters.
	/// Use <see cref="Validate.EDebug{T}(Expression{Func{IEnumerable{T}}}, Action{Validator{IEnumerable{T}}})"/>,
	/// <see cref="Validate.ERelease{T}(Expression{Func{IEnumerable{T}}}, Action{Validator{IEnumerable{T}}})"/>,
	/// <see cref="Validate.EAll{T}(Expression{Func{IEnumerable{T}}}, Action{Validator{IEnumerable{T}}})"/> and
	/// <see cref="Validate.EAssert{T}(Expression{Func{IEnumerable{T}}})"/> to validate <see cref="IEnumerable{T}"/>s.
	/// </remarks>
	public static class Validate
	{
		#region Debug
		/// <summary>
		/// Runs validation code only in Debug compilation.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <param name="validationCode">Code used for validation.</param>
		[Conditional("DEBUG")]
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static void Debug<T>(Expression<Func<T>> expression, Action<Validator<T>> validationCode)
		{
			var v = expression.SimpleNameAndValueOf(expression);
			validationCode(new Validator<T>(v.Item1, v.Item2));
		}

		/// <summary>
		/// Runs validation code for <see cref="IEnumerable{T}"/> only in Debug compilation.
		/// Validator is for single, not multiple, value.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <param name="validationCode">Code used for validation.</param>
		[Conditional("DEBUG")]
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static void EDebug<T>(Expression<Func<IEnumerable<T>>> expression, Action<Validator<IEnumerable<T>>> validationCode)
		{
			var v = expression.SimpleNameAndValueOf(expression);
			validationCode(new Validator<IEnumerable<T>>(v.Item1, v.Item2));
		}

		/// <summary>
		/// Runs validation code only in Debug compilation.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="name">Name of the argument.</param>
		/// <param name="value">Value of the argument.</param>
		/// <param name="validationCode">Code used for validation.</param>
		[Conditional("DEBUG")]
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static void Debug<T>(string name, T value, Action<Validator<T>> validationCode)
		{
			validationCode(new Validator<T>(name, value));
		}

		/// <summary>
		/// Runs validation code for <see cref="IEnumerable{T}"/> only in Debug compilation.
		/// Validator is for single, not multiple, value.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="name">Name of the argument.</param>
		/// <param name="value">Value of the argument.</param>
		/// <param name="validationCode">Code used for validation.</param>
		[Conditional("DEBUG")]
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static void EDebug<T>(string name, IEnumerable<T> value, Action<Validator<IEnumerable<T>>> validationCode)
		{
			validationCode(new Validator<IEnumerable<T>>(name, value));
		}
		#endregion

		#region Release
		/// <summary>
		/// Runs validation code only in Release compilation.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <param name="validationCode">Code used for validation.</param>
		[Conditional("RELEASE")]
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static void Release<T>(Expression<Func<T>> expression, Action<Validator<T>> validationCode)
		{
			var v = default(T).SimpleNameAndValueOf(expression);
			validationCode(new Validator<T>(v.Item1, v.Item2));
		}

		/// <summary>
		/// Runs validation code for <see cref="IEnumerable{T}"/> only in Release compilation.
		/// Validator is for single, not multiple, value.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <param name="validationCode">Code used for validation.</param>
		[Conditional("RELEASE")]
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static void ERelease<T>(Expression<Func<IEnumerable<T>>> expression, Action<Validator<IEnumerable<T>>> validationCode)
		{
			var v = expression.SimpleNameAndValueOf(expression);
			validationCode(new Validator<IEnumerable<T>>(v.Item1, v.Item2));
		}

		/// <summary>
		/// Runs validation code only in Release compilation.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="name">Name of the argument.</param>
		/// <param name="value">Value of the argument.</param>
		/// <param name="validationCode">Code used for validation.</param>
		[Conditional("RELEASE")]
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static void Release<T>(string name, T value, Action<Validator<T>> validationCode)
		{
			validationCode(new Validator<T>(name, value));
		}

		/// <summary>
		/// Runs validation code for <see cref="IEnumerable{T}"/> only in Release compilation.
		/// Validator is for single, not multiple, value.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="name">Name of the argument.</param>
		/// <param name="value">Value of the argument.</param>
		/// <param name="validationCode">Code used for validation.</param>
		[Conditional("RELEASE")]
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static void ERelease<T>(string name, IEnumerable<T> value, Action<Validator<IEnumerable<T>>> validationCode)
		{
			validationCode(new Validator<IEnumerable<T>>(name, value));
		}
		#endregion

		#region All
		/// <summary>
		/// Runs validation code in every compilation.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <param name="validationCode">Code used for validation.</param>
		/// <returns>True, when validation succeeds, otherwise false.</returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static bool All<T>(Expression<Func<T>> expression, Action<Validator<T>> validationCode)
		{
			var v = expression.SimpleNameAndValueOf(expression);
			var validator = new Validator<T>(v.Item1, v.Item2);
			validationCode(validator);
			return validator.IsValid;
		}

		/// <summary>
		/// Runs validation code for <see cref="IEnumerable{T}"/> in every compilation.
		/// Validator is for single, not multiple, value.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <param name="validationCode">Code used for validation.</param>
		/// <returns>True, when validation succeeds, otherwise false.</returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static bool EAll<T>(Expression<Func<IEnumerable<T>>> expression, Action<Validator<IEnumerable<T>>> validationCode)
		{
			var v = expression.SimpleNameAndValueOf(expression);
			var validator = new Validator<IEnumerable<T>>(v.Item1, v.Item2);
			validationCode(validator);
			return validator.IsValid;
		}

		/// <summary>
		/// Runs validation code in every compilation.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="name">Name of the argument.</param>
		/// <param name="value">Value of the argument.</param>
		/// <param name="validationCode">Code used for validation.</param>
		/// <returns>True, when validation succeeds, otherwise false.</returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static bool All<T>(string name, T value, Action<Validator<T>> validationCode)
		{
			var validator = new Validator<T>(name, value);
			validationCode(validator);
			return validator.IsValid;
		}

		/// <summary>
		/// Runs validation code for <see cref="IEnumerable{T}"/> in every compilation.
		/// Validator is for single, not multiple, value.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="name">Name of the argument.</param>
		/// <param name="value">Value of the argument.</param>
		/// <param name="validationCode">Code used for validation.</param>
		/// <returns>True, when validation succeeds, otherwise false.</returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static bool EAll<T>(string name, IEnumerable<T> value, Action<Validator<IEnumerable<T>>> validationCode)
		{
			var validator = new Validator<IEnumerable<T>>(name, value);
			validationCode(validator);
			return validator.IsValid;
		}
		#endregion

		#region Assert
		/// <summary>
		/// Returns <see cref="Validator{T}"/> for specified parameter(every compilation).
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <returns>Validator configured to validate specified object.</returns>
		public static Validator<T> Assert<T>(Expression<Func<T>> expression)
		{
			var v = expression.SimpleNameAndValueOf(expression);
			return new Validator<T>(v.Item1, v.Item2);
		}

		/// <summary>
		/// Returns <see cref="Validator{T}"/> for <see cref="IEnumerable{T}"/>s for specified parameter(every compilation).
		/// Validator is for single, not multiple, value.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <returns>Validator configured to validate specified object.</returns>
		public static Validator<IEnumerable<T>> EAssert<T>(Expression<Func<IEnumerable<T>>> expression)
		{
			var v = expression.SimpleNameAndValueOf(expression);
			return new Validator<IEnumerable<T>>(v.Item1, v.Item2);
		}

		/// <summary>
		/// Returns <see cref="Validator{T}"/> for specified parameter(every compilation).
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="name">Name of the argument.</param>
		/// <param name="value">Value of the argument.</param>
		/// <returns>Validator configured to validate specified object.</returns>
		public static Validator<T> Assert<T>(string name, T value)
		{
			return new Validator<T>(name, value);
		}

		/// <summary>
		/// Returns <see cref="Validator{T}"/> for <see cref="IEnumerable{T}"/>s for specified parameter(every compilation).
		/// Validator is for single, not multiple, value.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="name">Name of the argument.</param>
		/// <param name="value">Value of the argument.</param>
		/// <returns>Validator configured to validate specified object.</returns>
		public static Validator<IEnumerable<T>> EAssert<T>(string name, IEnumerable<T> value)
		{
			return new Validator<IEnumerable<T>>(name, value);
		}

		/// <summary>
		/// Returns <see cref="Validator{T}"/> for specified parameter(every compilation).
		/// </summary>
		/// <remarks>
		/// <code>
		/// arg.Assert();
		/// </code>
		/// is a shorthand for
		/// <code>
		/// Validate.Assert("", arg);
		/// </code>
		/// </remarks>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="value">Value of the argument.</param>
		/// <param name="paramName"></param>
		/// <returns>Validator configured to validate specified object.</returns>
		public static Validator<T> Assert<T>(this T value, string paramName = "")
		{
			return new Validator<T>(paramName, value);
		}

		/// <summary>
		/// Returns <see cref="Validator{T}"/> for <see cref="IEnumerable{T}"/>s for specified parameter(every compilation).
		/// Validator is for single, not multiple, value.
		/// </summary>
		/// <remarks>
		/// <code>
		/// arg.EAssert();
		/// </code>
		/// is a shorthand for
		/// <code>
		/// Validate.EAssert("", arg);
		/// </code>
		/// </remarks>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="value">Value of the argument.</param>
		/// <param name="paramName"></param>
		/// <returns>Validator configured to validate specified object.</returns>
		public static Validator<IEnumerable<T>> EAssert<T>(this IEnumerable<T> value, string paramName = "")
		{
			return new Validator<IEnumerable<T>>(paramName, value);
		}
		#endregion
	}
}
