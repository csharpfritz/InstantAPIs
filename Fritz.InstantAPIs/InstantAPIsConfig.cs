namespace Microsoft.AspNetCore.Builder;

internal class InstantAPIsConfig
{

	public static readonly string[] DefaultTables = new[] { "all" };

	public HashSet<string> IncludedTables { get; } = DefaultTables.ToHashSet();

	public HashSet<string> ExcludedTables { get; } = new();

	public void IncludeTable(string name)
	{
		if (IncludedTables.Contains("all")) IncludedTables.Clear();
		if (ExcludedTables.Contains(name)) ExcludedTables.Remove(name);
		IncludedTables.Add(name);
	}

	public void ExcludeTable(string name)
	{
		if (IncludedTables.Contains(name)) IncludedTables.Remove(name);
		if (!IncludedTables.Any()) IncludedTables.Add(DefaultTables.First());
		ExcludedTables.Add(name);
	}

}


public class InstantAPIsConfigBuilder<D> where D : DbContext
{

	private InstantAPIsConfig _Config = new();

	public InstantAPIsConfigBuilder<D> IncludeTable<T>(DbSet<T> tbl) where T : class
	{
		_Config.IncludeTable("");
		return this;
	}

	internal InstantAPIsConfig Build()
	{
		return _Config;
	}

}