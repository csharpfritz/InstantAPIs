using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationExtensions
{

	private static InstantAPIsConfig Configuration { get; set; } = new();

	public static IEndpointRouteBuilder MapInstantAPIs<D>(this IEndpointRouteBuilder app, Action<InstantAPIsConfigBuilder<D>> options = null) where D: DbContext
	{

		if (app is IApplicationBuilder applicationBuilder)
		{
			var ctx = applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetService(typeof(D)) as D;
			var builder = new InstantAPIsConfigBuilder<D>(ctx);
			if (options != null)
			{
				options(builder);
				Configuration = builder.Build();
			}
		}

		// Get the tables on the DbContext
		var dbTables = GetDbTablesForContext<D>();

		var requestedTables = !Configuration.Tables.Any() ?
			dbTables :
			Configuration.Tables.Where(t => dbTables.Any(db => db.Name.Equals(t.Name, StringComparison.OrdinalIgnoreCase))).ToArray();

		var allMethods = typeof(MapApiExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name.StartsWith("Map")).ToArray();
		var initialize = typeof(MapApiExtensions).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static);
		foreach (var table in requestedTables)
		{

			// The default URL for an InstantAPI is /api/TABLENAME
			var url = $"/api/{table.Name}";

			initialize.MakeGenericMethod(typeof(D), table.InstanceType).Invoke(null, null);

			// The remaining private static methods in this class build out the Mapped API methods..
			// let's use some reflection to get them
			foreach (var method in allMethods)
			{

				var sigAttr = method.CustomAttributes.First(x => x.AttributeType == typeof(ApiMethodAttribute)).ConstructorArguments.First();
				var methodType = (ApiMethodsToGenerate)sigAttr.Value;
				if ((table.ApiMethodsToGenerate & methodType) != methodType) continue;

				var genericMethod = method.MakeGenericMethod(typeof(D), table.InstanceType);
				genericMethod.Invoke(null, new object[] { app, url });
			}

		}

		return app;
	}

	internal static IEnumerable<TypeTable> GetDbTablesForContext<D>() where D : DbContext
	{
		return typeof(D).GetProperties(BindingFlags.Instance | BindingFlags.Public)
					.Where(x => x.PropertyType.FullName.StartsWith("Microsoft.EntityFrameworkCore.DbSet"))
					.Select(x => new TypeTable { Name = x.Name, InstanceType = x.PropertyType.GenericTypeArguments.First() })
					.ToArray();
	}

	internal class TypeTable
	{
		public string Name { get; set; }
		public Type InstanceType { get; set; }
		public ApiMethodsToGenerate ApiMethodsToGenerate { get; set; } = ApiMethodsToGenerate.All;
	}

}
