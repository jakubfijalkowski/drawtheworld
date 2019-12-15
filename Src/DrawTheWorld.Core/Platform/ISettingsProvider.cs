using System.Collections.Generic;
using System.ComponentModel;

namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// List of available states for <see cref="ISettingsProvider.Item"/>.
	/// </summary>
	public enum SettingsState
	{
		FirstSignIn,
		UserSignedIn
	}

	/// <summary>
	/// Provides access to the settings.
	/// </summary>
	public interface ISettingsProvider
		: INotifyPropertyChanged
	{
		/// <summary>
		/// Gets or sets state of some action.
		/// </summary>
		/// <param name="stateKey"></param>
		/// <returns></returns>
		bool this[SettingsState state] { get; set; }

		/// <summary>
		/// Gets or sets music volume in range 0..1.
		/// </summary>
		double MusicVolume { get; set; }

		/// <summary>
		/// Gets or sets sounds volume in range 0..1.
		/// </summary>
		double SoundsVolume { get; set; }

		/// <summary>
		/// Gets or sets music volume in range 0..100.
		/// </summary>
		double PercentageMusicVolume { get; set; }

		/// <summary>
		/// Gets or sets music volume in range 0..100.
		/// </summary>
		double PercentageSoundVolume { get; set; }

		/// <summary>
		/// Gets or sets the languages that user has selected as "supported".
		/// </summary>
		IReadOnlyList<string> UserLanguages { get; set; }
	}
}
