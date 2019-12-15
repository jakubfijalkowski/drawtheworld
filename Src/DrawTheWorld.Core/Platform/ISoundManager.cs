namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// Type of sound.
	/// </summary>
	public enum Sound
	{
		/// <summary>
		/// Sound when a page get's changed or a popup is displayed.
		/// </summary>
		PageChanged,

		/// <summary>
		/// Sound when field is being filled.
		/// </summary>
		FieldFilled,

		/// <summary>
		/// Sound when field is being erased.
		/// </summary>
		FieldErased,

		/// <summary>
		/// Sound when other action on field is being performed.
		/// </summary>
		OtherFieldAction,

		/// <summary>
		/// Sound when fine is being charged
		/// </summary>
		FineCharged,

		/// <summary>
		/// Sound when game is won.
		/// </summary>
		GameWon,

		/// <summary>
		/// Sound when game is won and new track is unlocked.
		/// </summary>
		GameWonTrackUnlocked,

		/// <summary>
		/// Sound when game is lost.
		/// </summary>
		GameLost,

		/// <summary>
		/// Sound when tool or brush data gets changed.
		/// </summary>
		ToolChanged = Sound.PageChanged
	}

	/// <summary>
	/// Manages sound subsystem.
	/// </summary>
	public interface ISoundManager
	{
		/// <summary>
		/// Plays specified <paramref name="sound"/>.
		/// </summary>
		/// <param name="sound"></param>
		void PlaySound(Sound sound);
	}
}
