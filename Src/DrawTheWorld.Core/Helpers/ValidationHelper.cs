using System;
using FLib;

namespace DrawTheWorld.Core
{
	public static class ValidationHelper
	{
		public static Validator<Size> IsValidSize(this Validator<Size> validator)
		{
			validator.Test(s => s.Width > 0 && s.Height > 0, s => new ArgumentNullException(s, "Size is invalid")); 
			return validator;
		}
	}
}
