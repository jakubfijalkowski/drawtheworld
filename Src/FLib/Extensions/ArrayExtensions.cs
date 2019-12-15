using System;

namespace FLib
{
	/// <summary>
	/// Extensions for arrays.
	/// </summary>
	public static class ArrayExtensions
	{
		/// <summary>
		/// Compares two arrays.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a">this</param>
		/// <param name="b"></param>
		/// <returns>Returns -1 if a is greater than b, 0 if they are equal, 1 if b is greater than a.</returns>
		public static int CompareTo<T>(this T[] a, T[] b)
			where T : IComparable<T>
		{
			return a.CompareTo(0, a.Length, b, 0, b.Length);
		}

		/// <summary>
		/// Compares two subarrays.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="startA"></param>
		/// <param name="b"></param>
		/// <param name="startB"></param>
		/// <returns>Returns -1 if a is greater than b, 0 if they are equal, 1 if b is greater than a.</returns>
		public static int CompareTo<T>(this T[] a, int startA, T[] b, int startB)
			where T : IComparable<T>
		{
			return a.CompareTo(startA, a.Length - startA, b, startB, b.Length - startB);
		}

		/// <summary>
		/// Compares two subarrays.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="startA"></param>
		/// <param name="lenA"></param>
		/// <param name="b"></param>
		/// <param name="startB"></param>
		/// <param name="lenB"></param>
		/// <returns>Returns -1 if a is greater than b, 0 if they are equal, 1 if b is greater than a.</returns>
		public static int CompareTo<T>(this T[] a, int startA, int lenA, T[] b, int startB, int lenB)
			where T : IComparable<T>
		{
			if (lenA != lenB)
				return lenB.CompareTo(lenA);
			for (int ia = startA, ib = startB; ia < startA + lenA; ++ia, ++ib)
			{
				if (b[ib].CompareTo(a[ia]) != 0)
					return b[ib].CompareTo(a[ia]);
			}
			return 0;
		}
	}
}
