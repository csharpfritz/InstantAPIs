namespace Fritz.InstantAPIs.Generators.Helpers
{
	public class InstanceAPIGeneratorConfig
	{
		public static InstanceAPIGeneratorConfig Default => new();

		public virtual string GetRoute(string tableName) => $"/api/{tableName}";

		public virtual string GetRouteById(string tableName) => $"/api/{tableName}/{{id}}";

		public virtual bool ShouldGetAll => true;

		public virtual bool ShouldGetById => true;
	}
}