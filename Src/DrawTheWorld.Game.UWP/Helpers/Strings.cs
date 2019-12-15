using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;

namespace DrawTheWorld.Game
{
	/// <summary>
	/// Strings helper - it is responsible for loading strings from resources.
	/// </summary>
	static class Strings
	{
		private static readonly ResourceLoader Resources = new ResourceLoader();
		private static readonly ResourceMap ResMap = ResourceManager.Current.MainResourceMap;

		/// <summary>
		/// Gets specified resource.
		/// </summary>
		/// <param name="resName"></param>
		/// <returns></returns>
		public static string Get(string resName)
		{
			return Resources.GetString(resName);
		}

		/// <summary>
		/// Gets specified string resource in specified language.
		/// </summary>
		/// <param name="resName"></param>
		/// <param name="language"></param>
		/// <returns></returns>
		public static string GetForLanguage(string resName, string language)
		{
			language = language.ToLower();
			var resource = ResMap["Resources/" + resName].Candidates.FirstOrDefault(r => r.GetQualifierValue("LANGUAGE").ToLower() == language);
			return resource != null ? resource.ValueAsString : string.Empty;
		}
	}
}
