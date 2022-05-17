namespace InstantAPIs.Generators.Helpers
{
	public sealed class TableConfig<T>
		where T : struct, Enum
	{
		public TableConfig(T key)
		{
			Key = key;
			Name = Enum.GetName(key);
		}

		public TableConfig(T key, Included included, string? name = null, ApisToGenerate apis = ApisToGenerate.All,
			Func<string?, string>? routeGet = null, Func<string?, string>? routeGetById = null,
			Func<string?, string>? routePost = null, Func<string?, string>? routePut = null,
			Func<string?, string>? routeDeleteById = null)
			: this(key)
		{
			Included = included;
			APIs = apis;
			if (!string.IsNullOrWhiteSpace(name)) { Name = name; }
			if (routeGet is not null) { RouteGet = routeGet; }
			if (routeGetById is not null) { RouteGetById = routeGetById; }
			if (routePost is not null) { RoutePost = routePost; }
			if (routePut is not null) { RoutePut = routePut; }
			if (routeDeleteById is not null) { RouteDeleteById = routeDeleteById; }
		}

		public T Key { get; }

		public string? Name { get; } = null;

		public Included Included { get; } = Included.Yes;

		public ApisToGenerate APIs { get; } = ApisToGenerate.All;

		public Func<string?, string> RouteDeleteById { get; } = value => $"/api/{value}/{{id}}";

		public Func<string?, string> RouteGet { get; } = value => $"/api/{value}";

		public Func<string?, string> RouteGetById { get; } = value => $"/api/{value}/{{id}}";

		public Func<string?, string> RoutePost { get; } = value => $"/api/{value}";

		public Func<string?, string> RoutePut { get; } = value => $"/api/{value}/{{id}}";
	}
}