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

	public InstantAPIsConfigBuilder<D> IncludeTable<T>(DbSet<T> table, ApiMethodsToGenerate methodsToGenerate = ApiMethodsToGenerate.All) where T : class
	{

		var property = _ContextType.GetProperties().Where(p => p.PropertyType == typeof(DbSet<T>)).FirstOrDefault();
		if (property == null) throw new ArgumentNullException(nameof(table));
		_Config.IncludeTable(new TableApiMapping(property.Name, methodsToGenerate));
		return this;
	}

	internal InstantAPIsConfig Build()
	{
		return _Config;
	}

}