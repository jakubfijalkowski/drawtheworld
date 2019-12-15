using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace FLib
{
	/// <summary>
	/// Validator class.
	/// Base class for extension methods that performs validation.
	/// </summary>
	/// <remarks>
	/// Validators stops executing after first test fails.
	/// For example, following code will execute only <see cref="NullEmptyValidation.NotNull{T}"/>:
	/// <code>
	/// object arg = null;
	/// (new Validator&lt;object&gt;("arg", arg)).NotNull().Equal(new object());
	/// (new Validator&lt;object&gt;("arg", arg)).DisableExceptions().NotNull().Equal(new object());
	/// </code>
	/// 
	/// Keep in mind that <see cref="CustomValidation.That{T, K}(Validator{T}, Expression{Func{T, K}}, Action{Validator{K}})"/> will throw exception even with exceptions disabled.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public sealed class Validator<T>
	{
		#region Fields
		/// <summary>
		/// Name of the field/property that is being validated.
		/// </summary>
		public readonly string Name = string.Empty;

		internal readonly T SingleValue = default(T);
		internal readonly IEnumerable<T> MultipleValues = null;

		private readonly object _Parent = null;

		private bool _IsValid = true;
		private bool ExceptionsEnabled = true;
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets value indicating if current validation chain succeeded.
		/// </summary>
		public bool IsValid
		{
			get { return this._IsValid; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes validator for single object.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="parent">Can be only <see cref="Validator{K}"/> object, but type is not checked.</param>
		public Validator(string name, T value, object parent = null)
		{
			this.Name = name;
			this.SingleValue = value;
			this._Parent = parent;
		}

		/// <summary>
		/// Initializes validator for multiple objects.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="values"></param>
		/// <param name="parent">Can be only <see cref="Validator{K}"/> object, but type is not checked.</param>
		public Validator(string name, IEnumerable<T> values, object parent = null)
		{
			this.Name = name;
			this.MultipleValues = values;
			this._Parent = parent;
		}
		#endregion

		#region Validation code
		/// <summary>
		/// Runs the test for the value(s) and throws the exception if it is not valid.
		/// </summary>
		/// <param name="testCode"></param>
		/// <param name="exceptionCreator"></param>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public void Test(Func<T, bool> testCode, Func<string, Exception> exceptionCreator)
		{
			if (!this.IsValid)
				return;
			if (this.MultipleValues == null)
			{
				if (!testCode(this.SingleValue))
				{
					this._IsValid = false;
					if (this.ExceptionsEnabled)
						throw exceptionCreator(this.Name);
				}
			}
			else
			{
				int i = 0;
				foreach (var item in this.MultipleValues)
				{
					if (!testCode(item))
					{
						this._IsValid = false;
						if (this.ExceptionsEnabled)
							throw exceptionCreator("{0}[{1}] is invalid.".FormatWith(this.Name, i));
						else
							break;
					}
					i++;
				}
			}
		}

		/// <summary>
		/// Runs the test for the value(s) but the test is responsible for throwing exception.
		/// </summary>
		/// <param name="testCode"></param>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public void Test(Action<T> testCode)
		{
			if (this.MultipleValues == null)
				testCode(this.SingleValue);
			else
				this.MultipleValues.ForEach(testCode);
		}
		#endregion

		#region ForAll
		/// <summary> 
		/// Converts current validator to handle collections.
		/// </summary>
		/// <typeparam name="DestType">Inner type of collection.</typeparam>
		/// <returns></returns>
		public Validator<DestType> ForAll<DestType>()
		{
			if (this.MultipleValues != null)
			{
				if (this.MultipleValues.All(o => o is IEnumerable<DestType>))
					return new Validator<DestType>(this.Name, this.MultipleValues.SelectMany(o => (IEnumerable<DestType>)o));
			}
			else if (this.SingleValue is IEnumerable<DestType>)
			{
				return new Validator<DestType>(this.Name, (IEnumerable<DestType>)this.SingleValue);
			}
			throw new InvalidOperationException("Cannot get enumerable validator for non-enumerable object.");
		}

		/// <summary>
		/// Converts current validator to handle collections.
		/// </summary>
		/// <returns></returns>
		public Validator<char> ForAll()
		{
			if (typeof(T) != typeof(string))
				throw new ArgumentException("ForAll can be used only on strings");
			return new Validator<char>(this.Name, (IEnumerable<char>)this.SingleValue);
		}

		/// <summary>
		/// Converts current validator to <see cref="Validator{IEnumerable}"/> for easier method calling.
		/// </summary>
		/// <typeparam name="DestType"></typeparam>
		/// <returns></returns>
		public Validator<IEnumerable<DestType>> AsEnumerable<DestType>()
		{
			if (!typeof(T).Implements(typeof(IEnumerable<DestType>)))
				throw new InvalidOperationException("Cannot cast type {0} to IEnumerable<{1}>.".FormatWith(typeof(T).Name, typeof(DestType).Name));
			if (this.MultipleValues != null)
				return new Validator<IEnumerable<DestType>>(this.Name, this.MultipleValues.Cast<IEnumerable<DestType>>(), this._Parent);
			return new Validator<IEnumerable<DestType>>(this.Name, (IEnumerable<DestType>)this.SingleValue, this._Parent);
		}
		#endregion

		#region Nest/Parent
		/// <summary>
		/// Gets validator for nested field/property.
		/// </summary>
		/// <remarks>
		/// Uses <see cref="ObjectExtensions.SimpleNameAndValueOf{K, T}"/> so only simple access is allowed.
		/// </remarks>
		/// <typeparam name="DestType">Value type of the nested field.</typeparam>
		/// <param name="expression">Field/Property access expression.</param>
		/// <returns>Validator for nested property(call <see cref="Parent{T}"/> to revert back to parent validator).</returns>
		public Validator<DestType> Nest<DestType>(Expression<Func<T, DestType>> expression)
		{
			if (this.MultipleValues == null)
			{
				var v = this.SingleValue.SimpleNameAndValueOf(expression);
				return new Validator<DestType>(this.Name + '.' + v.Item1, v.Item2, this);
			}
			else
			{
				var newValues = this.MultipleValues.Select(v => v.SimpleNameAndValueOf(expression).Item2);
				return new Validator<DestType>(this.Name + '.' + default(T).NameOf(expression), newValues, this);
			}
		}

		/// <summary>
		/// Gets parent validator. Type must be THE SAME as the one for which <see cref="Nest"/> was called.
		/// </summary>
		/// <typeparam name="ParentType">Type of the parent validator.</typeparam>
		/// <returns>Parent validator, if any.</returns>
		public Validator<ParentType> Parent<ParentType>()
		{
			if (this._Parent == null)
				throw new InvalidOperationException("Cannot get parent of root validator.");
			else if (!(this._Parent is Validator<ParentType>))
				throw new InvalidCastException("Cannot cast parent validator to Validator<{0}>.".FormatWith(typeof(ParentType).Name));
			return (Validator<ParentType>)this._Parent;
		}
		#endregion

		#region Others
		/// <summary>
		/// When called, further calls to validators will not throw exceptions - only <see cref="IsValid"/> will be set.
		/// </summary>
		/// <returns></returns>
		public Validator<T> DisableExceptions()
		{
			this.ExceptionsEnabled = false;
			return this;
		}
		#endregion
	}
}
