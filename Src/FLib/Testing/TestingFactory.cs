using System;
using System.Collections.Generic;
#if WINRT
using System.Reflection;
using System.Linq;
#endif

namespace FLib.Testing
{
	using Variations = Dictionary<string, BuildInfo>;

	/// <summary>
	/// Classes that implement this interface should setup factory for future use.
	/// </summary>
	public interface IBlueprint
	{
		/// <summary>
		/// Setups factory.
		/// </summary>
		/// <param name="factory"></param>
		void Setup(TestingFactory factory);
	}

	/// <summary>
	/// Testing factory.
	/// </summary> 
	/// <remarks>
	/// Based on FactoryGirl.NET*, Plant**, and factory_girl*** for Ruby.
	/// * - http://jameskovacs.com/2012/03/20/building-factorygirl-net/
	/// ** - https://github.com/jbrechtel/plant
	/// *** - https://github.com/thoughtbot/factory_girl
	/// </remarks>
	public class TestingFactory
	{
		#region Private Fields
		private readonly Dictionary<Type, Variations> Builders = new Dictionary<Type, Variations>();

		private readonly Dictionary<string, int> SequentialValues = new Dictionary<string, int>();

		private readonly Dictionary<Type, Action<object>> DefaultPreBuildActions = new Dictionary<Type, Action<object>>();
		private readonly Dictionary<Type, Action<object>> DefaultPostBuildActions = new Dictionary<Type, Action<object>>();
		#endregion

		#region Defining
		/// <summary>
		/// Defines default builder for type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="builder"></param>
		public void Define<T>(Func<T> builder)
		{
			Validate.All(() => builder, v => v.NotNull());

			var vars = this.GetVariationsCollectionFor<T>();
			if (vars.ContainsKey(string.Empty))
				throw new DuplicatedFactoryException(typeof(T), string.Empty, "Factory for type {0} already exist.".FormatWith(typeof(T).Name));
			vars.Add(string.Empty, new BuildInfo { Builder = () => builder() });
		}

		/// <summary>
		/// Defines buulder for variation of specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="variation"></param>
		/// <param name="builder"></param>
		public void Define<T>(string variation, Func<T> builder)
		{
			Validate.All(() => variation, v => v.NotNullAndNotWhiteSpace());
			Validate.All(() => builder, v => v.NotNull());

			var vars = this.GetVariationsCollectionFor<T>();
			if (vars.ContainsKey(variation))
				throw new DuplicatedFactoryException(typeof(T), string.Empty, "Factory for type {0}(variation: {1}) already exist.".FormatWith(typeof(T).Name, variation));
			vars.Add(variation, new BuildInfo { Builder = () => builder() });
		}

		/// <summary>
		/// Defines sequential builder for specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="builder"></param>
		/// <param name="initialValue"></param>
		public void Define<T>(Func<int, T> builder, int initialValue = 1)
		{
			Validate.All(() => builder, v => v.NotNull());

			var vars = this.GetVariationsCollectionFor<T>();
			if (vars.ContainsKey(string.Empty))
				throw new DuplicatedFactoryException(typeof(T), string.Empty, "Factory for type {0} already exist.".FormatWith(typeof(T).Name));
			this.SequentialValues[typeof(T).Name] = initialValue;
			vars.Add(string.Empty, new BuildInfo { Builder = () => builder(this.SequentialValues[typeof(T).Name]++) });
		}

		/// <summary>
		/// Defines sequential builder for variation of specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="variation"></param>
		/// <param name="builder"></param>
		/// <param name="initialValue"></param>
		public void Define<T>(string variation, Func<int, T> builder, int initialValue = 1)
		{
			Validate.All(() => variation, v => v.NotNullAndNotWhiteSpace());
			Validate.All(() => builder, v => v.NotNull());

			var vars = this.GetVariationsCollectionFor<T>();
			if (vars.ContainsKey(variation))
				throw new DuplicatedFactoryException(typeof(T), string.Empty, "Factory for type {0}(variation: {1}) already exist.".FormatWith(typeof(T).Name, variation));

			string seqName = typeof(T).Name + " " + variation;
			this.SequentialValues[seqName] = initialValue;
			vars.Add(variation, new BuildInfo { Builder = () => builder(this.SequentialValues[seqName]++) });
		}
		#endregion

		#region Building
		/// <summary>
		/// Builds new object of specified type using previously defined builder.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Build<T>()
		{
			return this.InternalBuild<T>(string.Empty, x => { });
		}

		/// <summary>
		/// Builds new object of specified variation of specified type using previously defined builder.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="variation"></param>
		/// <returns></returns>
		public T Build<T>(string variation)
		{
			Validate.All(() => variation, v => v.NotNullAndNotWhiteSpace());
			return this.InternalBuild<T>(variation, x => { });
		}

