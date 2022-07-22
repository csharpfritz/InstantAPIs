using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace InstantAPIs.Repositories;

internal class RepositoryHelperFactory<TContext, TSet, TEntity, TKey>
	: IRepositoryHelperFactory<TContext, TSet, TEntity, TKey>
	where TContext : class
	where TSet : class
	where TEntity : class
{
	private readonly IRepositoryHelper<TContext, TSet, TEntity, TKey> _repository;

	public RepositoryHelperFactory(IOptions<InstantAPIsOptions> options, IEnumerable<IRepositoryHelperFactory> repositories)
	{
		var option = options.Value.Tables.FirstOrDefault(x => x.InstanceType == typeof(TEntity));
		if (!(option is InstantAPIsOptions.Table<TContext, TSet, TEntity, TKey> tableOptions))
			throw new Exception("Configuration mismatch");

		var contextType = typeof(TContext);
		var setType = typeof(TSet);
		_repository = repositories
			.First(x => x.IsValidFor(contextType, setType))
			.Create(tableOptions.EntitySelector.Compile(), tableOptions.Config);
	}

	public Task<IEnumerable<TEntity>> Get(HttpRequest request, TContext context, string name, CancellationToken cancellationToken)
		=> _repository.Get(request, context, name, cancellationToken);

	public Task<TEntity?> GetById(HttpRequest request, TContext context, string name, TKey id, CancellationToken cancellationToken)
		=> _repository.GetById(request, context, name, id, cancellationToken);

	public Task<TKey> Insert(HttpRequest request, TContext context, string name, TEntity newObj, CancellationToken cancellationToken)
		=> _repository.Insert(request, context, name, newObj, cancellationToken);

	public Task Update(HttpRequest request, TContext context, string name, TKey id, TEntity newObj, CancellationToken cancellationToken)
		=> _repository.Update(request, context, name, id, newObj, cancellationToken);

	public Task<bool> Delete(HttpRequest request, TContext context, string name, TKey id, CancellationToken cancellationToken)
		=> _repository.Delete(request, context, name, id, cancellationToken);

}
