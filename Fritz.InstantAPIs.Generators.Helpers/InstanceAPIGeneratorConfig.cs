namespace Fritz.InstantAPIs.Generators.Helpers
{
	public class InstanceAPIGeneratorConfig<T>
		where T : Enum
	{
		public InstanceAPIGeneratorConfig() { }

		public virtual TableConfig<T> this[T key] =>
			new(key)
			{
				Name = Enum.GetName(typeof(T), key)!,
				Included = Included.Yes,
				APIs = ApisToGenerate.All,
			};
	}
}