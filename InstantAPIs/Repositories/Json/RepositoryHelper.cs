using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;

namespace InstantAPIs.Repositories.Json;

internal class RepositoryHelper :
	IRepositoryHelper<Context, JsonArray, JsonObject, int>
{
	private readonly Func<Context, JsonArray> _setSelector;

	public RepositoryHelper(Func<Context, JsonArray> setSelector, InstantAPIsOptions.TableOptions<JsonObject, int> config)
	{
		_setSelector = setSelector;
	}

	public Task<IEnumerable<JsonObject>> Get(HttpRequest request, Context context, string name, CancellationToken cancellationToken)
	{
		return Task.FromResult(_setSelector(context).OfType<JsonObject>());
	}

	public Task<JsonObject?> GetById(HttpRequest request, Context context, string name, int id, CancellationToken cancellationToken)
	{
		var array = context.LoadTable(name);
		var matchedItem = array.SingleOrDefault(row => row != null && row
			.AsObject()
			.Any(o => o.Key.ToLower() == "id" && o.Value?.GetValue<int>() == id)
		)?.AsObject();
		return Task.FromResult(matchedItem);
	}


	public Task<int> Insert(HttpRequest request, Context context, string name, JsonObject newObj, CancellationToken cancellationToken)
	{

		var array = context.LoadTable(name);
		var lastKey = array
			.Select(row => row?.AsObject().FirstOrDefault(o => o.Key.ToLower() == "id").Value?.GetValue<int>())
			.Select(x => x.GetValueOrDefault())
			.Max();

		var key = lastKey + 1;
		newObj.AsObject().Add("id", key);
		array.Add(newObj);
		context.SaveChanges();

		return Task.FromResult(key);
	}

	public Task Update(HttpRequest request, Context context, string name, int id, JsonObject newObj, CancellationToken cancellationToken)
	{
		var array = context.LoadTable(name);
		var matchedItem = array.SingleOrDefault(row => row != null
			&& row.AsObject().Any(o => o.Key.ToLower() == "id" && o.Value?.GetValue<int>() == id)
		)?.AsObject();
		if (matchedItem != null)
		{
			var updates = newObj
				.GroupJoin(matchedItem, o => o.Key, i => i.Key, (o, i) => new { NewValue = o, OldValue = i.FirstOrDefault() })
				.Where(x => x.NewValue.Key.ToLower() != "id")
				.ToList();
			foreach (var newField in updates)
			{
				if (newField.OldValue.Value != null)
				{
					matchedItem.Remove(newField.OldValue.Key);
				}
				matchedItem.Add(newField.NewValue.Key, JsonValue.Create(newField.NewValue.Value?.GetValue<string>()));
			}
			context.SaveChanges();
		}

		return Task.CompletedTask;
	}

	public Task<bool> Delete(HttpRequest request, Context context, string name, int id, CancellationToken cancellationToken)
	{
		var array = context.LoadTable(name);
		var matchedItem = array
			.Select((value, index) => new { value, index })
			.SingleOrDefault(row => row.value == null
				? false
				: row.value.AsObject().Any(o => o.Key.ToLower() == "id" && o.Value?.GetValue<int>() == id));
		if (matchedItem != null)
		{
			array.RemoveAt(matchedItem.index);
			context.SaveChanges();
		}

		return Task.FromResult(true);
	}

}
