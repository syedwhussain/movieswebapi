using Movies.Application.Models;

namespace Movies.Application;

public interface IMovieRespository
{
    Task<bool> CreateAsync(Movie movie);
    Task<Movie?> GetByIdAsync(Guid id);//Movie object or null
    Task<IEnumerable<Movie>> GetAllAsynch();

    Task<bool> UpdateAsync(Movie movie);

    Task<bool> DeleteByIdAsync(Guid id);

}