namespace InstantAPIs.Repositories.EntityFrameworkCore;

public class RepositoryHelperFactory :
	IRepositoryHelperFactory
{
	public bool IsValidFor(Type contextType, Type setType) =>
		contextType.IsAssignableTo(typeof(DbContext))
		&& setType.IsGenericType && setType.GetGenericTypeDefinition().Equals(typeof(DbSet<>));

	public IRepositoryHelper<TContext, TSet, TEntity, TKey> Create<TContext, TSet, TEntity, TKey>(
		Func<TContext, TSet> setSelector, InstantAPIsOptions.TableOptions<TEntity, TKey> config)
	{
		if (!typeof(TContext).IsAssignableTo(typeof(DbContext))) throw new ArgumentException("Context needs to derive from DbContext");

		var newRepositoryType = typeof(RepositoryHelper<,,,>).MakeGenericType(typeof(TContext), typeof(TSet), typeof(TEntity), typeof(TKey));
		var returnValue = Activator.CreateInstance(newRepositoryType, setSelector, config)
			?? throw new Exception("Could not create an instance of the EFCoreRepository implementation");

		return (IRepositoryHelper<TContext, TSet, TEntity, TKey>)returnValue;
	}
}
