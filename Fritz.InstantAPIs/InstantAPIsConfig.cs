namespace Microsoft.AspNetCore.Builder;

internal class InstantAPIsConfig
{

	internal HashSet<WebApplicationExtensions.TypeTable> Tables { get; } = new HashSet<WebApplicationExtensions.TypeTable>();

}


public class InstantAPIsConfigBuilder<D> where D : DbContext
{

	private InstantAPIsConfig _Config = new();
	private Type _ContextType = typeof(D);
	private D _TheContext;
	private readonly HashSet<TableApiMapping> _IncludedTables = new();
	private readonly List<string> _ExcludedTables = new();

	public InstantAPIsConfigBuilder(D theContext)
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
	public InstantAPIsConfigBuilder<D> IncludeTable<T>(Func<D, DbSet<T>> entitySelector, ApiMethodsToGenerate methodsToGenerate = ApiMethodsToGenerate.All) where T : class
	{

		var theSetType = entitySelector(_TheContext).GetType().BaseType;
		var property = _ContextType.GetProperties().First(p => p.PropertyType == theSetType);

		var tableApiMapping = new TableApiMapping(property.Name, methodsToGenerate);
		_IncludedTables.Add(tableApiMapping);

		if (_ExcludedTables.Contains(tableApiMapping.TableName)) _ExcludedTables.Remove(tableApiMapping.TableName);
		_IncludedTables.Add(tableApiMapping);

		return this;

	}

	/// <summary>
	/// Exclude individual tables from the API generation.  Exclusion takes priority over inclusion
	/// </summary>
	/// <param name="entitySelector">Select the entity to exclude from generation</param>
	/// <returns>Configuration builder with this configuraiton applied</returns>
	public InstantAPIsConfigBuilder<D> ExcludeTable<T>(Func<D, DbSet<T>> entitySelector) where T : class
	{

		var theSetType = entitySelector(_TheContext).GetType().BaseType;
		var property = _ContextType.GetProperties().First(p => p.PropertyType == theSetType);

		if (_IncludedTables.Select(t => t.TableName).Contains(property.Name)) _IncludedTables.Remove(_IncludedTables.First(t => t.TableName == property.Name));
		_ExcludedTables.Add(property.Name);

		return this;

	}

	private void BuildTables()
	{

		var tables = WebApplicationExtensions.GetDbTablesForContext<D>().ToArray();

		if (!_IncludedTables.Any() && !_ExcludedTables.Any())
		{
			_Config.Tables.UnionWith(tables.Select(t => new WebApplicationExtensions.TypeTable
			{
				Name = t.Name,
				InstanceType = t.InstanceType,
				ApiMethodsToGenerate = ApiMethodsToGenerate.All
			}));
			return;
		}

		// Add the Included tables
		var outTables = tables.Where(t => _IncludedTables.Any(i => i.TableName.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)))
			.Select(t => new WebApplicationExtensions.TypeTable
			{
				Name = t.Name,
				InstanceType = t.InstanceType,
				ApiMethodsToGenerate = _IncludedTables.First(i => i.TableName.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)).MethodsToGenerate
			}).ToArray();

		// If no tables were added, added them all
		if (outTables.Length == 0)
		{
			outTables = tables.Select(t => new WebApplicationExtensions.TypeTable
			{
				Name = t.Name,
				InstanceType = t.InstanceType
			}).ToArray();
		}

		// Remove the Excluded tables
		outTables = outTables.Where(t => !_ExcludedTables.Any(e => t.Name.Equals(e, StringComparison.InvariantCultureIgnoreCase))).ToArray();

		_Config.Tables.UnionWith(outTables);

	}

#endregion

	internal InstantAPIsConfig Build()
	{

		BuildTables();

		return _Config;
	}

}