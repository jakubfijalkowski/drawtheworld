using System.Collections.Generic;
using System.Linq;
using FLib;

namespace DrawTheWorld.Core.Helpers
{
	/// <summary>
	/// Helps in language detection.
	/// </summary>
	public static class LanguageHelper
	{
		/// <summary>
		/// Gets preferred languages, in order of preference.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> PreferredLanguages
		{
			get
			{
#if WINRT
				foreach (var lang in Windows.System.UserProfile.GlobalizationPreferences.Languages)
					yield return lang.ToLower();
#else
				yield return System.Globalization.CultureInfo.CurrentUICulture.Name.ToLower();
#endif
			}
		}

		/// <summary>
		/// Gets current language.
		/// </summary>
		public static string CurrentLanguage
		{
			get
			{
#if WINRT
				return Windows.System.UserProfile.GlobalizationPreferences.Languages[0].ToLower();
#else
				return System.Globalization.CultureInfo.CurrentUICulture.Name.ToLower();
#endif
			}
		}

		/// <summary>
		/// Gets short version of current language(without country code).
		/// </summary>
		public static string ShortCurrentLanguage
		{
			get
			{
				var idx = CurrentLanguage.IndexOf('-');
				return idx > -1 ? CurrentLanguage.Remove(idx) : CurrentLanguage;
			}
		}

		/// <summary>
		/// Gets all languages that user wants(or needs) to support.
		/// </summary>
		/// <remarks>
		/// Union of <see cref="PreferredLanguages"/> and <see cref="Config.DefaultLanguage"/>.
		/// </remarks>
		public static IEnumerable<string> AllLanguages
		{
			get
			{
				foreach (var lng in PreferredLanguages)
					yield return lng;
				if (!PreferredLanguages.Contains(Config.DefaultLanguage))
					yield return Config.DefaultLanguage;
			}
		}

		/// <summary>
		/// Gets the currently used language that is supported by the game.
		/// </summary>
		public static string UsedLanguage
		{
			get
			{
#if WINRT
				return Windows.Globalization.ApplicationLanguages.Languages.First();
#else
				throw new System.NotImplementedException();
#endif
			}
		}

		/// <summary>
		/// Tries to match translation to one of the available languages.
		/// </summary>
		/// <remarks>
		/// We assume, that <paramref name="language"/> and every key of the <paramref name="availableTranslations"/> is valid lower case language.
		/// </remarks>
		/// <param name="availableTranslations">Translations that we want to match to.</param>
		/// <param name="language">Language to match.</param>
		/// <param name="fallbackLanguage">Language that will be returned if no match is found.</param>
		/// <returns>Matched translation -or- null, when not found.</returns>
		public static string MatchTranslation(IReadOnlyDictionary<string, string> availableTranslations, string language, string fallbackLanguage = Config.DefaultLanguage)
		{
			Validate.Debug(() => availableTranslations, v => v.NotNullAndNotEmpty());
			Validate.Debug(() => language, v => v.NotNullAndNotWhiteSpace());

			string val = null;
			if (availableTranslations.TryGetValue(language, out val) && !string.IsNullOrWhiteSpace(val))
				return val;

			foreach (var translation in availableTranslations)
			{
				if (language.StartsWith(translation.Key) && !string.IsNullOrWhiteSpace(translation.Value))
					return translation.Value;
			}

			if (!string.IsNullOrWhiteSpace(fallbackLanguage))
			{
				availableTranslations.TryGetValue(fallbackLanguage, out val);
				return val;
			}
			return null;
		}

		/// <summary>
		/// Tries to match one of the preffered languages to one of the available translations.
		/// See <see cref="MatchTranslation"/> for more info.
		/// </summary>
		/// <param name="availableTranslation"></param>
		/// <returns></returns>
		public static string MatchTranslationForPreferredLanguage(IReadOnlyDictionary<string, string> availableTranslation)
		{
			Validate.Debug(() => availableTranslation, v => v.NotNullAndNotEmpty());

			foreach (var lang in AllLanguages)
			{
				string match = MatchTranslation(availableTranslation, lang, null);
				if (!string.IsNullOrWhiteSpace(match))
					return match;
			}
			return null;
		}

