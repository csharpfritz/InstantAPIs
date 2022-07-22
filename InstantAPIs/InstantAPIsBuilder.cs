using InstantAPIs.Repositories;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Builder;

public class InstantAPIsBuilder<TContext>
	where TContext : class
{
	private readonly InstantAPIsOptions _instantApiOptions;
	private readonly IContextHelper<TContext> _contextFactory;
	private readonly HashSet<InstantAPIsOptions.ITable> _tables = new HashSet<InstantAPIsOptions.ITable>();
	private readonly IList<string> _excludedTables = new List<string>();

	public InstantAPIsBuilder(InstantAPIsOptions instantApiOptions, IContextHelper<TContext> contextFactory)
	{
		_instantApiOptions = instantApiOptions;
		_contextFactory = contextFactory;
	}

	private IEnumerable<InstantAPIsOptions.ITable> DiscoverTables()
	{
		return _contextFactory != null
			? _contextFactory.DiscoverFromContext(_instantApiOptions.DefaultUri)
			: Array.Empty<InstantAPIsOptions.ITable>();
	}

	#region Table Inclusion/Exclusion

	/// <summary>
	/// Specify individual tables to include in the API generation with the methods requested
	/// </summary>
	/// <param name="setSelector">Select the EntityFramework DbSet to include - Required</param>
	/// <param name="methodsToGenerate">A flags enumerable indicating the methods to generate.  By default ALL are generated</param>
	/// <returns>Configuration builder with this configuration applied</returns>
	public InstantAPIsBuilder<TContext> IncludeTable<TSet, TEntity, TKey>(Expression<Func<TContext, TSet>> setSelector,
		InstantAPIsOptions.TableOptions<TEntity, TKey> config, ApiMethodsToGenerate methodsToGenerate = ApiMethodsToGenerate.All,
		string baseUrl = "")
		where TSet : class
		where TEntity : class
	{
		var propertyName = _contextFactory.NameTable(setSelector);

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
			baseUrl = string.Concat(_instantApiOptions.DefaultUri.ToString(), "/", propertyName);
		}

		var tableApiMapping = new InstantAPIsOptions.Table<TContext, TSet, TEntity, TKey>(propertyName, new Uri(baseUrl, UriKind.Relative), setSelector, config)
		{
			ApiMethodsToGenerate = methodsToGenerate
		};

		_tables.RemoveWhere(x => x.Name == tableApiMapping.Name);
		_tables.Add(tableApiMapping);

		return this;

	}

	/// <summary>
	/// Exclude individual tables from the API generation.  Exclusion takes priority over inclusion
	/// </summary>
	/// <param name="setSelector">Select the entity to exclude from generation</param>
	/// <returns>Configuration builder with this configuraiton applied</returns>
	public InstantAPIsBuilder<TContext> ExcludeTable<TSet>(Expression<Func<TContext, TSet>> setSelector) where TSet : class
	{
		var propertyName = _contextFactory.NameTable(setSelector);
		_excludedTables.Add(propertyName);

		return this;
	}

	private void BuildTables()
	{
		if (!_tables.Any())
		{
			var discoveredTables = DiscoverTables();
			foreach (var discoveredTable in discoveredTables)
			{
				_tables.Add(discoveredTable);
			}
		}

		_tables.RemoveWhere(t => _excludedTables.Any(e => t.Name.Equals(e, StringComparison.InvariantCultureIgnoreCase)));

		if (!_tables.Any()) throw new ArgumentException("All tables were excluded from this configuration");
	}

	#endregion

	internal IEnumerable<InstantAPIsOptions.ITable> Build()
	{
		BuildTables();

		return _tables;
	}

}
