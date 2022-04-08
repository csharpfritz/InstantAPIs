using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;

namespace InstantAPIs;

public static class JsonApiExtensions
{

	static JsonAPIsConfig _Config = new JsonAPIsConfig();

	public static WebApplication UseJsonRoutes(this WebApplication app, Action<JsonAPIsConfigBuilder>? options = null)
	{

		var builder = new JsonAPIsConfigBuilder();
		if (options != null)
		{
			options(builder);
			_Config = builder.Build();
		}

		var writableDoc = JsonNode.Parse(File.ReadAllText(_Config.JsonFilename))
			?? throw new Exception("Missing json file");
		var sets = writableDoc.Root?.AsObject()?.AsEnumerable();
		if (sets == null || !sets.Any()) return app;

		// print API
		foreach (var elem in sets)
		{

			var thisEntity = _Config.Tables.FirstOrDefault(t => elem.Key.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase));
			if (thisEntity == null) continue;

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.Get) == ApiMethodsToGenerate.Get)
				Console.WriteLine(string.Format("GET /{0}", elem.Key.ToLower()));

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.GetById) == ApiMethodsToGenerate.GetById)
				Console.WriteLine(string.Format("GET /{0}", elem.Key.ToLower()) + "/id");

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.Insert) == ApiMethodsToGenerate.Insert)
				Console.WriteLine(string.Format("POST /{0}", elem.Key.ToLower()));

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.Update) == ApiMethodsToGenerate.Update)
				Console.WriteLine(string.Format("PUT /{0}", elem.Key.ToLower()));

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.Delete) == ApiMethodsToGenerate.Delete)
				Console.WriteLine(string.Format("DELETE /{0}", elem.Key.ToLower()) + "/id");

			Console.WriteLine(" ");
		}

		// setup routes
		foreach (var elem in sets)
		{

			var thisEntity = _Config.Tables.FirstOrDefault(t => elem.Key.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase));
			if (thisEntity == null) continue;

			var arr = elem.Value?.AsArray() ?? new JsonArray();

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.Get) == ApiMethodsToGenerate.Get)
				app.MapGet(string.Format("/{0}", elem.Key), () => elem.Value?.ToString());

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.GetById) == ApiMethodsToGenerate.GetById)
				app.MapGet(string.Format("/{0}", elem.Key) + "/{id}", (int id) =>
				{
					var matchedItem = arr == null ? null :
					arr.SingleOrDefault(row => row != null && row
						.AsObject()
						.Any(o => o.Key.ToLower() == "id" && o.Value != null && int.Parse(o.Value.ToString()) == id)
					);
					return matchedItem == null ? Results.NotFound() : Results.Ok(matchedItem);
				});

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.Insert) == ApiMethodsToGenerate.Insert)
				app.MapPost(string.Format("/{0}", elem.Key), async (HttpRequest request) =>
				{
					string content = string.Empty;
					using (StreamReader reader = new StreamReader(request.Body))
					{
						content = await reader.ReadToEndAsync();
					}
					var newNode = JsonNode.Parse(content);
					var array = elem.Value?.AsArray();
					if (newNode == null || array == null) return Results.NotFound();
					newNode.AsObject().Add("Id", array.Count() + 1);
					array.Add(newNode);

					File.WriteAllText(_Config.JsonFilename, writableDoc.ToString());
					return Results.Ok(content);
				});

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.Update) == ApiMethodsToGenerate.Update)
				app.MapPut(string.Format("/{0}", elem.Key), async (HttpRequest request) =>
				{
					string content = string.Empty;
					using (StreamReader reader = new StreamReader(request.Body))
					{
						content = await reader.ReadToEndAsync();
					}
					var newNode = JsonNode.Parse(content);
					var array = elem.Value?.AsArray();
					if (array != null) 
					{
						array.Add(newNode);

						File.WriteAllText(_Config.JsonFilename, writableDoc.ToString());
					}

					return "OK";
				});

			if ((thisEntity.ApiMethodsToGenerate & ApiMethodsToGenerate.Delete) == ApiMethodsToGenerate.Delete)
				app.MapDelete(string.Format("/{0}", elem.Key) + "/{id}", (int id) =>
				{

					var matchedItem = arr
					 .Select((value, index) => new { value, index })
					 .SingleOrDefault(row => row.value
						?.AsObject()
						?.Any(o => o.Key.ToLower() == "id" && o.Value != null && int.Parse(o.Value.ToString()) == id)
						?? false
					);
					if (matchedItem != null)
					{
						arr.RemoveAt(matchedItem.index);
						File.WriteAllText(_Config.JsonFilename, writableDoc.ToString());
					}

					return "OK";
				});

		};

		return app;
	}
}