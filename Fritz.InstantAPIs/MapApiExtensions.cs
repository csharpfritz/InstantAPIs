using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Fritz.InstantAPIs;

internal class MapApiExtensions
{

	// TODO: Authentication / Authorization

	internal static void MapInstantGetAll<D, C>(WebApplication app, string url)
		where D : DbContext where C : class
	{

		app.MapGet(url, ([FromServices] D db) =>
		{
			return db.Set<C>();
		});

	}

	internal static void MapGetById<D,C>(WebApplication app, string url)
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

	internal static void MapInstantPost<D, C>(WebApplication app, string url)
		where D : DbContext where C : class
	{


		app.MapPost(url, async ([FromServices] D db, [FromBody] C newObj) =>
		{
			db.Add(newObj);
			await db.SaveChangesAsync();
		});

	}

	// TODO: MapInstantPut()

	internal static void MapDeleteById<D, C>(WebApplication app, string url)
		where D : DbContext where C : class
	{

		// identify the ID field
		var theType = typeof(C);
		var idProp = theType.GetProperty("id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? theType.GetProperties().FirstOrDefault(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute)));

		if (idProp == null) return;

		app.MapGet($"{url}/{{id}}", async ([FromServices] D db, [FromRoute] string id) =>
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
