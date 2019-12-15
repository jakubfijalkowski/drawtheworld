namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// Manages lifecycle of the app.
	/// </summary>
	public interface ILifecycleManager
	{
		/// <summary>
		/// Gets value that indicates whether the app is during exit/suspension.
		/// </summary>
		bool IsSuspending { get; }
	}
}
