using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FLib;

#if WINRT
using System.Reflection;
#endif

namespace DrawTheWorld.Core.UI
{
	/// <summary>
	/// Marker interface for messages.
	/// </summary>
	public interface IMessage
	{ }

	/// <summary>
	/// Manages notifications.
	/// </summary>
	public abstract class Notifications
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.UI.Notifications");

		private readonly Dictionary<Type, Func<IMessage, Task>> Handlers = null;

		/// <summary>
		/// Initializes the message.
		/// </summary>
		public Notifications()
		{
			var type = typeof(Notifications);
			var methods = type.GetMethods();
			var msgTypes = type.Assembly.ExportedTypes
				.Where(t => t.Implements(typeof(IMessage)) && t != typeof(IMessage))
				.Select(t => new { Name = t.Name.Remove(t.Name.Length - 7), Type = t })
				.ToDictionary(a => a.Name, a => a.Type);

			this.Handlers = methods
				.Where(m => m.Name.EndsWith("Handler"))
				.Select(m => new
				{
					Type = msgTypes[m.Name.Remove(m.Name.Length - 7)],
					Handler = new Func<IMessage, Task>(msg => (Task)m.Invoke(this, new object[] { msg }))
				})
				.ToDictionary(a => a.Type, a => a.Handler);
		}

		/// <summary>
		/// Notifies user of some action and returns result of the action.
		/// </summary>
		/// <param name="message"></param>
		public Task NotifyAsync(IMessage message)
		{
			Validate.Debug(() => message, v => v.NotNull().That(m => this.Handlers.Keys.Contains(m.GetType()), "Message type not supported."));

			Logger.Debug("Sending notification of type {0}.", message.GetType().Name);
			var result = this.Handlers[message.GetType()](message);
			return result ?? Task.FromResult<object>(null);
		}

		/// <summary>
		/// Notifies user of some action.
		/// </summary>
		/// <param name="message"></param>
		public void Notify(IMessage message)
		{
			this.NotifyAsync(message);
		}

		protected abstract Task NotEnoughMoneyHandler(IMessage message);
		protected abstract Task CannotAccessApiHandler(IMessage message);
		protected abstract Task PacksSynchronizedHandler(IMessage message);
		protected abstract Task PackPurchasedHandler(IMessage message);
		protected abstract Task PackDataErrorHandler(IMessage message);
		protected abstract Task PackLoadErrorHandler(IMessage message);
		protected abstract Task PackSaveErrorHandler(IMessage message);
		protected abstract Task StorageProblemHandler(IMessage message);
		protected abstract Task SignInRequiredHandler(IMessage message);
		protected abstract Task ErrorHandler(IMessage message);
		protected abstract Task BoardImportErrorHandler(IMessage message);
		protected abstract Task PackNotValidHandler(IMessage message);
		protected abstract Task PackIsEmptyHandler(IMessage message);
	}
}
