using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace InstantAPIs;

internal class MapApiExtensions
{

	// TODO: Authentication / Authorization
	private static Dictionary<Type, PropertyInfo> _IdLookup = new();

  private static ILogger Logger;

	internal static void Initialize<D,C>(ILogger logger) 
		where D: DbContext
		where C: class 
	{

		Logger = logger;

    var theType = typeof(C);
		var idProp = theType.GetProperty("id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? theType.GetProperties().FirstOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute)));

		if (idProp != null)
		{
			_IdLookup.Add(theType, idProp);
		}

	}

	[ApiMethod(ApiMethodsToGenerate.Get)]
	internal static void MapInstantGetAll<D, C>(IEndpointRouteBuilder app, string url)
		where D : DbContext where C : class
	{

		Logger.LogInformation($"Created API: HTTP GET\t{url}");
		app.MapGet(url, ([FromServices] D db) =>
		{
			return Results.Ok(db.Set<C>());
		});

	}

	[ApiMethod(ApiMethodsToGenerate.GetById)]
	internal static void MapGetById<D,C>(IEndpointRouteBuilder app, string url)
		where D: DbContext where C : class
	{

		// identify the ID field
		var theType = typeof(C);
		var idProp = _IdLookup[theType];

		if (idProp == null) return;

		Logger.LogInformation($"Created API: HTTP GET\t{url}/{{id}}");

		app.MapGet($"{url}/{{id}}", async ([FromServices] D db, [FromRoute] string id) =>
		{

			C outValue = default(C);
			if (idProp.PropertyType == typeof(Guid))
				outValue = await db.Set<C>().FindAsync(Guid.Parse(id));
			else if (idProp.PropertyType == typeof(int))
				outValue = await db.Set<C>().FindAsync(int.Parse(id));
			else if (idProp.PropertyType == typeof(long))
				outValue = await db.Set<C>().FindAsync(long.Parse(id));
			else //if (idProp.PropertyType == typeof(string))
				outValue = await db.Set<C>().FindAsync(id);

			if (outValue is null) return Results.NotFound();
			return Results.Ok(outValue);
		});


	}

	[ApiMethod(ApiMethodsToGenerate.Insert)]
	internal static void MapInstantPost<D, C>(IEndpointRouteBuilder app, string url)
		where D : DbContext where C : class
	{

		Logger.LogInformation($"Created API: HTTP POST\t{url}");

		app.MapPost(url, async ([FromServices] D db, [FromBody] C newObj) =>
		{
			db.Add(newObj);
			await db.SaveChangesAsync();
			var id = _IdLookup[typeof(C)].GetValue(newObj);
			return Results.Created($"{url}/{id.ToString()}", newObj);
		});

	}
	
	[ApiMethod(ApiMethodsToGenerate.Update)]
	internal static void MapInstantPut<D, C>(IEndpointRouteBuilder app, string url)
		where D : DbContext where C : class
	{

		Logger.LogInformation($"Created API: HTTP PUT\t{url}");

		app.MapPut($"{url}/{{id}}", async ([FromServices] D db, [FromRoute] string id, [FromBody] C newObj) =>
		{
			db.Set<C>().Attach(newObj);
			db.Entry(newObj).State = EntityState.Modified;
			await db.SaveChangesAsync();
			return Results.NoContent();
		});

	}

	[ApiMethod(ApiMethodsToGenerate.Delete)]
	internal static void MapDeleteById<D, C>(IEndpointRouteBuilder app, string url)
		where D : DbContext where C : class
	{

		// identify the ID field
		var theType = typeof(C);
		var idProp = _IdLookup[theType];

		if (idProp == null) return;
		Logger.LogInformation($"Created API: HTTP DELETE\t{url}");

		app.MapDelete($"{url}/{{id}}", async ([FromServices] D db, [FromRoute] string id) =>
		{

			var set = db.Set<C>();
			C? obj;

			if (idProp.PropertyType == typeof(Guid))
				obj = await set.FindAsync(Guid.Parse(id));
			else if (idProp.PropertyType == typeof(int))
				obj = await set.FindAsync(int.Parse(id));
			else if (idProp.PropertyType == typeof(long))
				obj = await set.FindAsync(long.Parse(id));
			else //if (idProp.PropertyType == typeof(string))
				obj = await set.FindAsync(id);

			if (obj == null) return Results.NotFound();

			db.Set<C>().Remove(obj);
			await db.SaveChangesAsync();
			return Results.NoContent();

		});


	}

}
