using System.ComponentModel;

namespace FLib.Interfaces.Data
{
	/// <summary>
	/// Interface for object that shares data context.
	/// </summary>
	public interface IDataContext
		: INotifyPropertyChanged
	{
		/// <summary>
		/// Data context.
		/// </summary>
		object DataContext { get; }
	}
}
