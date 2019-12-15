namespace FLib
{
	/// <summary>
	/// Extensions for <see cref="System.String"/>
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Formats string with specified data. Shorthand for <see cref="System.String.Format(string, object[])"/>.
		/// </summary>
		/// <param name="format">Format.</param>
		/// <param name="objs">Data.</param>
		/// <returns></returns>
		public static string FormatWith(this string format, params object[] objs)
		{
			return string.Format(format, objs);
		}
	}

	namespace StringManipulation
	{
		/// <summary>
		/// Extensions for <see cref="System.String"/> related with manipulation of strings.
		/// </summary>
		public static class StringExtension
		{
			/// <summary>
			/// Skips whitespaces.
			/// </summary>
			/// <param name="str">this</param>
			/// <param name="i">Current index.</param>
			public static void SkipWhiteSpaces(this string str, ref int i)
			{
				while (i < str.Length && char.IsWhiteSpace(str[i]))
					++i;
			}

			/// <summary>
			/// Moves to specified character or returns false.
			/// </summary>
			/// <param name="str">this</param>
			/// <param name="i">Current index.</param>
			/// <param name="c">Character to be found.</param>
			/// <returns>True when character was found, otherwise false.</returns>
			public static bool GoTo(this string str, ref int i, char c)
			{
				while (i < str.Length && str[i] != c)
					++i;
				return i < str.Length && str[i] == c;
			}
		}
	}
}
