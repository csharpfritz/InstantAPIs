using Microsoft.AspNetCore.Http;

namespace InstantAPIs.Repositories;

public interface IRepositoryHelperFactory<TContext, TSet, TEntity, TKey>
	where TContext : class
	where TSet : class
	where TEntity : class
{
	public Task<IEnumerable<TEntity>> Get(HttpRequest request, TContext context, string name, CancellationToken cancellationToken);
	public Task<TEntity?> GetById(HttpRequest request, TContext context, string name, TKey id, CancellationToken cancellationToken);
	Task<TKey> Insert(HttpRequest request, TContext context, string name, TEntity newObj, CancellationToken cancellationToken);
	Task Update(HttpRequest request, TContext context, string name, TKey id, TEntity newObj, CancellationToken cancellationToken);
	Task<bool> Delete(HttpRequest request, TContext context, string name, TKey id, CancellationToken cancellationToken);
}

public interface IRepositoryHelperFactory
{
	bool IsValidFor(Type contextType, Type setType);
	IRepositoryHelper<TContext, TSet, TEntity, TKey> Create<TContext, TSet, TEntity, TKey>(Func<TContext, TSet> setSelector, InstantAPIsOptions.TableOptions<TEntity, TKey> config);
}
