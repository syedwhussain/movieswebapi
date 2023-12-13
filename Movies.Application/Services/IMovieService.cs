using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie,CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(Guid id,CancellationToken cancellationToken = default);//Movie object or null
    Task<Movie?> GetBySlugAsync(string slug,CancellationToken cancellationToken = default);//Movie object or null

    Task<IEnumerable<Movie>> GetAllAsynch(CancellationToken cancellationToken = default);

    Task<Movie> UpdateAsync(Movie movie,CancellationToken cancellationToken = default);

    Task<bool> DeleteByIdAsync(Guid id,CancellationToken cancellationToken = default);
}