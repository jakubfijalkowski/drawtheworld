using System.Collections.Generic;
using FLib;

namespace DrawTheWorld.Core.Tools
{
	/// <summary>
	/// Base tool class with statically-typed user data.
	/// </summary>
	/// <typeparam name="T">User data passed to <see cref="Perform(IGame, T, IEnumerable{Field})"/>.</typeparam>
	public abstract class BaseTool<T>
		: ITool
	{
		/// <inheritdoc />
		public bool IsSingleAction
		{
			get { return false; }
		}

		/// <inheritdoc />
		public void Perform(IGame on, object userData, IEnumerable<Field> fields)
		{
			Validate.Debug(() => userData, v => v.That(o => o is T));
			this.Perform(on, (T)userData, fields);
		}

		/// <summary>
		/// Statically typed <see cref="ITool.Perform"/>
		/// </summary>
		/// <param name="on"></param>
		/// <param name="userData"></param>
		/// <param name="fields"></param>
		protected abstract void Perform(IGame on, T userData, IEnumerable<Field> fields);
	}
}
