using System;
using System.ComponentModel;
using System.Globalization;

namespace FLib.Data.Converters
{
	/// <summary>
	/// Type converter for <see cref="System.Type"/>.
	/// </summary>
	/// <remarks>
	/// Supported source type: string
	/// Supported destination type: string
	/// 
	/// Converts type names to appropriate <see cref="System.Type"/>(supports atomic types and arrays of these types).
	/// </remarks>
	public sealed class SystemTypeConverter
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
				switch (stringVal.ToLower(CultureInfo.InvariantCulture))
				{
					case "bool":
						return typeof(bool);
					case "char":
						return typeof(char);

					case "decimal":
						return typeof(decimal);
					case "double":
						return typeof(double);
					case "float":
						return typeof(float);

					case "byte":
						return typeof(byte);
					case "sbyte":
						return typeof(sbyte);
					case "short":
						return typeof(short);
					case "ushort":
						return typeof(ushort);
					case "int":
						return typeof(int);
					case "uint":
						return typeof(uint);
					case "long":
						return typeof(long);
					case "ulong":
						return typeof(ulong);

					case "object":
						return typeof(object);
					case "string":
						return typeof(string);

					case "bool[]":
						return typeof(bool[]);
					case "char[]":
						return typeof(char[]);

					case "decimal[]":
						return typeof(decimal[]);
					case "double[]":
						return typeof(double[]);
					case "float[]":
						return typeof(float[]);

					case "byte[]":
						return typeof(byte[]);
					case "sbyte[]":
						return typeof(sbyte[]);
					case "short[]":
						return typeof(short[]);
					case "ushort[]":
						return typeof(ushort[]);
					case "int[]":
						return typeof(int[]);
					case "uint[]":
						return typeof(uint[]);
					case "long[]":
						return typeof(long[]);
					case "ulong[]":
						return typeof(ulong[]);

					case "object[]":
						return typeof(object[]);
					case "string[]":
						return typeof(string[]);

					default:
						return Type.GetType(stringVal);
				}
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
				return ((Type)value).FullName;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
		#endregion
	}
}