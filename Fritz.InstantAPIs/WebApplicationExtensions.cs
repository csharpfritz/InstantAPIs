using System.Reflection;

namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationExtensions
{

	public static InstantAPIsConfig Configuration { get; set; } = new();

	public static WebApplication MapInstantAPIs<D>(this WebApplication app, Func<InstantAPIsConfig> configAction = null) where D: DbContext
	{

		Configuration = configAction != null ? configAction() : new();

		// Get the tables on the DbContext
		var dbTables = typeof(D).GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(x => x.PropertyType.FullName.StartsWith("Microsoft.EntityFrameworkCore.DbSet"))
			.Select(x => new TypeTable { Name = x.Name, InstanceType = x.PropertyType.GenericTypeArguments.First() });

		var requestedTables = Configuration?.Tables ?? InstantAPIsConfig.DefaultTables;

		foreach (var table in dbTables.Where(
			x => (requestedTables == InstantAPIsConfig.DefaultTables) || requestedTables.Any(t => t.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase))
			)
		)
		{

			// The default URL for an InstantAPI is /api/TABLENAME
			var url = $"/api/{table.Name}";

			// The remaining private static methods in this class build out the Mapped API methods..
			// let's use some reflection to get them
			var allMethods = typeof(MapApiExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name.StartsWith("Map"));
			foreach (var method in allMethods)
			{
				var genericMethod = method.MakeGenericMethod(typeof(D), table.InstanceType);
				genericMethod.Invoke(null, new object[] { app, url });
			}

		}

		return app;
	}

	internal class TypeTable
	{
		public string Name { get; set; }
		public Type InstanceType { get; set; }
	}

}
