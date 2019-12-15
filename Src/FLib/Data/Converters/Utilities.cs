using System;
using System.ComponentModel;
using System.Reflection;

namespace FLib.Data.Converters
{
	/// <summary>
	/// Type conversion utilities.
	/// </summary>
	public static class Utilities
	{
		/// <summary>
		/// Gets <see cref="System.ComponentModel.TypeConverter"/> for property.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="default">Default <see cref="System.ComponentModel.TypeConverter"/>(if custom is not present).</param>
		/// <returns><see cref="System.ComponentModel.TypeConverter"/> set in attribute or default value.</returns>
		public static TypeConverter GetTypeConverter(PropertyInfo property, Type @default = null)
		{
			var converter = property.GetCustomAttributes(typeof(TypeConverterAttribute), false);
			return converter.Length == 1 ?
				Activator.CreateInstance((Type.GetType((converter[0] as TypeConverterAttribute).ConverterTypeName))) as TypeConverter :
				(@default == null ? TypeDescriptor.GetConverter(property.PropertyType) : Activator.CreateInstance(@default) as TypeConverter);
		}


		/// <summary>
		/// Gets <see cref="System.ComponentModel.TypeConverter"/> for field.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="default">Default <see cref="System.ComponentModel.TypeConverter"/>(if custom is not present).</param>
		/// <returns><see cref="System.ComponentModel.TypeConverter"/> set in attribute or default value.</returns>
		public static TypeConverter GetTypeConverter(FieldInfo field, Type @default = null)
		{
			var converter = field.GetCustomAttributes(typeof(TypeConverterAttribute), false);
			return converter.Length == 1 ?
				Activator.CreateInstance((Type.GetType((converter[0] as TypeConverterAttribute).ConverterTypeName))) as TypeConverter :
				(@default == null ? TypeDescriptor.GetConverter(field.FieldType) : Activator.CreateInstance(@default) as TypeConverter);
		}

		/// <summary>
		/// Gets <see cref="System.ComponentModel.TypeConverter"/>.
		/// </summary>
		/// <param name="member">Property or field.</param>
		/// <param name="default">Default converter.</param>
		/// <exception cref="ArgumentException">Thrown, when member is neither PropertyInfo nor FieldInfo.</exception>
		/// <returns><see cref="System.ComponentModel.TypeConverter"/> set in attribute or default value.</returns>
		public static TypeConverter GetTypeConverter(MemberInfo member, Type @default = null)
		{
			if (member is PropertyInfo)
			{
				return GetTypeConverter(member as PropertyInfo, @default);
			}
			else if (member is FieldInfo)
			{
				return GetTypeConverter(member as FieldInfo, @default);
			}
			throw new ArgumentException("Parameter is not PropertyInfo nor FieldInfo", "member");
		}
	}
}
