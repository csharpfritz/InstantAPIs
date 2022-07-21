using System.Text.Json.Nodes;

namespace InstantAPIs.Repositories.Json;

public class RepositoryHelperFactory :
	IRepositoryHelperFactory
{
	public bool IsValidFor(Type contextType, Type setType) =>
		contextType.IsAssignableTo(typeof(Context)) && setType.Equals(typeof(JsonArray));

	public IRepositoryHelper<TContext, TSet, TEntity, TKey> Create<TContext, TSet, TEntity, TKey>(Func<TContext, TSet> setSelector, InstantAPIsOptions.TableOptions<TEntity, TKey> config)
	{
		if (!typeof(TContext).IsAssignableTo(typeof(Context))) throw new ArgumentException("Context needs to derive from JsonContext");

		var newRepositoryType = typeof(RepositoryHelper);
		var returnValue = Activator.CreateInstance(newRepositoryType, setSelector)
			?? throw new Exception("Could not create an instance of the JsonRepository implementation");

		return (IRepositoryHelper<TContext, TSet, TEntity, TKey>)returnValue;
	}
}
