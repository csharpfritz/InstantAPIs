namespace Fritz.InstantAPIs.Generators.Helpers
{
	[Flags]
	public enum ApisToGenerate
	{
		Get = 1,
		GetById = 2,
		Insert = 4,
		Update = 8,
		Delete = 16,
		All = 31
	}
}