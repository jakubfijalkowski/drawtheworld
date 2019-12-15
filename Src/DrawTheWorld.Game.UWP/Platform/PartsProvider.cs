using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DrawTheWorld.Core;
using DrawTheWorld.Core.Attributes;
using DrawTheWorld.Core.Platform;
using FLib;

namespace DrawTheWorld.Game.Platform
{
	/// <summary>
	/// Default implementation of the <see cref="IPartsProvider"/>.
	/// </summary>
	sealed class PartsProvider
		: IPartsProvider
	{
		private const string NamePrefix = "Platform_ToolNames_";
		private const string ToolLookPath = "ms-appx:///Assets/Tools/{0}.png";

		private readonly Dictionary<Type, Core.IToolBehavior> Behaviors = new Dictionary<Type, Core.IToolBehavior>();
		private readonly IReadOnlyList<IGameMode> _AvailableGameModes = ListAvailableModes().ToList();

		/// <inheritdoc />
		public IReadOnlyList<IGameMode> AvailableGameModes
		{
			get { return this._AvailableGameModes; }
		}

		/// <inheritdoc />
		public Core.IToolBehavior GetBehavior(Core.ITool tool)
		{
			var behaviorType = BindBehaviorAttribute.GetBehaviorType(tool.GetType());
			
			Core.IToolBehavior behavior = null;
			if (!this.Behaviors.TryGetValue(behaviorType, out behavior))
			{
				behavior = Activator.CreateInstance(behaviorType) as Core.IToolBehavior;
				this.Behaviors.Add(behaviorType, behavior);
			}
			return behavior;
		}

		/// <inheritdoc />
		public string GetName(Core.ITool tool)
		{
			var resource = NamePrefix + ToolNameAttribute.GetToolName(tool.GetType());
			return Strings.Get(resource);
		}

		/// <inheritdoc />
		public Uri GetLook(Core.ITool tool)
		{
			return new Uri(ToolLookPath.FormatWith(ToolNameAttribute.GetToolName(tool.GetType())));
		}

		private static IEnumerable<Core.IGameMode> ListAvailableModes()
		{
			return from t in typeof(Core.Game).Assembly.ExportedTypes
				   where t != typeof(Core.GameModes.DesignerMode)
				   where t.Implements(typeof(Core.IGameMode)) && !t.IsGenericType && !t.IsAbstract && t.GetConstructors().Contains(c => c.GetParameters().Length == 0)
				   select (Core.IGameMode)Activator.CreateInstance(t);
		}
	}
}
