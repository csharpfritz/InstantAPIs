using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using InstantAPIs.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace InstantAPIs;

internal partial class MapApiExtensions
{
	public static ILogger Logger = NullLogger.Instance;
	// TODO: Authentication / Authorization

	[ApiMethod(ApiMethodsToGenerate.Get)]
	internal static void MapInstantGetAll<TContext, TSet, TEntity, TKey>(IEndpointRouteBuilder app, string url, string name)
		where TContext : class
		where TSet : class
		where TEntity : class
	{
		app.MapGet(url, async (HttpRequest request, [FromServices] TContext context, [FromServices] IRepositoryHelperFactory<TContext, TSet, TEntity, TKey> repository,
			CancellationToken cancellationToken) =>
		{
			return Results.Ok(await repository.Get(request, context, name, cancellationToken));
		});
		Logger.LogInformation($"Created API: HTTP GET\t{url}");
	}

	[ApiMethod(ApiMethodsToGenerate.GetById)]
	internal static void MapGetById<TContext, TSet, TEntity, TKey>(IEndpointRouteBuilder app, string url, string name)
		where TContext : class
		where TSet : class
		where TEntity : class
	{
		app.MapGet($"{url}/{{id}}", async (HttpRequest request, [FromServices] TContext context, [FromRoute] TKey id,
			[FromServices] IRepositoryHelperFactory<TContext, TSet, TEntity, TKey> repository, CancellationToken cancellationToken) =>
		{
			var outValue = await repository.GetById(request, context, name, id, cancellationToken);
			if (outValue is null) return Results.NotFound();
			return Results.Ok(outValue);
		});
		Logger.LogInformation($"Created API: HTTP GET\t{url}/{{id}}");
	}

	[ApiMethod(ApiMethodsToGenerate.Insert)]
	internal static void MapInstantPost<TContext, TSet, TEntity, TKey>(IEndpointRouteBuilder app, string url, string name)
		where TContext : class
		where TSet : class
		where TEntity : class
	{
		app.MapPost(url, async (HttpRequest request, [FromServices] TContext context, [FromBody] TEntity newObj,
			[FromServices] IRepositoryHelperFactory<TContext, TSet, TEntity, TKey> repository, CancellationToken cancellationToken) =>
		{
			var id = await repository.Insert(request, context, name, newObj, cancellationToken);
			return Results.Created($"{url}/{id}", newObj);
		});
		Logger.LogInformation($"Created API: HTTP POST\t{url}");
	}
	
	[ApiMethod(ApiMethodsToGenerate.Update)]
	internal static void MapInstantPut<TContext, TSet, TEntity, TKey>(IEndpointRouteBuilder app, string url, string name)
		where TContext : class
		where TSet : class
		where TEntity : class
	{
		app.MapPut($"{url}/{{id}}", async (HttpRequest request, [FromServices] TContext context, [FromRoute] TKey id, [FromBody] TEntity newObj,
			[FromServices] IRepositoryHelperFactory<TContext, TSet, TEntity, TKey> repository, CancellationToken cancellationToken) =>
		{
			await repository.Update(request, context, name, id, newObj, cancellationToken);
			return Results.NoContent();
		});
		Logger.LogInformation($"Created API: HTTP PUT\t{url}");
	}

	[ApiMethod(ApiMethodsToGenerate.Delete)]
	internal static void MapDeleteById<TContext, TSet, TEntity, TKey>(IEndpointRouteBuilder app, string url, string name)
		where TContext : class
		where TSet : class
		where TEntity : class
	{
		app.MapDelete($"{url}/{{id}}", async (HttpRequest request, [FromServices] TContext context, [FromRoute] TKey id,
			[FromServices] IRepositoryHelperFactory<TContext, TSet, TEntity, TKey> repository, CancellationToken cancellationToken) =>
		{
			return await repository.Delete(request, context, name, id, cancellationToken)
				? Results.NoContent()
				: Results.NotFound();
		});
		Logger.LogInformation($"Created API: HTTP DELETE\t{url}");
	}

}
