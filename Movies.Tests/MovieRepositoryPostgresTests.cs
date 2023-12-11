using Movies.Application;
using Movies.Application.Models;

namespace Movies.Tests;

public class MovieRepositoryPostgresTests
{
    private readonly string _connectionString = "Server=localhost;Database=movies;User ID=course;Password=changeme;";



    [Fact]
    public async Task UpdateAsync_UpdatesMovieRecord_WhenMovieExists()
    {
        IDbConnectionFactory dbConnectionFactory = new NpgsqlConnectionFactory(_connectionString);
        // Arrange
        var movieId = new Guid("71dc6c59-9cbe-4a11-aea6-bc67b843f50f"); // replace with an ID that exists in your database
        var repo = new MovieRepositoryPostgres(dbConnectionFactory);

        var movieToUpdate = new Movie
        {
            Id = movieId,
            Title = "Updated2 Title2",
            YearOfRelease = 2022,
            Genres = new List<string> { "Horror, Sci-fi" }
        };

        // Act
        var result = await repo.UpdateAsync(movieToUpdate);

        // Assert
        Assert.True(result);

        // Verify the update
        var updatedMovie = await repo.GetByIdAsync(movieId);
        Assert.Equal("Updated2 Title2", updatedMovie.Title);
        Assert.Equal("updated2-title2-2022", updatedMovie.Slug);
        Assert.Equal(2022, updatedMovie.YearOfRelease);

    }


}