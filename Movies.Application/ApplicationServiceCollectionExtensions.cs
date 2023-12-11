 
    
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Movies.Application.Services;

namespace Movies.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //service.Buuild ...no no . cant do this. only abstractin DI decelaration ar ebroudh it.
        services.AddSingleton<IMovieRepository, MovieRepositoryPostgres>();
        services.AddSingleton<IMovieService, MovieService>();
        return services;
    }

    //add extension method to IServiceCollection for adding the db connection factory
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(connectionString));
        //add dbinitializer as singleton
        services.AddSingleton<DbInitializer>(); 
        return services;
    }
    
}