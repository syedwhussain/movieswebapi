using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Services;

namespace Movies.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //service.Buuild ...no no . cant do this. only abstractin DI decelaration ar ebroudh it.
        services.AddSingleton<IMovieRepository, MovieRepositoryPostgres>();
        services.AddSingleton<IMovieService, MovieService>();
        
        //this is the validator that will be loaded in into the system and automatically run. requires
        services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton);
        
        //add the injected validator from the entire assembly.
        
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