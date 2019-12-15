using System;

namespace FLib.Testing
{
	/// <summary>
	/// Specified factory was never added to <see cref="TestingFactory"/>.
	/// </summary>
	public class FactoryNotDefinedException
		: Exception
	{
		/// <summary>
		/// Base type.
		/// </summary>
		public Type FactoryType { get; private set; }

		/// <summary>
		/// Variation or empty.
		/// </summary>
		public string Variation { get; private set; }

		/// <summary>
		/// Initializes exception.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="variation"></param>
		/// <param name="message"></param>
		public FactoryNotDefinedException(Type type, string variation, string message = "")
			: base(message)
		{
			this.FactoryType = type;
			this.Variation = variation;
		}
	}
}
