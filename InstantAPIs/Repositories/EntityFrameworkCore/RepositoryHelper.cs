using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace InstantAPIs.Repositories.EntityFrameworkCore;

public class RepositoryHelper<TContext, TSet, TEntity, TKey> :
	IRepositoryHelper<TContext, TSet, TEntity, TKey>
	where TContext : DbContext
	where TSet : DbSet<TEntity>
	where TEntity : class
{
	private readonly Func<TContext, TSet> _setSelector;
	private readonly InstantAPIsOptions.TableOptions<TEntity, TKey> _config;
	private readonly Func<TEntity, TKey> _keySelector;
	private readonly string _keyName;

	/// <summary>
	/// This constructor is called using reflection in order to have meaningfull context and set generic types
	/// </summary>
	/// <param name="config"></param>
	public RepositoryHelper(Func<TContext, TSet> setSelector, InstantAPIsOptions.TableOptions<TEntity, TKey> config)
	{
		_setSelector = setSelector;
		_config = config;

		// create predicate based on the key selector?
		_keySelector = config.KeySelector?.Compile() ?? throw new Exception("Key selector required");
		// if no keyselector is found we need to find it? Or do we fall back to "id"?
		_keyName = config.KeySelector.Body.NodeType == ExpressionType.MemberAccess
				&& config.KeySelector.Body is MemberExpression memberExpression
			? memberExpression.Member.Name
			: throw new ArgumentException(nameof(config.KeySelector.Body.DebugInfo), "Not a valid expression");
	}

	private Expression<Func<TEntity, bool>> CreatePredicate(TKey key)
	{
		var parameterExpression = Expression.Parameter(typeof(TEntity), "x");
		var propertyExpression = Expression.Property(parameterExpression, _keyName);
		var keyValueExpression = Expression.Constant(key);
		return Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(propertyExpression, keyValueExpression), parameterExpression);
	}

	private TSet SelectSet(TContext context)
		=> _setSelector(context) ?? throw new ArgumentNullException("Empty set");

	public bool IsValidFor(Type type) => type.IsAssignableFrom(typeof(DbSet<>));

	public async Task<IEnumerable<TEntity>> Get(HttpRequest request, TContext context, string name, CancellationToken cancellationToken)
	{
		var set = SelectSet(context);
		return await set.ToListAsync(cancellationToken);
	}

	public async Task<TEntity?> GetById(HttpRequest request, TContext context, string name, TKey id, CancellationToken cancellationToken)
	{
		var set = SelectSet(context);
		return await set.FirstOrDefaultAsync(CreatePredicate(id), cancellationToken);
	}

	public async Task<TKey> Insert(HttpRequest request, TContext context, string name, TEntity newObj, CancellationToken cancellationToken)
	{
		var set = SelectSet(context);
		await set.AddAsync(newObj, cancellationToken);
		await context.SaveChangesAsync(cancellationToken);
		return _keySelector(newObj);
	}

	public async Task Update(HttpRequest request, TContext context, string name, TKey id, TEntity newObj, CancellationToken cancellationToken)
	{
		var set = SelectSet(context);
		var entity = set.Attach(newObj);
		entity.State = EntityState.Modified;
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> Delete(HttpRequest request, TContext context, string name, TKey id, CancellationToken cancellationToken)
	{
		var set = SelectSet(context);
		var entity = await set.FirstOrDefaultAsync(CreatePredicate(id), cancellationToken);

		if (entity == null) return false;

		set.Remove(entity);
		await context.SaveChangesAsync(cancellationToken);
		return true;
	}
}
