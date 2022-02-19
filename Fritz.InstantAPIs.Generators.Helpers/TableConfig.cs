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

		public Func<string?, string> Route { get; set; } = value => $"/api/{value}";

		public Func<string?, string> RouteById { get; set; } = value => $"/api/{value}/{{id}}";
	}
}