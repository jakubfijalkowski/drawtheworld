using System;
using System.Windows.Input;
using FLib;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Command that selects item on execution.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	sealed class SelectCommand<T>
		: ICommand
	{
		/// <summary>
		/// Gets or sets selected item.
		/// </summary>
		public T SelectedItem { get; set; }

		/// <summary>
		/// Raised when item gets selected.
		/// </summary>
		public event Action<object, T> ItemSelected;

		/// <inheritdoc />
#pragma warning disable 0067
		public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

		/// <inheritdoc />
		public bool CanExecute(object parameter)
		{
			return object.Equals(parameter, default(T)) || parameter is T;
		}

		/// <inheritdoc />
		public void Execute(object parameter)
		{
			this.SelectedItem = (T)parameter;
			this.ItemSelected.Raise(this, this.SelectedItem);
		}
	}
}
