using System;
using System.Threading.Tasks;
using FLib;

#if WINRT
using Windows.UI.Xaml;
#endif

namespace FLib
{
	/// <summary>
	/// Provides convenient way of waiting(for, mostly, events).
	/// </summary>
	public static class WaitFor
	{
		/// <summary>
		/// Waits for <see cref="System.Action{TSender, TEventArgs}"/> and sets result(object returned by <paramref name="resultGetter"/> or default value of <typeparamref name="TResult"/>).
		/// </summary>
		/// <typeparam name="TSender"></typeparam>
		/// <typeparam name="TEventArgs"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="addHandler"></param>
		/// <param name="removeHandler"></param>
		/// <param name="resultGetter"></param>
		/// <returns></returns>
		public static Task<TResult> Action<TSender, TEventArgs, TResult>(Action<Action<TSender, TEventArgs>> addHandler, Action<Action<TSender, TEventArgs>> removeHandler, Func<TResult> resultGetter)
		{
			Validate.Debug(() => addHandler, v => v.NotNull());
			Validate.Debug(() => removeHandler, v => v.NotNull());

			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			Action<TSender, TEventArgs> handler = null;
			handler = (s, e) =>
			{
				removeHandler(handler);
				tcs.SetResult(resultGetter != null ? resultGetter() : default(TResult));
			};
			addHandler(handler);
			return tcs.Task;
		}

		/// <summary>
		/// Waits for <see cref="System.Action{TSender, TEventArgs}"/>.
		/// </summary>
		/// <typeparam name="TSender"></typeparam>
		/// <typeparam name="TEventArgs"></typeparam>
		/// <param name="addHandler"></param>
		/// <param name="removeHandler"></param>
		/// <returns></returns>
		public static Task Action<TSender, TEventArgs>(Action<Action<TSender, TEventArgs>> addHandler, Action<Action<TSender, TEventArgs>> removeHandler)
		{
			return Action<TSender, TEventArgs, object>(addHandler, removeHandler, null);
		}

		/// <summary>
		/// Waits for <see cref="System.Action{TObject, TEventArgs}"/> and sets result(object returned by <paramref name="resultGetter"/> or default value of <typeparamref name="TResult"/>).
		/// </summary>
		/// <typeparam name="TEventArgs"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="addHandler"></param>
		/// <param name="removeHandler"></param>
		/// <param name="resultGetter"></param>
		/// <returns></returns>
		public static Task<TResult> Action<TEventArgs, TResult>(Action<Action<object, TEventArgs>> addHandler, Action<Action<object, TEventArgs>> removeHandler, Func<TResult> resultGetter)
		{
			return Action<object, TEventArgs, TResult>(addHandler, removeHandler, resultGetter);
		}

		/// <summary>
		/// Waits for <see cref="System.Action{TObject, TEventArgs}"/>.
		/// </summary>
		/// <typeparam name="TEventArgs"></typeparam>
		/// <param name="addHandler"></param>
		/// <param name="removeHandler"></param>
		/// <returns></returns>
		public static Task Action<TEventArgs>(Action<Action<object, TEventArgs>> addHandler, Action<Action<object, TEventArgs>> removeHandler)
		{
			return Action<object, TEventArgs, object>(addHandler, removeHandler, null);
		}

		/// <summary>
		/// Waits for <see cref="EventHandler{TEventArgs}"/>.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <typeparam name="TEventArgs"></typeparam>
		/// <param name="addHandler"></param>
		/// <param name="removeHandler"></param>
		/// <param name="resultGetter"></param>
		/// <returns></returns>
		public static Task<TResult> Event<TResult, TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler, Func<TResult> resultGetter)
#if !WINRT
 where TEventArgs : EventArgs
#endif
		{
			Validate.Debug(() => addHandler, v => v.NotNull());
			Validate.Debug(() => removeHandler, v => v.NotNull());

			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			EventHandler<TEventArgs> handler = null;
			handler = (s, e) =>
			{
				removeHandler(handler);
				tcs.SetResult(resultGetter != null ? resultGetter() : default(TResult));
			};
			addHandler(handler);
			return tcs.Task;
		}

		public static Task<TResult> RoutedEvent<TResult>(Action<RoutedEventHandler> addHandler, Action<RoutedEventHandler> removeHandler, Func<TResult> resultGetter)
		{
			Validate.Debug(() => addHandler, v => v.NotNull());
			Validate.Debug(() => removeHandler, v => v.NotNull());

			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			RoutedEventHandler handler = null;
			handler = (s, e) =>
			{
				removeHandler(handler);
				tcs.SetResult(resultGetter != null ? resultGetter() : default(TResult));
			};
			addHandler(handler);
			return tcs.Task;
		}

		/// <summary>
		/// Waits for <see cref="EventHandler{TEventArgs}"/>, but ignores result.
		/// </summary>
		/// <typeparam name="TEventArgs"></typeparam>
		/// <param name="addHandler"></param>
		/// <param name="removeHandler"></param>
		/// <returns></returns>
		public static Task Event<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler)
#if !WINRT
 where TEventArgs : EventArgs
#endif
		{
			return Event<object, TEventArgs>(addHandler, removeHandler, null);
		}

		/// <summary>
		/// Waits for <see cref="EventHandler{TEventArgs}"/>, but ignores result.
		/// </summary>
		/// <typeparam name="TEventArgs"></typeparam>
		/// <param name="addHandler"></param>
		/// <param name="removeHandler"></param>
		/// <returns></returns>
		public static Task RoutedEvent(Action<RoutedEventHandler> addHandler, Action<RoutedEventHandler> removeHandler)
		{
			return RoutedEvent<object>(addHandler, removeHandler, null);
		}

#if WINRT
		/// <summary>
		/// Waits for <see cref="RoutedEventHandler"/>.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="addHandler"></param>
		/// <param name="removeHandler"></param>
		/// <param name="resultGetter"></param>
		/// <returns></returns>
		public static Task<TResult> Event<TResult>(Action<EventHandler<object>> addHandler, Action<EventHandler<object>> removeHandler, Func<TResult> resultGetter)
		{
			Validate.Debug(() => addHandler, v => v.NotNull());
			Validate.Debug(() => removeHandler, v => v.NotNull());

			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			EventHandler<object> handler = null;
			handler = (s, e) =>
			{
				removeHandler(handler);
				tcs.SetResult(resultGetter != null ? resultGetter() : default(TResult));
			};
			addHandler(handler);
			return tcs.Task;
		}

		/// <summary>
		/// Waits for <see cref="RoutedEventHandler"/> but ignores the result.
		/// </summary>
		/// <param name="addHandler"></param>
		/// <param name="removeHandler"></param>
		/// <returns></returns>
		public static Task Event(Action<EventHandler<object>> addHandler, Action<EventHandler<object>> removeHandler)
		{
			return Event<object>(addHandler, removeHandler, null);
		}
#endif
	}
}
