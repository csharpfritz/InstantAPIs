using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq.Expressions;

namespace InstantAPIs;

public enum EnableSwagger
{
    None,
    DevelopmentOnly,
    Always
}

public class InstantAPIsOptions
{

    public EnableSwagger? EnableSwagger { get; set; }
    public Action<SwaggerGenOptions>? Swagger { get; set; }

	internal class Table<TContext, TSet, TEntity, TKey>
		: ITable
	{
		public Table(string name, Uri baseUrl, Expression<Func<TContext, TSet>> entitySelector, TableOptions<TEntity, TKey> config)
		{
			Name = name;
			BaseUrl = baseUrl;
			EntitySelector = entitySelector;
			Config = config;

			RepoType = typeof(TContext);
			InstanceType = typeof(TEntity);
		}

		public string Name { get; }
		public Type RepoType { get; }
		public Type InstanceType { get; }
		public Uri BaseUrl { get; set; }

		public Expression<Func<TContext, TSet>> EntitySelector { get; }
		public TableOptions<TEntity, TKey> Config { get; }

		public ApiMethodsToGenerate ApiMethodsToGenerate { get; set; } = ApiMethodsToGenerate.All;

		public object EntitySelectorObject => EntitySelector;
		public object ConfigObject => Config;
	}

	public interface ITable
	{
		public string Name { get; }
		public Type RepoType { get; }
		public Type InstanceType { get; }
		public Uri BaseUrl { get; set; }
		public ApiMethodsToGenerate ApiMethodsToGenerate { get; set; }

		public object EntitySelectorObject { get; }
		public object ConfigObject { get; }

	}

	public record TableOptions<TEntity, TKey>()
	{
		public Expression<Func<TEntity, TKey>>? KeySelector { get; set; }

		public Expression<Func<TEntity, TKey>>? OrderBy { get; set; }
	}
}