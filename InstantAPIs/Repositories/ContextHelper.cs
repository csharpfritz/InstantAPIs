using System.Linq.Expressions;

namespace InstantAPIs.Repositories;

internal class ContextHelper<TContext>
	: IContextHelper<TContext>
	where TContext : class
{
	private readonly IContextHelper _context;

	public ContextHelper(IEnumerable<IContextHelper> contexts)
	{
		// need to inject the configuration with the list of table mappings as reference to read out the 
		var contextType = typeof(TContext);
		_context = contexts
			.First(x => x.IsValidFor(contextType));
	}

	public IEnumerable<InstantAPIsOptions.ITable> DiscoverFromContext(Uri baseUrl)
		 => _context.DiscoverFromContext<TContext>(baseUrl);

	public string NameTable<TSet>(Expression<Func<TContext, TSet>> setSelector)
		 => _context.NameTable(setSelector);
}
