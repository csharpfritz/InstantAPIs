using System.Linq.Expressions;

namespace InstantAPIs.Repositories;

public interface IContextHelper<TContext>
{
	IEnumerable<InstantAPIsOptions.ITable> DiscoverFromContext(Uri baseUrl);
	string NameTable<TSet>(Expression<Func<TContext, TSet>> setSelector);
}

public interface IContextHelper
{
	bool IsValidFor(Type contextType);
	IEnumerable<InstantAPIsOptions.ITable> DiscoverFromContext<TContext>(Uri baseUrl);
	string NameTable<TContext, TSet>(Expression<Func<TContext, TSet>> setSelector);
}