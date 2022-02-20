namespace Fritz.InstantAPIs.Generators.Helpers
{
	public sealed class TableConfig<T>
		where T : Enum
	{
		public TableConfig(T key) =>
			Key = key;

		public T Key { get; }

		public string? Name { get; set; }

		public Included Included { get; set; }

		public ApisToGenerate APIs { get; set; }

		public Func<string?, string> RouteDeleteById { get; set; } = value => $"/api/{value}/{{id}}";

		public Func<string?, string> RouteGet { get; set; } = value => $"/api/{value}";

		public Func<string?, string> RouteGetById { get; set; } = value => $"/api/{value}/{{id}}";

		public Func<string?, string> RoutePost { get; set; } = value => $"/api/{value}";

		public Func<string?, string> RoutePut { get; set; } = value => $"/api/{value}/{{id}}";
	}
}