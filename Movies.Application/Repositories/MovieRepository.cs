﻿using Movies.Application.Models;

namespace Movies.Application;

public class MovieRepository : IMovieRepository
{
    private readonly List<Movie> _movies = new();
    public Task<bool> CreateAsync(Movie movie)
    {
        _movies.Add(movie);
        return Task.FromResult(true);
    }

    public Task<Movie?> GetByIdAsync(Guid id)
    {
        var movie = _movies.SingleOrDefault(m => m.Id == id);
        return Task.FromResult(movie);
    }

    public Task<Movie?> GetBySlugAsync(string slug)
    {
        var movie = _movies.SingleOrDefault(m => m.Slug == slug);
        return Task.FromResult(movie);
    }


    public Task<IEnumerable<Movie>> GetAllAsynch()
    {
         return Task.FromResult(_movies.AsEnumerable());
    }

    public Task<bool> UpdateAsync(Movie movie)
    {
        var movieIndex = _movies.FindIndex(m => m.Id == movie.Id);  //this is for list only

        if(movieIndex == -1)
        {
            return Task.FromResult(false);
        }   

        _movies[movieIndex] = movie;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteByIdAsync(Guid id)
    {
         var removeCount = _movies.RemoveAll(m => m.Id == id);
            return Task.FromResult(removeCount > 0);

    }

    public Task<bool> ExistsByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}