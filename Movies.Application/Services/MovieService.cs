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

    public async Task<bool> CreateAsync(Movie movie)
    {
        await _movieValidator.ValidateAndThrowAsync(movie);
        
        return await _movieRepository.CreateAsync(movie);
    }

    public Task<Movie?> GetByIdAsync(Guid id)
    {
        return _movieRepository.GetByIdAsync(id);

    }

    public Task<Movie?> GetBySlugAsync(string slug)
    {
        return _movieRepository.GetBySlugAsync(slug);

    }

    public Task<IEnumerable<Movie>> GetAllAsynch()
    {
        return _movieRepository.GetAllAsynch();

    }

    public async Task<Movie?> UpdateAsync(Movie movie)
    {
        await _movieValidator.ValidateAndThrowAsync(movie);//thwo error
        var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id);

        if (!movieExists)
        {
            return null;
        }

        await _movieRepository.UpdateAsync(movie);
        return movie;
    }

    public Task<bool> DeleteByIdAsync(Guid id)
    {
        return _movieRepository.DeleteByIdAsync(id);
    }
}