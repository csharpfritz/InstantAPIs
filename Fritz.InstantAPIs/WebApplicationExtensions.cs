using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationExtensions
{

	public static WebApplication MapInstantAPIs<D>(this WebApplication app, Func<InstantAPIsConfig> configAction = null) where D: DbContext
	{

		// Get the tables on the DbContext
		var dbTables = typeof(D).GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(x => x.PropertyType.FullName.StartsWith("Microsoft.EntityFrameworkCore.DbSet"))
			.Select(x => new TypeTable { Name = x.Name, InstanceType = x.PropertyType.GenericTypeArguments.First() });

		var config = configAction != null ? configAction() : new InstantAPIsConfig();
		var requestedTables = config?.Tables ?? InstantAPIsConfig.DefaultTables;

		foreach (var table in dbTables.Where(
			x => (requestedTables == InstantAPIsConfig.DefaultTables) || requestedTables.Any(t => t.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase))
			)
		)
		{

			// The default URL for an InstantAPI is /api/TABLENAME
			var url = $"/api/{table.Name}";

			// The remaining private static methods in this class build out the Mapped API methods..
			// let's use some reflection to get them
			var allMethods = typeof(WebApplicationExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
			foreach (var method in allMethods)
			{
				var genericMethod = method.MakeGenericMethod(typeof(D), table.InstanceType);
				genericMethod.Invoke(null, new object[] { app, url });
			}

		}

		return app;
	}

	// TODO: Authentication / Authorization

	private static void MapInstantGetAll<D,C>(WebApplication app, string url) 
		where D : DbContext where C : class
	{

		app.MapGet(url, ([FromServices] D db) =>
		{
			return db.Set<C>();
		});

	}

	// TODO: MapInstantGetById()

	private static void MapInstantPost<D,C>(WebApplication app, string url) 
		where D : DbContext where C : class
	{


		app.MapPost(url, async ([FromServices] D db, [FromBody] C newObj) =>
		{
			db.Add(newObj);
			await db.SaveChangesAsync();
		});

	}

	// TODO: MapInstantPut()

	// TODO: MapInstantDelete()


	private class TypeTable
	{
		public string Name { get; set; }
		public Type InstanceType { get; set; }
	}

}

public class InstantAPIsConfig
{

	public static readonly string[] DefaultTables = new[] { "all" };

	public string[] Tables { get; set; } = DefaultTables;

}