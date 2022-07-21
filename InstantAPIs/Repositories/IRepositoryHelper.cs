using Microsoft.AspNetCore.Http;

namespace InstantAPIs.Repositories;

public interface IRepositoryHelper<TContext, TSet, TEntity, TKey>
{
	Task<IEnumerable<TEntity>> Get(HttpRequest request, TContext context, string name, CancellationToken cancellationToken);
	Task<TEntity?> GetById(HttpRequest request, TContext context, string name, TKey id, CancellationToken cancellationToken);
	Task<TKey> Insert(HttpRequest request, TContext context, string name, TEntity newObj, CancellationToken cancellationToken);
	Task Update(HttpRequest request, TContext context, string name, TKey id, TEntity newObj, CancellationToken cancellationToken);
	Task<bool> Delete(HttpRequest request, TContext context, string name, TKey id, CancellationToken cancellationToken);
}
