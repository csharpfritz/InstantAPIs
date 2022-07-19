using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace InstantAPIs;

public static class WebApplicationExtensions
{
	
	internal const string LOGGER_CATEGORY_NAME = "InstantAPI";

	private static HashSet<InstantAPIsOptions.Table> Configuration { get; set; } = new();

	public static IEndpointRouteBuilder MapInstantAPIs<D>(this IEndpointRouteBuilder app, Action<InstantAPIsBuilder<D>>? options = null) where D : DbContext
	{
		if (app is IApplicationBuilder applicationBuilder)
		{
			AddOpenAPIConfiguration(app, options, applicationBuilder);
		}

		// Get the tables on the DbContext
		var dbTables = GetDbTablesForContext<D>();

		var requestedTables = !Configuration.Any() ?
				dbTables :
				Configuration.Where(t => dbTables.Any(db => db.Name != null && db.Name.Equals(t.Name, StringComparison.OrdinalIgnoreCase))).ToArray();

		MapInstantAPIsUsingReflection<D>(app, requestedTables);

		return app;
	}

	private static void MapInstantAPIsUsingReflection<D>(IEndpointRouteBuilder app, IEnumerable<InstantAPIsOptions.Table> requestedTables) where D : DbContext
	{

		ILogger logger = NullLogger.Instance;
		if (app.ServiceProvider != null)
		{
			var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();
			logger = loggerFactory.CreateLogger(LOGGER_CATEGORY_NAME);
		}

		var allMethods = typeof(MapApiExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name.StartsWith("Map")).ToArray();
		var initialize = typeof(MapApiExtensions).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static)
			?? throw new Exception($"{nameof(MapApiExtensions)} doest have an {nameof(MapApiExtensions.Initialize)}, this should never happen");
		foreach (var table in requestedTables)
		{

			// The default URL for an InstantAPI is /api/TABLENAME
			//var url = $"/api/{table.Name}";

			initialize.MakeGenericMethod(typeof(D), table.InstanceType ?? throw new Exception($"Instance type for table {table.Name} is null")).Invoke(null, new[] { logger });

			// The remaining private static methods in this class build out the Mapped API methods..
			// let's use some reflection to get them
			foreach (var method in allMethods)
			{

				var sigAttr = method.CustomAttributes.First(x => x.AttributeType == typeof(ApiMethodAttribute)).ConstructorArguments.First();
				if (sigAttr.Value == null) continue;
				var methodType = (ApiMethodsToGenerate)sigAttr.Value;
				if ((table.ApiMethodsToGenerate & methodType) != methodType) continue;

				var genericMethod = method.MakeGenericMethod(typeof(D), table.InstanceType);
				genericMethod.Invoke(null, new object[] { app, table.BaseUrl?.ToString() ?? throw new Exception($"BaseUrl for {table.Name} is null") });
			}

		}
	}

	private static void AddOpenAPIConfiguration<D>(IEndpointRouteBuilder app, Action<InstantAPIsBuilder<D>>? options, IApplicationBuilder applicationBuilder) where D : DbContext
	{
		// Check if AddInstantAPIs was called by getting the service options and evaluate EnableSwagger property
		var serviceOptions = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<InstantAPIsOptions>>().Value;
		if (serviceOptions == null || serviceOptions.EnableSwagger == null)
		{
			throw new ArgumentException("Call builder.Services.AddInstantAPIs(options) before MapInstantAPIs.");
		}

		var webApp = (WebApplication)app;
		if (serviceOptions.EnableSwagger == EnableSwagger.Always ||
			 (serviceOptions.EnableSwagger == EnableSwagger.DevelopmentOnly && webApp.Environment.IsDevelopment()))
		{
			applicationBuilder.UseSwagger();
			applicationBuilder.UseSwaggerUI();
		}

		var ctx = applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<D>();
		var builder = new InstantAPIsBuilder<D>(ctx);
		if (options != null)
		{
			options(builder);
			Configuration = builder.Build();
		}
	}

	internal static IEnumerable<InstantAPIsOptions.Table> GetDbTablesForContext<D>() where D : DbContext
	{
		return typeof(D).GetProperties(BindingFlags.Instance | BindingFlags.Public)
								.Where(x => (x.PropertyType.FullName?.StartsWith("Microsoft.EntityFrameworkCore.DbSet") ?? false)
											&& x.PropertyType.GenericTypeArguments.First().GetCustomAttributes(typeof(KeylessAttribute), true).Length <= 0)
								.Select(x => new InstantAPIsOptions.Table(x.Name, new Uri($"/api/{x.Name}", uriKind: UriKind.RelativeOrAbsolute), x.PropertyType.GenericTypeArguments.First()))
								.ToArray();
	}

}
