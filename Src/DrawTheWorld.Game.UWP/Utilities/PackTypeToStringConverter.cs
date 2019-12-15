using System;
using DrawTheWorld.Core.UserData;
using Windows.UI.Xaml.Data;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Converts <see cref="PackType"/> to translated string.
	/// </summary>
	sealed class PackTypeToStringConverter
		: IValueConverter
	{
		public const string ResourceKey = "Pages_GameList_PackType_";

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is PackType)
				return Strings.Get(ResourceKey + value.ToString());
			throw new NotSupportedException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}
