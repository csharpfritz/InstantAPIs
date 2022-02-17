namespace Microsoft.AspNetCore.Builder;

public class InstantAPIsConfig
{

	public static readonly string[] DefaultTables = new[] { "all" };

	public string[] Tables { get; set; } = DefaultTables;

}