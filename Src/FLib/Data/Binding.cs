using System;
using System.ComponentModel;
using FLib.Interfaces.Data;

namespace FLib.Data
{
	/// <summary>
	/// See <see cref="FLib.Interfaces.Data.IBinding"/> for full description.
	/// </summary>
	/// <seealso cref="FLib.Interfaces.Data.IBinding"/>
	public class Binding
		: IBinding
	{
		#region Private fields
		private TypeConverter SourceConverter = null;
		private TypeConverter TargetConverter = null;
		private bool ControlFlowSource = false; //Used to prevent StackOverflow when Mode is BindingMode.TwoWay
		private bool ControlFlowTarget = false;
		#endregion

		#region IBinding Members
		/// <inheritdoc />
		public BindingMode Mode { get; private set; }

		/// <inheritdoc />
		public object Source
		{
			get { return this.SourcePath.Root; }
			internal set { this.SourcePath.Root = value; }
		}

		/// <inheritdoc />
		public IPropertyPath SourcePath { get; private set; }

		/// <inheritdoc />
		public object Target
		{
			get { return this.TargetPath.Root; }
		}

		/// <inheritdoc />
		public IPropertyPath TargetPath { get; private set; }

		/// <inheritdoc />
		public Type ConverterType { get; set; }
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes new binding.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourceProperty"></param>
		/// <param name="target"></param>
		/// <param name="targetProperty"></param>
		/// <param name="setTarget"></param>
		/// <param name="mode"></param>
		/// <param name="converterType"></param>
		internal Binding(object source, IPropertyPath sourceProperty, object target, IPropertyPath targetProperty, bool setTarget,
			BindingMode mode = BindingMode.OneWay, Type converterType = null)
		{
			Validate.All(() => source, v => v.NotNull());
			Validate.All(() => sourceProperty, v => v.NotNull());
			Validate.All(() => target, v => v.NotNull());
			Validate.All(() => targetProperty, v => v.NotNull());

			this.SourcePath = sourceProperty;
			if (!this.SourcePath.Initialized)
			{
				this.SourcePath.BeginInit();
				this.SourcePath.Root = source;
				this.SourcePath.EndInit();
			}
			else
			{
				this.SourcePath.Root = source;
			}

			this.TargetPath = targetProperty;
			if (!this.TargetPath.Initialized)
			{
				this.TargetPath.BeginInit();
				this.TargetPath.Root = target;
				this.TargetPath.EndInit();
			}
			else
			{
				this.TargetPath.Root = target;
			}

			this.Mode = mode;
			this.ConverterType = converterType;
			this.SetBindings();

			if (setTarget)
			{
				this.TargetPath.Value = this.GetConvertedSource();
			}
		}

		/// <summary>
		/// Initializes new binding.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="sourceProperty"></param>
		/// <param name="target"></param>
		/// <param name="targetProperty"></param>
		/// <param name="mode"></param>
		/// <param name="converterType"></param>
		public Binding(object source, IPropertyPath sourceProperty, object target, IPropertyPath targetProperty,
			BindingMode mode = BindingMode.OneWay, Type converterType = null)
			: this(source, sourceProperty, target, targetProperty, true, mode, converterType)
		{ }
		#endregion

		#region Destructors
		/// <summary>
		/// Disposes.
		/// </summary>
		~Binding()
		{
			this.Dispose(false);
		}
		#endregion

		#region Private methods
		private void SourceToTarget(object sender, PropertyChangedEventArgs e)
		{
			if (this.ControlFlowSource)
			{
				this.ControlFlowSource = false;
				return;
			}
			this.ControlFlowTarget = true;

			this.TargetPath.Value = this.GetConvertedSource();
		}

		private void TargetToSource(object sender, PropertyChangedEventArgs e)
		{
			if (this.ControlFlowTarget)
			{
				this.ControlFlowTarget = false;
				return;
			}
			this.ControlFlowSource = true;

			this.SourcePath.Value = this.GetConvertedTarget();
		}

		/// <summary>
		/// Gets source value and converts it to destination type.
		/// </summary>
		/// <returns></returns>
		private object GetConvertedSource()
		{
			object obj = this.SourcePath.Value;
			if (obj != null && this.SourceConverter.CanConvertTo(this.TargetPath.ValueType))
			{
				obj = this.SourceConverter.ConvertTo(obj, this.TargetPath.ValueType);
			}
			else if (obj != null && this.TargetConverter.CanConvertFrom(obj.GetType()))
			{
				obj = this.TargetConverter.ConvertFrom(obj);
			}
			return obj;
		}

		/// <summary>
		/// Gets target value and converts it to source type.
		/// </summary>
		/// <returns></returns>
		private object GetConvertedTarget()
		{
			object obj = this.TargetPath.Value;
			if (obj != null && this.TargetConverter.CanConvertTo(this.SourcePath.ValueType))
			{
				obj = this.TargetConverter.ConvertTo(obj, this.SourcePath.ValueType);
			}
			else if (obj != null && this.SourceConverter.CanConvertFrom(obj.GetType()))
			{
				obj = this.SourceConverter.ConvertFrom(obj);
			}
			return obj;
		}

		/// <summary>
		/// Sets bindings
		/// </summary>
		private void SetBindings()
		{
			#region Converters
			if (this.ConverterType != null)
			{
				this.SourceConverter = this.TargetConverter = Activator.CreateInstance(this.ConverterType) as TypeConverter;
			}
			else
			{
				this.SourceConverter = this.SourcePath.ValueConverter;
				this.TargetConverter = this.TargetPath.ValueConverter;
			}
			#endregion

			#region Bindings
			switch (this.Mode)
			{
				case BindingMode.OneWay:
					this.SourcePath.PropertyChanged += new PropertyChangedEventHandler(this.SourceToTarget);
					break;

				case BindingMode.TwoWay:
					this.SourcePath.PropertyChanged += new PropertyChangedEventHandler(this.SourceToTarget);
					this.TargetPath.PropertyChanged += new PropertyChangedEventHandler(this.TargetToSource);
					break;
			}
			#endregion
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Disposes binding.
		/// </summary>
		/// <param name="dispose"></param>
		protected virtual void Dispose(bool dispose)
		{
			if (dispose)
			{
				this.SourcePath.Dispose();
				this.TargetPath.Dispose();
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
