using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.Platform;
using FLib;
using Windows.Storage;

namespace DrawTheWorld.Game.Platform
{
	sealed class SettingsProvider
		: ISettingsProvider
	{
		private const string SettingsContainer = "Settings";
		private const double Epsilon = 0.001;

		private readonly ApplicationDataContainer Container = null;

		private IReadOnlyList<string> _UserLanguages = null;
		private double _MusicVolume = 0.0;
		private double _SoundsVolume = 0.0;

		/// <inheritdoc />
		public bool this[SettingsState state]
		{
			get { return this.Get<bool>(state.ToString()); }
			set { this.Save(value, state.ToString()); }
		}

		/// <inheritdoc />
		public double MusicVolume
		{
			get { return this._MusicVolume; }
			set
			{
				Validate.Debug(() => value, v => v.InRange(0.0, 1.0));
				if (Math.Abs(this._MusicVolume - value) > Epsilon)
				{
					this._MusicVolume = value;
					this.Save(value);

					this.PropertyChanged.Raise(this);
					this.PropertyChanged.Raise(this, _ => _.PercentageMusicVolume);
				}
			}
		}

		/// <inheritdoc />
		public double SoundsVolume
		{
			get { return this._SoundsVolume; }
			set
			{
				Validate.Debug(() => value, v => v.InRange(0.0, 1.0));
				if (Math.Abs(this._SoundsVolume - value) > Epsilon)
				{
					this._SoundsVolume = value;
					this.Save(value);

					this.PropertyChanged.Raise(this);
					this.PropertyChanged.Raise(this, _ => _.PercentageSoundVolume);
				}
			}
		}

		/// <inheritdoc />
		public double PercentageMusicVolume
		{
			get { return this._MusicVolume * 100.0; }
			set
			{
				Validate.Debug(() => value, v => v.InRange(0.0, 100.0));
				this.MusicVolume = value / 100.0;
			}
		}

		/// <inheritdoc />
		public double PercentageSoundVolume
		{
			get { return this._SoundsVolume * 100.0; }
			set
			{
				Validate.Debug(() => value, v => v.InRange(0.0, 100.0));
				this.SoundsVolume = value / 100.0;
			}
		}

		/// <inheritdoc />
		/// <remarks>
		/// Current implementation does not support changes of user languages.
		/// </remarks>
		public IReadOnlyList<string> UserLanguages
		{
			get { return this._UserLanguages; }
			set { throw new InvalidOperationException(); }
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes the settings.
		/// </summary>
		public SettingsProvider()
		{
			this._UserLanguages = LanguageHelper.AllLanguages.ToList();

			this.Container = ApplicationData.Current.LocalSettings.CreateContainer(SettingsContainer, ApplicationDataCreateDisposition.Always);

			this._MusicVolume = this.Get<double>(_ => _.MusicVolume, 1.0).Clamp(0.0, 1.0);
			this._SoundsVolume = this.Get<double>(_ => _.SoundsVolume, 1.0).Clamp(0.0, 1.0);
		}

		private T Get<T>(Expression<Func<SettingsProvider, T>> name, T defaultValue = default(T))
		{
			return this.Get<T>(this.NameOf(name), defaultValue);
		}

		private T Get<T>(string name, T defaultValue = default(T))
		{
			object obj = null;
			if (this.Container.Values.TryGetValue(name, out obj) && obj is T)
				return (T)obj;
			return defaultValue;
		}

		private void Save(object value, [CallerMemberName] string name = "")
		{
			this.Container.Values[name] = value;
		}
	}
}
