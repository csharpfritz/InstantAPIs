namespace Fritz.InstantAPIs.Generators.Helpers
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class InstantAPIsForDbContextAttribute
		: Attribute
	{
		public InstantAPIsForDbContextAttribute(Type dbContextType) =>
			DbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));

		public Type DbContextType { get; }
	}
}