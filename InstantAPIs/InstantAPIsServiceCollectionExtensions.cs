using InstantAPIs.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class InstantAPIsServiceCollectionExtensions
{
	public static IServiceCollection AddInstantAPIs(this IServiceCollection services, Action<InstantAPIsOptions>? setupAction = null)
	{
		var options = new InstantAPIsOptions();

		// Get the service options
		setupAction?.Invoke(options);

		if (options.EnableSwagger == null)
		{
			options.EnableSwagger = EnableSwagger.DevelopmentOnly;
		}

		// Add and configure Swagger services if it is enabled
		if (options.EnableSwagger != EnableSwagger.None)
		{
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen(options.Swagger);
		}

		// Register the required options so that it can be accessed by InstantAPIs middleware
		services.Configure<InstantAPIsOptions>(config =>
		{
			config.EnableSwagger = options.EnableSwagger;
		});

		services.AddSingleton(typeof(IRepositoryHelperFactory<,,,>), typeof(RepositoryHelperFactory<,,,>));
		services.AddSingleton(typeof(IContextHelper<>), typeof(ContextHelper<>));

		// ef core specific
		services.AddSingleton<IRepositoryHelperFactory, InstantAPIs.Repositories.EntityFrameworkCore.RepositoryHelperFactory>();
		services.AddSingleton<IContextHelper, InstantAPIs.Repositories.EntityFrameworkCore.ContextHelper>();

		// json specific
		services.AddSingleton<IRepositoryHelperFactory, InstantAPIs.Repositories.Json.RepositoryHelperFactory>();
		services.AddSingleton<IContextHelper, InstantAPIs.Repositories.Json.ContextHelper>();

		return services;
	}
}