		/// <summary>
		/// Tries to match language to one of the available languages.
		/// </summary>
		/// <remarks>
		/// We assume, that <paramref name="language"/> and every element of the <paramref name="availableLanguages"/> is valid lower case language.
		/// </remarks>
		/// <param name="availableLanguages">Languages that we want to match to.</param>
		/// <param name="language">Language to match.</param>
		/// <param name="fallbackLanguage">Language that will be returned if no match is found.</param>
		/// <returns>Matched language -or- null, when not found.</returns>
		public static string MatchLanguage(ICollection<string> availableLanguages, string language, string fallbackLanguage = Config.DefaultLanguage)
		{
			Validate.Debug(() => availableLanguages, v => v.NotNullAndNotEmpty());
			Validate.Debug(() => language, v => v.NotNullAndNotWhiteSpace());

			if (availableLanguages.Contains(language))
				return language;

			return availableLanguages.FirstOrDefault(l => language.StartsWith(l)) ?? fallbackLanguage;
		}

		/// <summary>
		/// Tries to match one of the preffered languages to one of the available languages.
		/// See <see cref="MatchLanguage"/> for more info.
		/// </summary>
		/// <param name="availableLanguages"></param>
		/// <returns></returns>
		public static string MatchForPreferredLanguage(ICollection<string> availableLanguages)
		{
			Validate.Debug(() => availableLanguages, v => v.NotNullAndNotEmpty());

			foreach (var lang in AllLanguages)
			{
				string match = MatchLanguage(availableLanguages, lang, null);
				if (match != null)
					return match;
			}
			return null;
		}

		/// <summary>
		/// Gets the form of the noun for particular number. The form is dependent on <see cref="UsedLanguage"/>.
		/// </summary>
		/// <remarks>
		/// For English:
		///   1 - singular form
		///   2 - plural form,
		///   
		/// For Polish:
		///   1 - singular,
		///   2 - last digit is between 2 and 4
		///   3 - last digit is 1 or is between 5 and 9 or the number is between 10 and 20
		/// </remarks>
		/// <param name="number"></param>
		/// <param name="languageOverride">Overrides value returned from <see cref="UsedLanguage"/>. May be null, pl or en.</param>
		/// <returns></returns>
		public static int GetNounForm(int number, string languageOverride = null)
		{
			if (languageOverride != null)
				Validate.Debug(() => languageOverride, v => v.IsIn(new string[] { "en", "pl" }));

			var lng = languageOverride ?? UsedLanguage.Substring(0, 2).ToLower();
			if (lng == "en")
				return number == 1 ? 1 : 2;
			else if (lng == "pl")
			{
				if (number == 1)
					return 1;
				if (number >= 10 && number <= 21)
					return 3;
				var last = number % 10;
				if (last >= 2 && last <= 4)
					return 2;
				return 3;
			}
			return 2;
		}

		/// <summary>
		/// Gets the noun that can be used with particular number.
		/// </summary>
		/// <remarks>
		/// It uses resource system to get the correct form. Final resource key is of form {<paramref name="resourceKey"/>}_{form}, where form
		/// is value returned by <see cref="GetNounForm"/>.
		/// </remarks>
		/// <param name="number"></param>
		/// <param name="resourceKey"></param>
		/// <returns></returns>
		public static string GetNoun(int number, string resourceKey)
		{
#if WINRT
			resourceKey = "Resources/" + resourceKey;
			var lng = Windows.ApplicationModel.Resources.Core.ResourceManager.Current.MainResourceMap[resourceKey].Resolve().Qualifiers[0].QualifierValue;
			resourceKey += "_" + GetNounForm(number, lng.Substring(0, 2).ToLower());
			return Windows.ApplicationModel.Resources.Core.ResourceManager.Current.MainResourceMap[resourceKey].Resolve().ValueAsString;
#else
			throw new System.NotImplementedException();
#endif
		}
	}
}
