namespace Microsoft.AspNetCore.Builder;

internal class InstantAPIsConfig
{

	public static readonly TableApiMapping[] DefaultTables = new[] { new TableApiMapping("all") };

	public HashSet<TableApiMapping> IncludedTables { get; } = DefaultTables.ToHashSet();

	public HashSet<string> ExcludedTables { get; } = new();

	public void IncludeTable(TableApiMapping tableApiMapping)
	{
		if (IncludedTables.Select(t => t.TableName).Contains("all")) IncludedTables.Clear();
		if (ExcludedTables.Contains(tableApiMapping.TableName)) ExcludedTables.Remove(tableApiMapping.TableName);
		IncludedTables.Add(tableApiMapping);
	}

	public void ExcludeTable(string name)
	{
		if (IncludedTables.Select(t => t.TableName).Contains(name)) IncludedTables.Remove(IncludedTables.First(t => t.TableName == name));
		if (!IncludedTables.Any()) IncludedTables.Add(DefaultTables.First());
		ExcludedTables.Add(name);
	}

}


public class InstantAPIsConfigBuilder<D> where D : DbContext
{

	private InstantAPIsConfig _Config = new();
	private Type _ContextType = typeof(D);
	private D _TheContext;

	public InstantAPIsConfigBuilder(D theContext)
	{
		this._TheContext = theContext;
	}

	public InstantAPIsConfigBuilder<D> IncludeTable<T>(Func<D,DbSet<T>> entitySelector, ApiMethodsToGenerate methodsToGenerate = ApiMethodsToGenerate.All) where T : class
	{

		if (entitySelector == null) throw new ArgumentNullException(nameof(entitySelector));

		var theSetType = entitySelector(_TheContext).GetType();
		var property = _ContextType.GetProperties().First(p => p.PropertyType == theSetType);
		_Config.IncludeTable(new TableApiMapping(property.Name, methodsToGenerate));
		return this;
	}

	internal InstantAPIsConfig Build()
	{
		return _Config;
	}

}