		/// <summary>
		/// Builds new object of specified type using previously defined builder and overrides properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="overrides"></param>
		/// <returns></returns>
		public T Build<T>(Action<T> overrides)
		{
			Validate.All(() => overrides, v => v.NotNull());
			return this.InternalBuild<T>(string.Empty, overrides);
		}

		/// <summary>
		/// Builds new object of specified variation of specified type using previously defined builder and overrides properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="variation"></param>
		/// <param name="overrides"></param>
		/// <returns></returns>
		public T Build<T>(string variation, Action<T> overrides)
		{
			Validate.All(() => variation, v => v.NotNullAndNotWhiteSpace());
			Validate.All(() => overrides, v => v.NotNull());
			return this.InternalBuild<T>(variation, overrides);
		}

		#region BuildingMany
		/// <summary>
		/// Builds many new objects of specified type using previously defined builder.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="count">Number of new objects.</param>
		/// <returns></returns>
		public T[] BuildMany<T>(int count)
		{
			Validate.All(() => count, v => v.Min(1));
			return this.InternalBuildMany<T>(count, string.Empty, (x, i) => { });
		}

		/// <summary>
		/// Builds new object of specified variation of specified type using previously defined builder.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="count">Number of new objects.</param>
		/// <param name="variation"></param>
		/// <returns></returns>
		public T[] BuildMany<T>(int count, string variation)
		{
			Validate.All(() => count, v => v.Min(1));
			return this.InternalBuildMany<T>(count, variation, (x, i) => { });
		}

		/// <summary>
		/// Builds new object of specified type using previously defined builder and overrides properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="count">Number of new objects.</param>
		/// <param name="overrides"></param>
		/// <returns></returns>
		public T[] BuildMany<T>(int count, Action<int, T> overrides)
		{
			Validate.All(() => count, v => v.Min(1));
			Validate.All(() => overrides, v => v.NotNull());
			return this.InternalBuildMany<T>(count, string.Empty, overrides);
		}

		/// <summary>
		/// Builds new object of specified variation of specified type using previously defined builder and overrides properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="count">Number of new objects.</param>
		/// <param name="variation"></param>
		/// <param name="overrides"></param>
		/// <returns></returns>
		public T[] BuildMany<T>(int count, string variation, Action<int, T> overrides)
		{
			Validate.All(() => count, v => v.Min(1));
			Validate.All(() => variation, v => v.NotNullAndNotWhiteSpace());
			Validate.All(() => overrides, v => v.NotNull());
			return this.InternalBuildMany<T>(count, variation, overrides);
		}
		#endregion

		#endregion

		#region Pre/Post Build Actions
		/// <summary>
		/// Adds pre build action for default variation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action"></param>
		public void SetPreBuildAction<T>(Action<T> action)
		{
			var bi = this.GetBuilderFor<T>(string.Empty);
			bi.PreBuild = o => action((T)o);
		}

		/// <summary>
		/// Adds pre build action for specified variation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="variation"></param>
		/// <param name="action"></param>
		public void SetPreBuildAction<T>(string variation, Action<T> action)
		{
			var bi = this.GetBuilderFor<T>(variation);
			bi.PreBuild = o => action((T)o);
		}

		/// <summary>
		/// Adds pre build action that will be executed for all variations.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action"></param>
		public void SetDefaultPreBuildAction<T>(Action<T> action)
		{
			if (!this.Builders.ContainsKey(typeof(T)))
				throw new FactoryNotDefinedException(typeof(T), string.Empty, "There are no factories for {0}.".FormatWith(typeof(T).Name));

			this.DefaultPreBuildActions[typeof(T)] = o => action((T)o);
		}

		/// <summary>
		/// Adds post build action for default variation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action"></param>
		public void SetPostBuildAction<T>(Action<T> action)
		{
			var bi = this.GetBuilderFor<T>(string.Empty);
			bi.PostBuild = o => action((T)o);
		}

		/// <summary>
		/// Adds post build action for specified variation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="variation"></param>
		/// <param name="action"></param>
		public void SetPostBuildAction<T>(string variation, Action<T> action)
		{
			var bi = this.GetBuilderFor<T>(variation);
			bi.PostBuild = o => action((T)o);
		}

		/// <summary>
		/// Adds post build action that will be executed for all variations.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action"></param>
		public void SetDefaultPostBuildAction<T>(Action<T> action)
		{
			if (!this.Builders.ContainsKey(typeof(T)))
				throw new FactoryNotDefinedException(typeof(T), string.Empty, "There are no factories for {0}.".FormatWith(typeof(T).Name));

			this.DefaultPostBuildActions[typeof(T)] = o => action((T)o);
		}
		#endregion

		#region Misc
		/// <summary>
		/// Gets defined types.
		/// </summary>
		public IEnumerable<Type> DefinedFactories
		{
			get { return this.Builders.Keys; }
		}

		/// <summary>
		/// Clears everything(definitions, post build actions).
		/// </summary>
		public void ClearAll()
		{
			this.Builders.Clear();
			this.SequentialValues.Clear();
			this.DefaultPreBuildActions.Clear();
			this.DefaultPostBuildActions.Clear();
		}

