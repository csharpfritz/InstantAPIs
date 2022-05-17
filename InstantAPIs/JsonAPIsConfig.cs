using System.Text.Json.Nodes;

namespace InstantAPIs;

internal class JsonAPIsConfig
{

	internal HashSet<WebApplicationExtensions.TypeTable> Tables { get; } = new HashSet<WebApplicationExtensions.TypeTable>();

	internal string JsonFilename = "mock.json";

}


public class JsonAPIsConfigBuilder
{

	private JsonAPIsConfig _Config = new();
	private string _FileName;
	private readonly HashSet<TableApiMapping> _IncludedTables = new();
	private readonly List<string> _ExcludedTables = new();

	public JsonAPIsConfigBuilder SetFilename(string fileName)
	{
		_FileName = fileName;
		return this;
	}

	#region Table Inclusion/Exclusion

	/// <summary>
	/// Specify individual entities to include in the API generation with the methods requested
	/// </summary>
	/// <param name="entityName">Name of the JSON entity collection to include</param>
	/// <param name="methodsToGenerate">A flags enumerable indicating the methods to generate.  By default ALL are generated</param>
	/// <returns>Configuration builder with this configuration applied</returns>
	public JsonAPIsConfigBuilder IncludeEntity(string entityName, ApiMethodsToGenerate methodsToGenerate = ApiMethodsToGenerate.All)
	{

		var tableApiMapping = new TableApiMapping(entityName, methodsToGenerate);
		_IncludedTables.Add(tableApiMapping);

		if (_ExcludedTables.Contains(entityName)) _ExcludedTables.Remove(tableApiMapping.TableName);

		return this;

	}

	/// <summary>
	/// Exclude individual entities from the API generation.  Exclusion takes priority over inclusion
	/// </summary>
	/// <param name="entitySelector">Name of the JSON entity collection to exclude</param>
	/// <returns>Configuration builder with this configuraiton applied</returns>
	public JsonAPIsConfigBuilder ExcludeTable(string entityName)
	{

		if (_IncludedTables.Select(t => t.TableName).Contains(entityName)) _IncludedTables.Remove(_IncludedTables.First(t => t.TableName == entityName));
		_ExcludedTables.Add(entityName);

		return this;

	}

	private HashSet<string> IdentifyEntities()
	{
		var writableDoc = JsonNode.Parse(File.ReadAllText(_FileName));

		// print API
		return writableDoc?.Root.AsObject()
			.AsEnumerable().Select(x => x.Key)
			.ToHashSet();

	}

	private void BuildTables()
	{

		var tables = IdentifyEntities();

		if (!_IncludedTables.Any() && !_ExcludedTables.Any())
		{
			_Config.Tables.UnionWith(tables.Select(t => new WebApplicationExtensions.TypeTable
			{
				Name = t,
				ApiMethodsToGenerate = ApiMethodsToGenerate.All
			}));
			return;
		}

		// Add the Included tables
		var outTables = _IncludedTables
			.Select(t => new WebApplicationExtensions.TypeTable
			{
				Name = t.TableName,
				ApiMethodsToGenerate = t.MethodsToGenerate
			}).ToArray();

		// If no tables were added, added them all
		if (outTables.Length == 0)
		{
			outTables = tables.Select(t => new WebApplicationExtensions.TypeTable
			{
				Name = t,
				ApiMethodsToGenerate = ApiMethodsToGenerate.All
			}).ToArray();
		}

		// Remove the Excluded tables
		outTables = outTables.Where(t => !_ExcludedTables.Any(e => t.Name.Equals(e, StringComparison.InvariantCultureIgnoreCase))).ToArray();

		if (outTables == null || !outTables.Any()) throw new ArgumentException("All tables were excluded from this configuration");

		_Config.Tables.UnionWith(outTables);

	}

#endregion

	internal JsonAPIsConfig Build()
	{

		if (string.IsNullOrEmpty(_FileName)) throw new ArgumentNullException("Missing Json Filename for configuration");
		if (!File.Exists(_FileName)) throw new ArgumentException($"Unable to locate the JSON file for APIs at {_FileName}");
		_Config.JsonFilename = _FileName;

		BuildTables();

		return _Config;
	}

}