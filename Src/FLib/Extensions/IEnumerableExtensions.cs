using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FLib
{
	/// <summary>
	/// Extensions for <see cref="IEnumerable{T}"/> and <see cref="IEnumerable"/>.
	/// </summary>
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Executes specified action on each element in the collection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="this"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> @this, Action<T> action)
		{
			foreach (var item in @this)
				action(item);
			return @this;
		}

		/// <summary>
		/// Executes specified action on each element in the collection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="this"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> @this, Action<T, int> action)
		{
			int i = 0;
			foreach (var item in @this)
				action(item, i++);
			return @this;
		}

		/// <summary>
		/// Executes specified action on each element in the collection.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static IEnumerable ForEach(this IEnumerable @this, Action<object> action)
		{
			foreach (var item in @this)
				action(item);
			return @this;
		}

		/// <summary>
		/// Executes specified action on each element in the collection.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static IEnumerable ForEach(this IEnumerable @this, Action<object, int> action)
		{
			int i = 0;
			foreach (var item in @this)
				action(item, i++);
			return @this;
		}

		/// <summary>
		/// Evaluates to true if all elements of the second collection are in the first.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static bool ContainsAll<T>(this IEnumerable<T> first, IEnumerable<T> second)
		{
			foreach (var item in second)
				if (!first.Contains(item))
					return false;
			return true;
		}

		/// <summary>
		/// Evaluates to true if all elements of the second collection are in the first.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <param name="comparer"></param>
		/// <returns></returns>
		public static bool ContainsAll<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
		{
			foreach (var item in second)
				if (!first.Contains(item, comparer))
					return false;
			return true;
		}

		/// <summary>
		/// Evaluates to true if all elements of the first collection are in the second.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static bool AllIn<T>(this IEnumerable<T> first, IEnumerable<T> second)
		{
			foreach (var item in first)
				if (!second.Contains(item))
					return false;
			return true;
		}

		/// <summary>
		/// Evaluates to true if all elements of the first collection are in the second.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <param name="comparer"></param>
		/// <returns></returns>
		public static bool AllIn<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
		{
			foreach (var item in first)
				if (!second.Contains(item, comparer))
					return false;
			return true;
		}

		/// <summary>
		/// Evaluates to true if the first collection have the same elements as the second.
		/// T must have valid <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> implementations.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static bool EqualsTo<T>(this IEnumerable<T> first, IEnumerable<T> second)
		{
			var a = first.ToLookup(i => i);
			var b = second.ToLookup(i => i);
			return a.Count == b.Count && a.All(g => g.Count() == b[g.Key].Count());
		}

		/// <summary>
		/// Checks if collection contains element that matches predicate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static bool Contains<T>(this IEnumerable<T> collection, Predicate<T> predicate)
		{
			foreach (var item in collection)
			{
				if (predicate(item))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Checks if collection does not contain element that matches predicate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static bool NotContain<T>(this IEnumerable<T> collection, Predicate<T> predicate)
		{
			foreach (var item in collection)
			{
				if (predicate(item))
					return false;
			}
			return true;
		}
	}
}
