using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace InstantAPIs.Repositories.Json;

internal class JsonRepositoryHelper :
	IRepositoryHelper<Context, JsonArray, JsonObject, int>
{
	private readonly Func<Context, JsonArray> _setSelector;
	private readonly InstantAPIsOptions.TableOptions<JsonObject, int> _config;

	public JsonRepositoryHelper(Func<Context, JsonArray> setSelector, InstantAPIsOptions.TableOptions<JsonObject, int> config)
	{
		_setSelector = setSelector;
		_config = config;
	}

	public Task<IEnumerable<JsonObject>> Get(HttpRequest request, Context context, string name, CancellationToken cancellationToken)
	{
		return Task.FromResult(_setSelector(context).OfType<JsonObject>());
	}

	public Task<JsonObject?> GetById(HttpRequest request, Context context, string name, int id, CancellationToken cancellationToken)
	{
		var array = context.LoadTable(name);
		var matchedItem = array.SingleOrDefault(row => (row ?? throw new Exception("No row found"))
			.AsObject()
			.Any(o => o.Key.ToLower() == "id" && o.Value?.ToString() == id.ToString())
		)?.AsObject();
		return Task.FromResult(matchedItem);
	}


	public Task<int> Insert(HttpRequest request, Context context, string name, JsonObject newObj, CancellationToken cancellationToken)
	{

		var array = context.LoadTable(name);
		var key = array.Count + 1;
		newObj.AsObject().Add("Id", key.ToString());
		array.Add(newObj);
		context.SaveChanges();

		return Task.FromResult(key);
	}

	public Task Update(HttpRequest request, Context context, string name, int id, JsonObject newObj, CancellationToken cancellationToken)
	{
		var array = context.LoadTable(name);
		array.Add(newObj);
		context.SaveChanges();

		return Task.CompletedTask;
	}

	public Task<bool> Delete(HttpRequest request, Context context, string name, int id, CancellationToken cancellationToken)
	{
		var array = context.LoadTable(name);
		var matchedItem = array
			.Select((value, index) => new { value, index })
			.SingleOrDefault(row => (row.value ?? throw new Exception("No json value found"))
				.AsObject()
				.Any(o => o.Key.ToLower() == "id" && o.Value?.ToString() == id.ToString()));
		if (matchedItem != null)
		{
			array.RemoveAt(matchedItem.index);
			context.SaveChanges();
		}

		return Task.FromResult(true);
	}
}
