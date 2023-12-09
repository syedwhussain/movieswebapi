 
    
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Movies.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //service.Buuild ...no no . cant do this. only abstractin DI decelaration ar ebroudh it.
        services.AddSingleton<IMovieRespository, MovieRepository>();
        return services;
    }
    
}