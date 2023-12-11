using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie);
    Task<Movie?> GetByIdAsync(Guid id);//Movie object or null
    Task<Movie?> GetBySlugAsync(string slug);//Movie object or null

    Task<IEnumerable<Movie>> GetAllAsynch();

    Task<Movie> UpdateAsync(Movie movie);

    Task<bool> DeleteByIdAsync(Guid id);
}