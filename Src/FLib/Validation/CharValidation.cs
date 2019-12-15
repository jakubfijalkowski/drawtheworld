using System;
using System.Diagnostics;

namespace FLib
{
	/// <summary>
	/// Validators for char type.
	/// </summary>
	public static class CharValidation
	{
		/// <summary>
		/// Value must be letter or digit.
		/// </summary>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<char> IsLetterOrDigit(this Validator<char> validator)
		{
			validator.Test(v => char.IsLetterOrDigit(v), n => new ArgumentException("Value must be letter or digit.", n));
			return validator;
		}

		/// <summary>
		/// Value must be letter.
		/// </summary>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<char> IsLetter(this Validator<char> validator)
		{
			validator.Test(v => char.IsLetter(v), n => new ArgumentException("Value must be letter.", n));
			return validator;
		}

		/// <summary>
		/// Value must be digit.
		/// </summary>
		/// <param name="validator"></param>
		/// <returns></returns>
		[DebuggerStepThrough, DebuggerNonUserCode, DebuggerHidden]
		public static Validator<char> IsDigit(this Validator<char> validator)
		{
			validator.Test(v => char.IsDigit(v), n => new ArgumentException("Value must be digit.", n));
			return validator;
		}
	}
}
