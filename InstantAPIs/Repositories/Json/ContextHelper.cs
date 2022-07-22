using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Text.Json.Nodes;

namespace InstantAPIs.Repositories.Json;

public class ContextHelper :
	IContextHelper
{
	private readonly IOptions<Context.Options> _options;

	public ContextHelper(IOptions<Context.Options> options)
	{
		_options = options;
	}

	public bool IsValidFor(Type contextType) => contextType.IsAssignableTo(typeof(Context));

	public IEnumerable<InstantAPIsOptions.ITable> DiscoverFromContext<TContext>(Uri baseUrl)
	{
		var doc = JsonNode.Parse(File.ReadAllText(_options.Value.JsonFilename));
		var tables = doc?.Root.AsObject().AsEnumerable() ?? throw new Exception("No json file found");
		return tables.Select(x => new InstantAPIsOptions.Table<Context, JsonArray, JsonObject, int>(
			x.Key, new Uri($"{baseUrl.OriginalString}/{x.Key}", UriKind.Relative), c => c.LoadTable(x.Key),
			new InstantAPIsOptions.TableOptions<JsonObject, int>()));
	}

	public string NameTable<TContext, TSet>(Expression<Func<TContext, TSet>> setSelector)
	{
		return setSelector.Body.NodeType == ExpressionType.Call
				&& setSelector.Body is MethodCallExpression methodExpression
				&& methodExpression.Arguments.Count == 1
				&& methodExpression.Arguments.First() is ConstantExpression constantExpression
				&& constantExpression.Value != null
			? (constantExpression.Value.ToString() ?? string.Empty)
			: throw new ArgumentException(nameof(setSelector.Body.DebugInfo), "Not a valid expression");
	}
}
