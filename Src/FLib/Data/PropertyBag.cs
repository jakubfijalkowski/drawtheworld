using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace FLib.Data
{
	/// <summary>
	/// Property bag allows commiting/discarding new values - they aren't directly written to object.
	/// Uses DynamicObject as implementation.
	/// Usable with 'dynamic' type.
	/// Doesn't support indexed properties.
	/// </summary>
	public sealed class PropertyBag
		: DynamicObject, INotifyPropertyChanged
	{
		#region Private fields
		private Dictionary<string, ValueInfo> Values = new Dictionary<string, ValueInfo>();
		#endregion

		#region Public Properties
		/// <summary>
		/// Base object.
		/// </summary>
		public object Base { get; private set; }
		#endregion

		#region Public Methods
		/// <summary>
		/// Commits changes.
		/// </summary>
		public void Commit()
		{
			foreach (var item in this.Values)
			{
				if (item.Value.IsSet)
				{
					item.Value.Property.SetValue(this.Base, item.Value.Value, null);
					item.Value.IsSet = false;
				}
			}
			this.PropertyChanged.Raise(this, () => this.Base);
		}

		/// <summary>
		/// Discards changes.
		/// </summary>
		public void Discard()
		{
			foreach (var item in this.Values)
			{
				if (item.Value.IsSet)
				{
					item.Value.Value = item.Value.Property.GetValue(this.Base, null);
					item.Value.IsSet = false;
					this.PropertyChanged.Raise(this, item.Key);
				}
			}
		}

		/// <summary>
		/// After calling, property bag will mark all properties as not saved and following call to <see cref="Commit"/> will save all properties.
		/// </summary>
		public void Invalidate()
		{
			foreach (var item in this.Values.Values)
				item.IsSet = true;
		}
		#endregion

		#region INotifyPropertyChanged Members
		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes property bag.
		/// </summary>
		/// <param name="obj"></param>
		public PropertyBag(object obj)
		{
			Validate.All(() => obj, v => v.NotNull());

			this.Base = obj;
			var properties = obj
				.GetType()
#if WINRT
				.GetRuntimeProperties()
#else
.GetProperties()
#endif
.Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);
			foreach (var prop in properties)
			{
				this.Values.Add(prop.Name, new ValueInfo(prop, obj));
			}
		}
		#endregion

		#region Overrides
		/// <inheritdoc />
		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return this.Values.Keys;
		}

		/// <inheritdoc />
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			ValueInfo val = this.Get(binder.Name, binder.IgnoreCase);
			if (val != null)
			{
				result = val.Value;
				return true;
			}
			result = null;
			return false;
		}

		/// <inheritdoc />
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			ValueInfo val = this.Get(binder.Name, binder.IgnoreCase);
			if (val != null)
			{
#if !WINRT
				if (value != null && value.GetType() != val.Property.PropertyType)
					value = val.Converter.ConvertFrom(value);
#endif
				val.IsSet = true;
				val.Value = value;
				this.PropertyChanged.Raise(this, val.Property.Name);
				return true;
			}
			return false;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return this.Base.ToString();
		}
		#endregion

		#region Auxiliary methods
		private ValueInfo Get(string name, bool ignoreCase)
		{
			if (!ignoreCase)
			{
				ValueInfo ret = null;
				this.Values.TryGetValue(name, out ret);
				return ret;
			}
			else
			{
				name = name.ToLower();
				foreach (var item in this.Values)
				{
					if (item.Key.ToLower() == name)
						return item.Value;
				}
			}
			return null;
		}
		#endregion

		#region ValueInfo class
		private class ValueInfo
		{
			public PropertyInfo Property = null;
#if !WINRT
			public TypeConverter Converter = null;
#endif
			public bool IsSet = false;
			public object Value = null;

			public ValueInfo(PropertyInfo info, object obj)
			{
				this.Property = info;
#if !WINRT
				this.Converter = Converters.Utilities.GetTypeConverter(info);
#endif
				this.Value = info.GetValue(obj, null);
			}
		}
		#endregion
	}
}
