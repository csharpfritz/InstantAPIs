namespace Microsoft.AspNetCore.Builder;

public class InstantAPIsBuilder<D> where D : DbContext
{

	private HashSet<InstantAPIsOptions.Table> _Config = new();
	private Type _ContextType = typeof(D);
	private D _TheContext;
	private readonly HashSet<InstantAPIsOptions.Table> _IncludedTables = new();
	private readonly List<string> _ExcludedTables = new();
	private const string DEFAULT_URI = "/api/";

	public InstantAPIsBuilder(D theContext)
	{
		this._TheContext = theContext;
	}

	#region Table Inclusion/Exclusion

	/// <summary>
	/// Specify individual tables to include in the API generation with the methods requested
	/// </summary>
	/// <param name="entitySelector">Select the EntityFramework DbSet to include - Required</param>
	/// <param name="methodsToGenerate">A flags enumerable indicating the methods to generate.  By default ALL are generated</param>
	/// <returns>Configuration builder with this configuration applied</returns>
	public InstantAPIsBuilder<D> IncludeTable<T>(Func<D, DbSet<T>> entitySelector, ApiMethodsToGenerate methodsToGenerate = ApiMethodsToGenerate.All, string baseUrl = "") where T : class
	{

		var theSetType = entitySelector(_TheContext).GetType().BaseType;
		var property = _ContextType.GetProperties().First(p => p.PropertyType == theSetType);

		if (!string.IsNullOrEmpty(baseUrl))
		{
			try
			{
				var testUri = new Uri(baseUrl, UriKind.RelativeOrAbsolute);
				baseUrl = testUri.IsAbsoluteUri ? testUri.LocalPath : baseUrl;
			}
			catch
			{
				throw new ArgumentException(nameof(baseUrl), "Not a valid Uri");
			}
		}
		else
		{
			baseUrl = string.Concat(DEFAULT_URI, property.Name);
		}

		var tableApiMapping = new InstantAPIsOptions.Table(property.Name, new Uri(baseUrl), typeof(T)) { ApiMethodsToGenerate = methodsToGenerate };
		_IncludedTables.Add(tableApiMapping);

		if (_ExcludedTables.Contains(tableApiMapping.Name)) _ExcludedTables.Remove(tableApiMapping.Name);
		_IncludedTables.Add(tableApiMapping);

		return this;

	}

	/// <summary>
	/// Exclude individual tables from the API generation.  Exclusion takes priority over inclusion
	/// </summary>
	/// <param name="entitySelector">Select the entity to exclude from generation</param>
	/// <returns>Configuration builder with this configuraiton applied</returns>
	public InstantAPIsBuilder<D> ExcludeTable<T>(Func<D, DbSet<T>> entitySelector) where T : class
	{

		var theSetType = entitySelector(_TheContext).GetType().BaseType;
		var property = _ContextType.GetProperties().First(p => p.PropertyType == theSetType);

		if (_IncludedTables.Select(t => t.Name).Contains(property.Name)) _IncludedTables.Remove(_IncludedTables.First(t => t.Name == property.Name));
		_ExcludedTables.Add(property.Name);

		return this;

	}

	private void BuildTables()
	{

		var tables = WebApplicationExtensions.GetDbTablesForContext<D>().ToArray();
		InstantAPIsOptions.Table[]? outTables;

		// Add the Included tables
		if (_IncludedTables.Any())
		{
			outTables = tables.Where(t => _IncludedTables.Any(i => i.Name.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)))
				.Select(t => new InstantAPIsOptions.Table(t.Name, new Uri(_IncludedTables.First(i => i.Name.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)).BaseUrl.ToString(), UriKind.Relative), t.InstanceType)
				{
					ApiMethodsToGenerate = _IncludedTables.First(i => i.Name.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)).ApiMethodsToGenerate
				}).ToArray();
		} else { 
			outTables = tables.Select(t => new InstantAPIsOptions.Table(t.Name, new Uri(DEFAULT_URI + t.Name, uriKind: UriKind.Relative), t.InstanceType)).ToArray();
		}

		// Exit now if no tables were excluded
		if (!_ExcludedTables.Any())
		{
			_Config.UnionWith(outTables);
			return;
		}

		// Remove the Excluded tables
		outTables = outTables.Where(t => !_ExcludedTables.Any(e => e.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase))).ToArray();

		if (outTables == null || !outTables.Any()) throw new ArgumentException("All tables were excluded from this configuration");

		_Config.UnionWith(outTables);

	}

#endregion

	internal HashSet<InstantAPIsOptions.Table> Build()
	{

		BuildTables();

		return _Config;
	}

}