		/// <summary>
		/// Gets variations for specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<string> GetVariationsFor<T>()
		{
			if (this.Builders.ContainsKey(typeof(T)))
				return this.Builders[typeof(T)].Keys;
			return new string[0];
		}
		#endregion

		#region Statics
		/// <summary>
		/// Creates factory with data from 1 blueprint.
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <returns></returns>
		public static TestingFactory Create<T1>()
			where T1 : IBlueprint
		{
			return Create(typeof(T1));
		}

		/// <summary>
		/// Creates factory with data from 2 blueprints.
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <returns></returns>
		public static TestingFactory Create<T1, T2>()
			where T1 : IBlueprint
			where T2 : IBlueprint
		{
			return Create(typeof(T1), typeof(T2));
		}

		/// <summary>
		/// Creates factory with data from 3 blueprints.
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <returns></returns>
		public static TestingFactory Create<T1, T2, T3>()
			where T1 : IBlueprint
			where T2 : IBlueprint
			where T3 : IBlueprint
		{
			return Create(typeof(T1), typeof(T2), typeof(T3));
		}

		/// <summary>
		/// Creates factory with data from 4 blueprints.
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <returns></returns>
		public static TestingFactory Create<T1, T2, T3, T4>()
			where T1 : IBlueprint
			where T2 : IBlueprint
			where T3 : IBlueprint
			where T4 : IBlueprint
		{
			return Create(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
		}

		/// <summary>
		/// Creates factory with data from many blueprints.
		/// </summary>
		/// <param name="blueprints"></param>
		/// <returns></returns>
		public static TestingFactory Create(params Type[] blueprints)
		{
			Validate.All(() => blueprints, v => v.NotNullAndNotEmpty().ForAll<Type>()
				.That(t => t.Implements(typeof(IBlueprint)), "Each class should implement IBlueprint interface.")
				.That(t => t.GetConstructor(Type.EmptyTypes) != null, "Each class should have parameterless constructor.")
				);

			var factory = new TestingFactory();
			foreach (var blueprint in blueprints)
				((IBlueprint)Activator.CreateInstance(blueprint)).Setup(factory);
			return factory;
		}

		/// <summary>
		/// Creates factory with data from many blueprints.
		/// </summary>
		/// <param name="blueprints"></param>
		/// <returns></returns>
		public static TestingFactory Create(params IBlueprint[] blueprints)
		{
			Validate.All(() => blueprints, v => v.NotNullAndNotEmpty());

			var factory = new TestingFactory();
			foreach (var blueprint in blueprints)
				blueprint.Setup(factory);
			return factory;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets variations collection for specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private Variations GetVariationsCollectionFor<T>()
		{
			if (this.Builders.ContainsKey(typeof(T)))
				return this.Builders[typeof(T)];
			var v = new Variations();
			this.Builders.Add(typeof(T), v);
			return v;
		}

		/// <summary>
		/// Gets builder for specified variation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="variation"></param>
		/// <returns></returns>
		private BuildInfo GetBuilderFor<T>(string variation)
		{
			try
			{
				return this.Builders[typeof(T)][variation];
			}
			catch
			{
				throw new FactoryNotDefinedException(typeof(T), variation,
					"Factory for type {0}{1} was not defined.".FormatWith(typeof(T).Name, !string.IsNullOrWhiteSpace(variation) ? "(variation: {0})".FormatWith(variation) : ""));
			}
		}

		/// <summary>
		/// Builds object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="variation"></param>
		/// <param name="overrides"></param>
		/// <returns></returns>
		private T InternalBuild<T>(string variation, Action<T> overrides)
		{
			var bi = this.GetBuilderFor<T>(variation);
			T obj = (T)bi.Builder();
			if (this.DefaultPreBuildActions.ContainsKey(typeof(T)))
				this.DefaultPreBuildActions[typeof(T)](obj);

			if (bi.PreBuild != null)
				bi.PreBuild(obj);

			overrides(obj);

			if (this.DefaultPostBuildActions.ContainsKey(typeof(T)))
				this.DefaultPostBuildActions[typeof(T)](obj);

			if (bi.PostBuild != null)
				bi.PostBuild(obj);

			return obj;
		}

		/// <summary>
		/// Builds many objects.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="count"></param>
		/// <param name="variation"></param>
		/// <param name="overrides"></param>
		/// <returns></returns>
		private T[] InternalBuildMany<T>(int count, string variation, Action<int, T> overrides)
		{
			T[] arr = new T[count];
			for (int i = 0; i < count; i++)
				arr[i] = this.InternalBuild<T>(variation, x => overrides(i, x));
			return arr;
		}
		#endregion
	}

	class BuildInfo
	{
		public Func<object> Builder;
		public Action<object> PreBuild;
		public Action<object> PostBuild;
	}
}
