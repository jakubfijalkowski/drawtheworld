using System;

namespace FLib
{
	/// <summary>
	/// Extensions for <see cref="System.Delegate"/>.
	/// </summary>
	public static class DelegateExtensions
	{
		/// <summary>
		/// Raises event(action) when it is not null.
		/// </summary>
		/// <param name="eventHandler"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void Raise(this EventHandler eventHandler, object sender, EventArgs args)
		{
			var handler = eventHandler;
			if (handler != null)
				handler(sender, args);
		}

		/// <summary>
		/// Raises event when eventHandler is not null.
		/// </summary>
		/// <typeparam name="TEventArgs"></typeparam>
		/// <param name="eventHandler">this</param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void Raise<TEventArgs>(this EventHandler<TEventArgs> eventHandler, object sender, TEventArgs args)
#if !WINRT
 where TEventArgs : EventArgs
#endif
		{
			var handler = eventHandler;
			if (handler != null)
				handler(sender, args);
		}

		/// <summary>
		/// Raises event(action) when it is not null.
		/// </summary>
		/// <typeparam name="TSender"></typeparam>
		/// <typeparam name="TEventArgs"></typeparam>
		/// <param name="eventHandler"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void Raise<TSender, TEventArgs>(this Action<TSender, TEventArgs> eventHandler, TSender sender, TEventArgs args)
		{
			var handler = eventHandler;
			if (handler != null)
				handler(sender, args);
		}

		/// <summary>
		/// Raises delegate when it is not null.
		/// </summary>
		/// <remarks>
		/// Argument checks are done in <see cref="Delegate.DynamicInvoke"/>.
		/// </remarks>
		/// <param name="eventHandler">this</param>
		/// <param name="args"></param>
		public static void Raise(this Delegate eventHandler, params object[] args)
		{
			var handler = eventHandler;
			if (handler != null)
				handler.DynamicInvoke(args);
		}
	}
}
