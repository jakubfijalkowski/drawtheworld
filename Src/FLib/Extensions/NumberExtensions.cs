using System;

namespace FLib
{
	/// <summary>
	/// Extensions for integer atomic types.
	/// </summary>
	/// <remarks>
	/// <see cref="NumberExtensions.CountBits(byte)"/> is based on "Sparse ones" from http://gurmeet.net/puzzles/fast-bit-counting-routines/
	/// </remarks>
	public static class NumberExtensions
	{
		/// <summary>
		/// Counts number of bits that are set.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static int CountBits(this byte n)
		{
			int cnt = 0;
			while (n != 0)
			{
				cnt++;
				n &= (byte)(n - 1);
			}
			return cnt;
		}

		/// <summary>
		/// Counts number of bits that are set.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static int CountBits(this char n)
		{
			int cnt = 0;
			while (n != 0)
			{
				cnt++;
				n &= (char)(n - 1);
			}
			return cnt;
		}

		/// <summary>
		/// Counts number of bits that are set.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static int CountBits(this ushort n)
		{
			int cnt = 0;
			while (n != 0)
			{
				cnt++;
				n &= (ushort)(n - 1);
			}
			return cnt;
		}

		/// <summary>
		/// Counts number of bits that are set.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static int CountBits(this uint n)
		{
			int cnt = 0;
			while (n != 0)
			{
				cnt++;
				n &= (uint)(n - 1);
			}
			return cnt;
		}

		/// <summary>
		/// Counts number of bits that are set.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static int CountBits(this ulong n)
		{
			int cnt = 0;
			while (n != 0)
			{
				cnt++;
				n &= (ulong)(n - 1);
			}
			return cnt;
		}

		/// <summary>
		/// Clamps value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static T Clamp<T>(this T val, T min, T max)
			where T : IComparable<T>
		{
			if (val.CompareTo(min) < 0)
				return min;
			else if (val.CompareTo(max) > 0)
				return max;
			return val;
		}

#if !WINRT
		/// <summary>
		/// Counts flags that are set.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static int Count(this Enum e)
		{
			return ((IConvertible)e).ToUInt64(System.Globalization.CultureInfo.InvariantCulture).CountBits();
		}
#endif
	}
}
