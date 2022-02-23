using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Fritz.InstantAPIs;

internal class MapApiExtensions
{

	// TODO: Authentication / Authorization

	[ApiMethod(ApiMethodsToGenerate.Get)]
	internal static void MapInstantGetAll<D, C>(IEndpointRouteBuilder app, string url)
		where D : DbContext where C : class
	{

		app.MapGet(url, ([FromServices] D db) =>
		{
			return db.Set<C>();
		});

	}

	[ApiMethod(ApiMethodsToGenerate.GetById)]
	internal static void MapGetById<D,C>(IEndpointRouteBuilder app, string url)
		where D: DbContext where C : class
	{

		// identify the ID field
		var theType = typeof(C);
		var idProp = theType.GetProperty("id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? theType.GetProperties().FirstOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute)));

		if (idProp == null) return;

		app.MapGet($"{url}/{{id}}", async ([FromServices] D db, [FromRoute] string id) =>
		{

			if (idProp.PropertyType == typeof(Guid))
				return await db.Set<C>().FindAsync(Guid.Parse(id));
			else if (idProp.PropertyType == typeof(int))
				return await db.Set<C>().FindAsync(int.Parse(id));
			else if (idProp.PropertyType == typeof(long))
				return await db.Set<C>().FindAsync(long.Parse(id));
			else //if (idProp.PropertyType == typeof(string))
				return await db.Set<C>().FindAsync(id);

		});


	}

	[ApiMethod(ApiMethodsToGenerate.Insert)]
	internal static void MapInstantPost<D, C>(IEndpointRouteBuilder app, string url)
		where D : DbContext where C : class
	{


		app.MapPost(url, async ([FromServices] D db, [FromBody] C newObj) =>
		{
			db.Add(newObj);
			await db.SaveChangesAsync();
		});

	}

	[ApiMethod(ApiMethodsToGenerate.Update)]
	internal static void MapInstantPut<D, C>(IEndpointRouteBuilder app, string url)
		where D : DbContext where C : class
	{


		app.MapPut($"{url}/{{id}}", async ([FromServices] D db, [FromRoute] string id, [FromBody] C newObj) =>
		{
			db.Set<C>().Attach(newObj);
			db.Entry(newObj).State = EntityState.Modified;
			await db.SaveChangesAsync();
		});

	}

	[ApiMethod(ApiMethodsToGenerate.Delete)]
	internal static void MapDeleteById<D, C>(IEndpointRouteBuilder app, string url)
		where D : DbContext where C : class
	{

		// identify the ID field
		var theType = typeof(C);
		var idProp = theType.GetProperty("id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? theType.GetProperties().FirstOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute)));

		if (idProp == null) return;

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

			if (obj == null) return;

			db.Set<C>().Remove(obj);
			await db.SaveChangesAsync();

		});


	}

}
