using System.Linq.Expressions;
using System.Reflection;

namespace InstantAPIs.Repositories.EntityFrameworkCore;

public class ContextHelper :
	IContextHelper
{

	public bool IsValidFor(Type contextType) =>
		contextType.IsAssignableTo(typeof(DbContext));

	public IEnumerable<InstantAPIsOptions.ITable> DiscoverFromContext<TContext>(Uri baseUrl)
	{
		var dbSet = typeof(DbSet<>);
		return typeof(TContext)
			.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(x => (x.PropertyType.FullName?.StartsWith("Microsoft.EntityFrameworkCore.DbSet") ?? false)
						&& x.PropertyType.GenericTypeArguments.First().GetCustomAttributes(typeof(KeylessAttribute), true).Length <= 0)
			.Select(x => CreateTable(x.Name, new Uri($"{baseUrl.OriginalString}/{x.Name}", UriKind.Relative), typeof(TContext), x.PropertyType, x.PropertyType.GenericTypeArguments.First()))
			.Where(x => x != null).OfType<InstantAPIsOptions.ITable>();
	}

	private static InstantAPIsOptions.ITable? CreateTable(string name, Uri baseUrl, Type contextType, Type setType, Type entityType)
	{
		var keyProperty = entityType.GetProperties().Where(x => "id".Equals(x.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
		if (keyProperty == null) return null;

		var genericMethod = typeof(ContextHelper).GetMethod(nameof(CreateTableGeneric), BindingFlags.NonPublic | BindingFlags.Static)
			?? throw new Exception("Missing method");
		var concreteMethod = genericMethod.MakeGenericMethod(contextType, setType, entityType, keyProperty.PropertyType);

		var entitySelector = CreateExpression(contextType, name, setType);
		var keySelector = CreateExpression(entityType, keyProperty.Name, keyProperty.PropertyType);
		return concreteMethod.Invoke(null, new object?[] { name, baseUrl, entitySelector, keySelector, null }) as InstantAPIsOptions.ITable;
	}

	private static object CreateExpression(Type memberOwnerType, string property, Type returnType)
	{
		var parameterExpression = Expression.Parameter(memberOwnerType, "x");
		var propertyExpression = Expression.Property(parameterExpression, property);
		//var block = Expression.Block(propertyExpression, returnExpression);
		return Expression.Lambda(typeof(Func<,>).MakeGenericType(memberOwnerType, returnType), propertyExpression, parameterExpression);
	}

	private static InstantAPIsOptions.ITable CreateTableGeneric<TContext, TSet, TEntity, TKey>(string name, Uri baseUrl,
		Expression<Func<TContext, TSet>> entitySelector, Expression<Func<TEntity, TKey>>? keySelector, Expression<Func<TEntity, TKey>>? orderBy)
		where TContext : class
		where TSet : class
		where TEntity : class
	{
		return new InstantAPIsOptions.Table<TContext, TSet, TEntity, TKey>(name, baseUrl, entitySelector,
			new InstantAPIsOptions.TableOptions<TEntity, TKey>()
			{
				KeySelector = keySelector,
				OrderBy = orderBy
			});
	}

	public string NameTable<TContext, TSet>(Expression<Func<TContext, TSet>> setSelector)
	{
		return setSelector.Body.NodeType == ExpressionType.MemberAccess
				&& setSelector.Body is MemberExpression memberExpression
			? memberExpression.Member.Name
			: throw new ArgumentException(nameof(setSelector.Body.DebugInfo), "Not a valid expression");
	}
}
