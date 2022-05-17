using System.Collections.Immutable;

namespace InstantAPIs.Generators.Helpers
{
	public sealed class InstanceAPIGeneratorConfigBuilder<T>
		where T : struct, Enum
	{
		private readonly Dictionary<T, TableConfig<T>> _tablesConfig = new();

		public InstanceAPIGeneratorConfigBuilder()
		{
			foreach(var key in Enum.GetValues<T>())
			{
				_tablesConfig.Add(key, new TableConfig<T>(key));
			}
		}

		public InstanceAPIGeneratorConfigBuilder<T> Include(T key, string? name = null, ApisToGenerate apis = ApisToGenerate.All,
			Func<string?, string>? routeGet = null, Func<string?, string>? routeGetById = null,
			Func<string?, string>? routePost = null, Func<string?, string>? routePut = null,
			Func<string?, string>? routeDeleteById = null)
		{
			_tablesConfig[key] = new TableConfig<T>(key, Included.Yes, name: name,
				apis: apis, routeGet: routeGet, routeGetById: routeGetById, 
				routePost: routePost, routePut: routePut, routeDeleteById: routeDeleteById);
			return this;
		}

		public InstanceAPIGeneratorConfigBuilder<T> Exclude(T key)
		{
			_tablesConfig[key] = new TableConfig<T>(key, Included.No);
			return this;
		}

		public InstanceAPIGeneratorConfig<T> Build() => 
			new InstanceAPIGeneratorConfig<T>(_tablesConfig.ToImmutableDictionary());
	}
}