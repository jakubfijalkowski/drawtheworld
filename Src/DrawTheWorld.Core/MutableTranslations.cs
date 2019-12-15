using System.Collections.Generic;
using System.ComponentModel;
using DrawTheWorld.Core.Helpers;
using FLib;

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Editable list of translations.
	/// </summary>
	public sealed class MutableTranslations
		: IReadOnlyDictionary<string, string>, INotifyPropertyChanged
	{
		private readonly Dictionary<string, string> Messages = null;
		private string _EditedLanguage = Config.DefaultLanguage;

		/// <summary>
		/// Gets or sets currently edited language.
		/// </summary>
		public string EditedLanguage
		{
			get { return this._EditedLanguage; }
			set
			{
				if (value != null)
					value = value.Trim().ToLower();

				Validate.Debug(() => value, v => v.NotNullAndNotWhiteSpace());

				if (value != this._EditedLanguage)
				{
					if (!this.Messages.ContainsKey(value))
						this.Messages.Add(value, string.Empty);

					this._EditedLanguage = value;

					this.PropertyChanged.Raise(this);
					this.PropertyChanged.Raise(this, _ => _.EditedTranslation);
				}
			}
		}

		/// <summary>
		/// Gets or sets translation for the <see cref="EditedLanguage"/>.
		/// </summary>
		public string EditedTranslation
		{
			get { return this.Messages[this.EditedLanguage]; }
			set { this[this.EditedLanguage] = value; }
		}

		/// <summary>
		/// Returns translation for specified <paramref name="language" />
		/// </summary>
		/// <param name="language"></param>
		/// <returns>Returns translation for specified <paramref name="language"/> -or- null, when translation is not found.</returns>
		public string this[string language]
		{
			get
			{
				if (language != null)
					language = language.Trim().ToLower();
				Validate.Debug(() => language, v => v.NotNullAndNotWhiteSpace());

				string translation = null;
				this.Messages.TryGetValue(language, out translation);
				return translation;
			}
			set
			{
				if (language != null)
					language = language.Trim().ToLower();
				Validate.Debug(() => language, v => v.NotNullAndNotWhiteSpace());

				this.Messages[language] = value;

				this.PropertyChanged.Raise(this);
				if (language == this.EditedLanguage)
					this.PropertyChanged.Raise(this, _ => _.EditedTranslation);
				if (language == LanguageHelper.MatchLanguage(this.Messages.Keys, Config.DefaultLanguage))
					this.PropertyChanged.Raise(this, _ => _.DefaultTranslation);
				if (language == LanguageHelper.MatchForPreferredLanguage(this.Messages.Keys))
					this.PropertyChanged.Raise(this, _ => _.MainTranslation);
			}
		}

		/// <summary>
		/// Gets translation for <see cref="Config.DefaultLanguage"/>.
		/// </summary>
		public string DefaultTranslation
		{
			get { return LanguageHelper.MatchTranslation(this.Messages, Config.DefaultLanguage); }
		}

		/// <summary>
		/// Gets translation that matches current language best.
		/// </summary>
		public string MainTranslation
		{
			get { return LanguageHelper.MatchTranslationForPreferredLanguage(this.Messages); }
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc />
		public IEnumerable<string> Keys
		{
			get { return this.Messages.Keys; }
		}

		/// <inheritdoc />
		public IEnumerable<string> Values
		{
			get { return this.Messages.Values; }
		}

		/// <inheritdoc />
		public int Count
		{
			get { return this.Messages.Count; }
		}

		/// <summary>
		/// Initializes empty object.
		/// </summary>
		public MutableTranslations()
		{
			this.Messages = new Dictionary<string, string>(1);
			this.Messages.Add(Config.DefaultLanguage, string.Empty);
		}

		/// <summary>
		/// Initializes the object using data from <paramref name="translations"/>.
		/// </summary>
		/// <param name="translations"></param>
		public MutableTranslations(Translations translations)
		{
			Validate.Debug(() => translations, v => v.NotNull());

			this.Messages = new Dictionary<string, string>(translations.Count);
			foreach (var i in translations)
				this.Messages.Add(i.Key, i.Value);
		}

		/// <summary>
		/// Returns immutable copy of this object.
		/// </summary>
		/// <returns></returns>
		public Translations ToReadOnly()
		{
			return new Translations(this);
		}

		/// <summary>
		/// Resets the collection to the <paramref name="original"/>.
		/// </summary>
		/// <param name="original"></param>
		public void Reset(Translations original)
		{
			Validate.Debug(() => original, v => v.NotNull());

			this.Messages.Clear();
			foreach (var i in original)
				this.Messages.Add(i.Key, i.Value);

			this._EditedLanguage = Config.DefaultLanguage;
			this.PropertyChanged.Raise(this, _ => _.EditedLanguage);
			this.PropertyChanged.Raise(this, _ => _.EditedTranslation);
			this.PropertyChanged.Raise(this, _ => _.DefaultTranslation);
			this.PropertyChanged.Raise(this, _ => _.MainTranslation);
		}

		/// <inheritdoc />
		public bool ContainsKey(string key)
		{
			return this.Messages.ContainsKey(key);
		}

		/// <inheritdoc />
		public bool TryGetValue(string key, out string value)
		{
			return this.Messages.TryGetValue(key, out value);
		}

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return this.Messages.GetEnumerator();
		}

		/// <inheritdoc />
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.Messages.GetEnumerator();
		}
	}
}
