using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
    public static Movie MapToMovie(this CreateMovieRequest request)
    {
        return new Movie()
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }
    
    public static MovieResponse MapToResponse(this Movie movie)
    {
        return new MovieResponse()
        {
            Id = movie.Id,
            Genres = movie.Genres,
            Title = movie.Title,
            YearOfRelease = movie.YearOfRelease
        };
    }

    //domain model to UI mode
    public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies)
    {
        return new MoviesResponse()
        {
            Items = movies.Select(x => x.MapToResponse())
        };
    }

    public static Movie MapToMovie(this UpdateMovieRequest updateRequest, Guid id)
    {
        return new Movie()
        {
            Id = id,
            Title = updateRequest.Title,
            YearOfRelease = updateRequest.YearOfRelease,
            Genres = updateRequest.Genres.ToList()
        };
    }



    
}