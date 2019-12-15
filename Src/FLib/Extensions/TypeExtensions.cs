using System;
using System.Linq;
using System.Reflection;

namespace FLib
{
	/// <summary>
	/// Extensions for <see cref="System.Type"/>.
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// Checks if @this derives from type.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool HasInBase(this Type @this, Type type)
		{
			Validate.All(() => type, v => v.NotNull());
			var t = @this;
			while (t != null && t != typeof(object))
			{
				if (t == type)
					return true;
				t = t.BaseType;
			}
			return false;
		}

		/// <summary>
		/// Checks if @this implements specified interface.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool Implements(this Type @this, Type type)
		{
			Validate.All(() => type, v => v.NotNull().That(t => t.IsInterface, "Type must be interface"));
			return @this == type || @this.GetInterfaces().Any(t => t == type);
		}

		/// <summary>
		/// Gets value of <see cref="MemberInfo"/> when it is <see cref="FieldInfo"/> or <see cref="PropertyInfo"/>.
		/// </summary>
		/// <remarks>
		/// When <paramref name="this"/> is neither <see cref="FieldInfo"/> nor <see cref="PropertyInfo"/>, <see cref="InvalidCastException"/> will be thrown.
		/// </remarks>
		/// <param name="this"></param>
		/// <param name="arg">Object on which method should be called.</param>
		/// <returns></returns>
		public static object GetValue(this MemberInfo @this, object arg)
		{
			return @this is PropertyInfo ? ((PropertyInfo)@this).GetValue(arg, null) : ((FieldInfo)@this).GetValue(arg);
		}
	}
}
