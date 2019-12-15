using System.Collections.Generic;
using System.Linq;
using DrawTheWorld.Core.Helpers;
using FLib;

namespace DrawTheWorld.Core
{
	/// <summary>
	/// List with translated messages that are resolved at runtime.
	/// </summary>
	public sealed class Translations
		: IReadOnlyDictionary<string, string>
	{
		private readonly Dictionary<string, string> Messages = null;
		private readonly string _MainLanguage = null;

		/// <summary>
		/// Returns translation for <see cref="Config.DefaultLanguage"/>.
		/// </summary>
		public string DefaultTranslation
		{
			get { return this.Messages[Config.DefaultLanguage]; }
		}

		/// <summary>
		/// The language that is the best match for current language.
		/// </summary>
		public string MainLanguage
		{
			get { return this._MainLanguage; }
		}

		/// <summary>
		/// Returns translation for <see cref="MainLanguage"/>.
		/// </summary>
		public string MainTranslation
		{
			get { return this.Messages[this._MainLanguage]; }
		}

		/// <summary>
		/// Returns translation for specified <paramref name="language" />
		/// </summary>
		/// <param name="language"></param>
		/// <returns>Returns translation thet is the best match for specified <paramref name="langauge"/>.</returns>
		public string this[string language]
		{
			get
			{
				if (language != null)
					language = language.Trim().ToLower();
				Validate.Debug(() => language, v => v.NotNullAndNotWhiteSpace());

				return LanguageHelper.MatchTranslation(this.Messages, language);
			}
		}

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
		public Translations()
		{
			this.Messages = new Dictionary<string, string>();
			this.Messages.Add(Config.DefaultLanguage, string.Empty);
			this._MainLanguage = Config.DefaultLanguage;
		}

		/// <summary>
		/// Initializes the object with specified translations, ensuring that translation for <see cref="Config.DefaultLanguage"/> is present.
		/// </summary>
		/// <param name="source"></param>
		public Translations(IReadOnlyDictionary<string, string> source)
		{
			Validate.Debug(() => source, v => v.NotNull());

			this.Messages = source.Where(kv => !string.IsNullOrWhiteSpace(kv.Value)).ToDictionary(kv => kv.Key.Trim().ToLower(), kv => kv.Value);
			if (!this.Messages.ContainsKey(Config.DefaultLanguage))
				this.Messages.Add(Config.DefaultLanguage, string.Empty);

			this._MainLanguage = LanguageHelper.MatchForPreferredLanguage(this.Messages.Keys);
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
