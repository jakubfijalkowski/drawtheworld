using System;
using System.ComponentModel;
using FLib.Interfaces.Data;

namespace FLib.Data.Converters
{
	/// <summary>
	/// Type converter for <see cref="FLib.Interfaces.Data.IPropertyPath"/>.
	/// </summary>
	/// <remarks>
	/// Supported source type: string
	/// Supported destination type: string
	/// </remarks>
	public sealed class PropertyPathConverter
		: TypeConverter
	{
		#region From
		/// <inheritdoc />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		/// <inheritdoc />
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			var stringVal = value as string;
			if (stringVal != null)
			{
				if (stringVal.IndexOf('.') > 0 || stringVal.IndexOf('[') > 0)
					return new PropertyPath(stringVal);
				return new OneLevelPropertyPath(stringVal);
			}
			return base.ConvertFrom(context, culture, value);
		}
		#endregion

		#region To
		/// <inheritdoc />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		/// <inheritdoc />
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return (value as IPropertyPath).Path;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
		#endregion
	}
}
