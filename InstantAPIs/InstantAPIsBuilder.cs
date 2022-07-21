using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder;

public class InstantAPIsBuilder<TContext> 
	where TContext : DbContext
{

	private HashSet<InstantAPIsOptions.ITable> _Config = new();
	private Type _ContextType = typeof(TContext);
	private TContext _TheContext;
	private readonly HashSet<InstantAPIsOptions.ITable> _IncludedTables = new();
	private readonly List<string> _ExcludedTables = new();
	private const string DEFAULT_URI = "/api/";

	public InstantAPIsBuilder(TContext theContext)
	{
		this._TheContext = theContext;
	}

	#region Table Inclusion/Exclusion

	/// <summary>
	/// Specify individual tables to include in the API generation with the methods requested
	/// </summary>
	/// <param name="setSelector">Select the EntityFramework DbSet to include - Required</param>
	/// <param name="methodsToGenerate">A flags enumerable indicating the methods to generate.  By default ALL are generated</param>
	/// <returns>Configuration builder with this configuration applied</returns>
	public InstantAPIsBuilder<TContext> IncludeTable<TSet, TEntity, TKey>(Expression<Func<TContext, TSet>> setSelector, 
			InstantAPIsOptions.TableOptions<TEntity, TKey> config, ApiMethodsToGenerate methodsToGenerate = ApiMethodsToGenerate.All, string baseUrl = "")
		where TSet : DbSet<TEntity>
		where TEntity : class
	{

		var theSetType = setSelector.Compile()(_TheContext).GetType().BaseType;
		var property = _ContextType.GetProperties().First(p => p.PropertyType == theSetType);

		if (!string.IsNullOrEmpty(baseUrl))
		{
			try
			{
				var testUri = new Uri(baseUrl, UriKind.RelativeOrAbsolute);
				baseUrl = testUri.IsAbsoluteUri ? testUri.LocalPath : baseUrl;
			}
			catch
			{
				throw new ArgumentException(nameof(baseUrl), "Not a valid Uri");
			}
		}
		else
		{
			baseUrl = string.Concat(DEFAULT_URI, property.Name);
		}

		var tableApiMapping = new InstantAPIsOptions.Table<TContext, TSet, TEntity, TKey>(property.Name, new Uri(baseUrl, UriKind.Relative), setSelector, config) 
		{ 
			ApiMethodsToGenerate = methodsToGenerate 
		};
		_IncludedTables.Add(tableApiMapping);

		if (_ExcludedTables.Contains(tableApiMapping.Name)) _ExcludedTables.Remove(tableApiMapping.Name);
		_IncludedTables.Add(tableApiMapping);

		return this;

	}

	/// <summary>
	/// Exclude individual tables from the API generation.  Exclusion takes priority over inclusion
	/// </summary>
	/// <param name="entitySelector">Select the entity to exclude from generation</param>
	/// <returns>Configuration builder with this configuraiton applied</returns>
	public InstantAPIsBuilder<TContext> ExcludeTable<T>(Func<TContext, DbSet<T>> entitySelector) where T : class
	{

		var theSetType = entitySelector(_TheContext).GetType().BaseType;
		var property = _ContextType.GetProperties().First(p => p.PropertyType == theSetType);

		if (_IncludedTables.Select(t => t.Name).Contains(property.Name)) _IncludedTables.Remove(_IncludedTables.First(t => t.Name == property.Name));
		_ExcludedTables.Add(property.Name);

		return this;

	}

	private void BuildTables()
	{
		var tables = WebApplicationExtensions.GetDbTablesForContext<TContext>().ToArray();
		InstantAPIsOptions.ITable[]? outTables;

		// Add the Included tables
		if (_IncludedTables.Any())
		{
			outTables = tables.Where(t => _IncludedTables.Any(i => i.Name.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)))
				.Select(t => {
					var table = CreateTable(t.Name, new Uri(_IncludedTables.First(i => i.Name.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)).BaseUrl.ToString(), UriKind.Relative), typeof(TContext), typeof(DbSet<>).MakeGenericType(t.InstanceType), t.InstanceType);
					if (table != null)
					{
						table.ApiMethodsToGenerate = _IncludedTables.First(i => i.Name.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)).ApiMethodsToGenerate;
					}
					return table;
				})
				.Where(x => x != null).OfType<InstantAPIsOptions.ITable>()
				.ToArray();
		} else { 
			outTables = tables
					.Select(t => CreateTable(t.Name, new Uri(DEFAULT_URI + t.Name, uriKind: UriKind.Relative), typeof(TContext), typeof(DbSet<>).MakeGenericType(t.InstanceType), t.InstanceType))
					.Where(x => x != null).OfType<InstantAPIsOptions.ITable>()
					.ToArray();
		}

		// Exit now if no tables were excluded
		if (!_ExcludedTables.Any())
		{
			_Config.UnionWith(outTables);
			return;
		}

		// Remove the Excluded tables
		outTables = outTables.Where(t => !_ExcludedTables.Any(e => e.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase))).ToArray();

		if (outTables == null || !outTables.Any()) throw new ArgumentException("All tables were excluded from this configuration");

		_Config.UnionWith(outTables);

	}

	public static InstantAPIsOptions.ITable? CreateTable(string name, Uri baseUrl, Type contextType, Type setType, Type entityType)
	{
		var keyProperty = entityType.GetProperties().Where(x => "id".Equals(x.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
		if (keyProperty == null) return null;

		var genericMethod = typeof(InstantAPIsBuilder<>).MakeGenericType(contextType).GetMethod(nameof(CreateTableGeneric), BindingFlags.NonPublic | BindingFlags.Static)
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

	private static InstantAPIsOptions.ITable CreateTableGeneric<TContextStatic, TSet, TEntity, TKey>(string name, Uri baseUrl,
		Expression<Func<TContextStatic, TSet>> entitySelector, Expression<Func<TEntity, TKey>>? keySelector, Expression<Func<TEntity, TKey>>? orderBy)
		where TContextStatic : class
		where TSet : class
		where TEntity : class
	{
		return new InstantAPIsOptions.Table<TContextStatic, TSet, TEntity, TKey>(name, baseUrl, entitySelector,
			new InstantAPIsOptions.TableOptions<TEntity, TKey>()
			{
				KeySelector = keySelector,
				OrderBy = orderBy
			});
	}
	#endregion

	internal HashSet<InstantAPIsOptions.ITable> Build()
	{

		BuildTables();

		return _Config;
	}

}