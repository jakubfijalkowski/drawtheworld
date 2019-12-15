using System.Collections.Specialized;

namespace FLib
{
	/// <summary>
	/// Extensions for <see cref="System.Collections.Specialized.NotifyCollectionChangedEventHandler"/>.
	/// </summary>
	public static class CollectionChangedExtensions
	{
		/// <summary>
		/// Raises <see cref="System.Collections.Specialized.NotifyCollectionChangedEventHandler"/> without any data when handler is not null.
		/// </summary>
		/// <param name="eventHandler">this</param>
		/// <param name="sender"></param>
		/// <param name="action"></param>
		public static void Raise(this NotifyCollectionChangedEventHandler eventHandler, object sender, NotifyCollectionChangedAction action)
		{
			if (eventHandler != null)
			{
				eventHandler(sender, new NotifyCollectionChangedEventArgs(action));
			}
		}

		/// <summary>
		/// Raises <see cref="System.Collections.Specialized.NotifyCollectionChangedEventHandler"/> with single item when handler is not null.
		/// </summary>
		/// <param name="eventHandler">this</param>
		/// <param name="sender"></param>
		/// <param name="action"></param>
		/// <param name="changedItem"></param>
		public static void Raise(this NotifyCollectionChangedEventHandler eventHandler, object sender, NotifyCollectionChangedAction action, object changedItem)
		{
			if (eventHandler != null)
			{
				eventHandler(sender, new NotifyCollectionChangedEventArgs(action, changedItem));
			}
		}

		/// <summary>
		/// Raises <see cref="System.Collections.Specialized.NotifyCollectionChangedAction.Replace"/> when handler is not null.
		/// </summary>
		/// <param name="eventHandler">this</param>
		/// <param name="sender"></param>
		/// <param name="oldItem"></param>
		/// <param name="newItem"></param>
		public static void RaiseReplace(this NotifyCollectionChangedEventHandler eventHandler, object sender, object oldItem, object newItem)
		{
			if (eventHandler != null)
			{
				eventHandler(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem));
			}
		}

		/// <summary>
		/// Raises <see cref="System.Collections.Specialized.NotifyCollectionChangedAction.Move"/> when handler is not null.
		/// </summary>
		/// <param name="eventHandler"></param>
		/// <param name="sender"></param>
		/// <param name="item"></param>
		/// <param name="oldPos"></param>
		/// <param name="newPos"></param>
		public static void RaiseMove(this NotifyCollectionChangedEventHandler eventHandler, object sender, object item, int oldPos, int newPos)
		{
			if (eventHandler != null)
			{
				eventHandler(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newPos, oldPos));
			}
		}
	}
}
