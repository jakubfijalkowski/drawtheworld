using System.Collections.Generic;
using System.Collections.Specialized;

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Combines <see cref="IReadOnlyList{T}"/> and <see cref="INotifyCollectionChanged"/> into single interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IReadOnlyObservableList<T>
		: IReadOnlyList<T>, INotifyCollectionChanged
	{ }

	/// <summary>
	/// Combines <see cref="IList{T}"/> and <see cref="INotifyCollectionChanged"/> into single interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IObservableList<T>
		: IList<T>, IReadOnlyObservableList<T>, INotifyCollectionChanged
	{ }
}
