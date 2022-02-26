using System.Collections.Immutable;

namespace Fritz.InstantAPIs.Generators.Helpers
{
	public class InstanceAPIGeneratorConfig<T>
		where T : struct, Enum
	{
		private readonly ImmutableDictionary<T, TableConfig<T>> _tablesConfig;

		internal InstanceAPIGeneratorConfig(ImmutableDictionary<T, TableConfig<T>> tablesConfig) 
		{ 
			_tablesConfig = tablesConfig ?? throw new ArgumentNullException(nameof(tablesConfig));
		}

		public virtual TableConfig<T> this[T key] => _tablesConfig[key];
	}
}