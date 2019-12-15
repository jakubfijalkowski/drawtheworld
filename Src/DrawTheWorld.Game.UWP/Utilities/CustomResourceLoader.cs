using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DrawTheWorld.Core.Platform;
using Windows.UI.Xaml.Resources;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Custom resource loader. Supports <c>Settings</c> and <c>String</c> keys.
	/// </summary>
	sealed class CustomResourceLoader
		: CustomXamlResourceLoader
	{
		private const string SettingsKey = "Settings";
		private const string StringKey = "String";
		private const string UISettingsKey = "UI";

		private readonly ISettingsProvider Settings = null;
		private readonly UISettings UISettings = null;
		private readonly Dictionary<string, PropertyInfo> SettingsProperties = null;

		public CustomResourceLoader(ISettingsProvider settings, UISettings uiSettings)
		{
			this.Settings = settings;
			this.UISettings = uiSettings;

			this.SettingsProperties = typeof(ISettingsProvider).GetProperties().ToDictionary(p => p.Name, p => p);
		}

		protected override object GetResource(string resourceId, string objectType, string propertyName, string propertyType)
		{
			var dotIndex = resourceId.IndexOf('.');
			if (dotIndex > -1)
			{
				string key = resourceId.Substring(0, dotIndex);
				string id = resourceId.Remove(0, dotIndex + 1);

				switch (key)
				{
					case SettingsKey:
						return this.SettingsProperties[id].GetValue(this.Settings);

					case StringKey:
						return Strings.Get(id);
				}
			}
			else if (resourceId == UISettingsKey)
				return this.UISettings;

			return base.GetResource(resourceId, objectType, propertyName, propertyType);
		}
	}
}
