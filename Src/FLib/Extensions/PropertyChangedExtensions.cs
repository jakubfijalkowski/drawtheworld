using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace FLib
{
	/// <summary>
	/// Extensions for <see cref="System.ComponentModel.PropertyChangedEventHandler"/>.
	/// </summary>
	public static class PropertyChangedExtensions
	{
		/// <summary>
		/// Raises event when eventHandler is not empty.
		/// </summary>
		/// <param name="eventHandler">this</param>
		/// <param name="sender"></param>
		/// <param name="propertyName"></param>
		public static void Raise(this PropertyChangedEventHandler eventHandler, object sender,
#if WINRT || NET45
 [CallerMemberName] string propertyName = ""
#else
 string propertyName
#endif
)
		{
			if (eventHandler != null)
			{
				eventHandler(sender, new PropertyChangedEventArgs(propertyName));
			}
		}

		/// <summary>
		/// Raises event when eventHandler is not empty.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="eventHandler">this</param>
		/// <param name="sender"></param>
		/// <param name="propertyExpression"></param>
		public static void Raise<T>(this PropertyChangedEventHandler eventHandler, T sender, Expression<Func<T, object>> propertyExpression)
		{
			if (eventHandler != null)
			{
				eventHandler(sender, new PropertyChangedEventArgs(sender.NameOf(propertyExpression)));
			}
		}

		/// <summary>
		/// Raises event when eventHandler is not empty.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="eventHandler">this</param>
		/// <param name="sender"></param>
		/// <param name="propertyExpression"></param>
		public static void Raise<T>(this PropertyChangedEventHandler eventHandler, T sender, Expression<Func<object>> propertyExpression)
		{
			if (eventHandler != null)
			{
				eventHandler(sender, new PropertyChangedEventArgs(sender.NameOf(propertyExpression)));
			}
		}
	}
}
