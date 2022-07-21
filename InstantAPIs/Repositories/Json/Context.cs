using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace InstantAPIs.Repositories.Json;

public class Context
{
	private readonly Options _options;
	private readonly JsonNode _writableDoc;

	public Context(IOptions<Options> options)
	{
		_options = options.Value;
		_writableDoc = JsonNode.Parse(File.ReadAllText(_options.JsonFilename))
			?? throw new Exception("Invalid json content");
	}

	public JsonArray LoadTable(string name)
	{
		return _writableDoc?.Root.AsObject().AsEnumerable().First(elem => elem.Key == name).Value as JsonArray
			?? throw new Exception("Not a json array");
	}

	internal void SaveChanges()
	{
		File.WriteAllText(_options.JsonFilename, _writableDoc.ToString());
	}

	public class Options
	{
		public string JsonFilename { get; set; } = "mock.json";
	}
}

