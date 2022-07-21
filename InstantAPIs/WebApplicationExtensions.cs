using InstantAPIs.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationExtensions
{

	internal const string LOGGER_CATEGORY_NAME = "InstantAPI";

	public static IEndpointRouteBuilder MapInstantAPIs<TContext>(this IEndpointRouteBuilder app, Action<InstantAPIsBuilder<TContext>>? options = null)
		where TContext : class
	{
		var instantApiOptions = app.ServiceProvider.GetRequiredService<IOptions<InstantAPIsOptions>>().Value;
		if (app is IApplicationBuilder applicationBuilder)
		{
			AddOpenAPIConfiguration(app, applicationBuilder);
		}

		// Get the tables on the TContext
		var contextFactory = app.ServiceProvider.GetRequiredService<IContextHelper<TContext>>();
		var builder = new InstantAPIsBuilder<TContext>(instantApiOptions, contextFactory);
		if (options != null)
		{
			options(builder);
		}
		var requestedTables = builder.Build();
		instantApiOptions.Tables = requestedTables;

		MapInstantAPIsUsingReflection<TContext>(app, requestedTables);

		return app;
	}

	private static void MapInstantAPIsUsingReflection<D>(IEndpointRouteBuilder app, IEnumerable<InstantAPIsOptions.ITable> requestedTables)
	{

		ILogger logger = NullLogger.Instance;
		if (app.ServiceProvider != null)
		{
			var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();
			logger = loggerFactory.CreateLogger(LOGGER_CATEGORY_NAME);
		}

		var allMethods = typeof(MapApiExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name.StartsWith("Map")).ToArray();
		foreach (var table in requestedTables)
		{
			// The remaining private static methods in this class build out the Mapped API methods..
			// let's use some reflection to get them
			foreach (var method in allMethods)
			{

				var sigAttr = method.CustomAttributes.First(x => x.AttributeType == typeof(ApiMethodAttribute)).ConstructorArguments.First();
				var methodType = (ApiMethodsToGenerate)(sigAttr.Value ?? throw new NullReferenceException("Missing attribute on method map"));
				if ((table.ApiMethodsToGenerate & methodType) != methodType) continue;

				var url = table.BaseUrl.ToString();

				if (table.EntitySelectorObject != null && table.ConfigObject != null)
				{
					var typesSelector = table.EntitySelectorObject.GetType().GetGenericArguments();
					if (typesSelector.Length == 1 && typesSelector[0].IsGenericType)
					{
						typesSelector = typesSelector[0].GetGenericArguments();
					}
					var typesConfig = table.ConfigObject.GetType().GetGenericArguments();
					var genericMethod = method.MakeGenericMethod(typesSelector[0], typesSelector[1], typesConfig[0], typesConfig[1]);
					genericMethod.Invoke(null, new object[] { app, url, table.Name });
				}
			}
		}
	}

	private static void AddOpenAPIConfiguration(IEndpointRouteBuilder app, IApplicationBuilder applicationBuilder)
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
	}
}
