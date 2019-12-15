using System;
using System.Collections.Generic;
using System.Globalization;

namespace FLib.Data
{
	/// <summary>
	/// Comparer for strings that sorts them in natural order.
	/// </summary>
	public class NaturalOrder
		: IComparer<string>
	{
		#region IComparer<string> Members
		/// <inheritdoc />
		public int Compare(string x, string y)
		{
			for (int i = 0, j = 0; i < Math.Min(x.Length, y.Length); i++, j++)
			{
				while (i < x.Length && char.IsWhiteSpace(x[i])) //Skipping whitespaces
					++i;
				while (j < y.Length && char.IsWhiteSpace(y[j]))
					++j;

				char c1 = x[i];
				char c2 = y[j];
				if (c1 == c2)
					continue;
				if (char.IsDigit(c1) && char.IsDigit(c2))
				{
					int startI = i, startJ = j;
					while (i < x.Length && (char.IsDigit(x[i]) || char.IsWhiteSpace(x[i])))
						++i;
					while (j < y.Length && (char.IsDigit(y[j]) || char.IsWhiteSpace(y[j])))
						++j;

					var subA = x.Substring(startI, i - startI);
					var subB = y.Substring(startJ, j - startJ);

					for (int k = 0; k < subA.Length; k++)
					{
						if (!char.IsDigit(subA[k]))
							subA = subA.Remove(k--, 1);
					}
					for (int k = 0; k < subB.Length; k++)
					{
						if (!char.IsDigit(subB[k]))
							subB = subB.Remove(k--, 1);
					}

					long a = long.Parse(subA, CultureInfo.CurrentCulture);
					long b = long.Parse(subB, CultureInfo.CurrentCulture);
					if (a != b)
						return a.CompareTo(b);
				}
				else
				{
					return StringComparer.CurrentCulture.Compare(c1.ToString(), c2.ToString());
				}
			}
			return x.Length.CompareTo(y.Length);
		}
		#endregion
	}
}
