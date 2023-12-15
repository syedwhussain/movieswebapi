using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IValidator<Movie> _movieValidator;

    public MovieService(IMovieRepository movieRepository,IValidator<Movie> movieValidator)
    {
        _movieRepository = movieRepository;
        this._movieValidator = movieValidator;
    }

    public async Task<bool> CreateAsync(Movie movie,Guid? userId = default,CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie,cancellationToken);
        
        return await _movieRepository.CreateAsync(movie,userId,cancellationToken);
    }

    public Task<Movie?> GetByIdAsync(Guid id,Guid? userId = default,CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetByIdAsync(id,userId,cancellationToken);

    }

    public Task<Movie?> GetBySlugAsync(string slug,Guid? userId = default,CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetBySlugAsync(slug,userId,cancellationToken);

    }

    public Task<IEnumerable<Movie>> GetAllAsynch(Guid? userId = default,CancellationToken cancellationToken = default)
    {
        return _movieRepository.GetAllAsynch(userId,cancellationToken);

    }

    public Task<Movie> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Movie?> UpdateAsync(Movie movie,CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie);//thwo error
        var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id,cancellationToken);

        if (!movieExists)
        {
            return null;
        }

        await _movieRepository.UpdateAsync(movie,cancellationToken);
        return movie;
    }

    public Task<bool> DeleteByIdAsync(Guid id,Guid? userId = default,CancellationToken cancellationToken = default)
    {
        return _movieRepository.DeleteByIdAsync(id,userId,cancellationToken);
    }
}