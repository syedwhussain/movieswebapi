﻿using Movies.Application.Models;

namespace Movies.Application;

public interface IMovieRepository
{
    Task<bool> CreateAsync(Movie movie, Guid? userId = default,CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default,CancellationToken cancellationToken = default);//Movie object or null
    Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default,CancellationToken cancellationToken = default);//Movie object or null

    Task<IEnumerable<Movie>> GetAllAsynch(Guid? userId = default,CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Movie movie,CancellationToken cancellationToken = default);

    Task<bool> DeleteByIdAsync(Guid id, Guid? userId = default,CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);

}