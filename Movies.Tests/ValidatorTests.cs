using Movies.Application;
using Movies.Application.Models;
using Movies.Application.Validators;

namespace Movies.Tests;

public class ValidatorTests
{
    
    [Fact]
    public async Task should_pass_validator()
    {
        string _connectionString = "Server=localhost;Database=movies;User ID=course;Password=changeme;";
       
        IDbConnectionFactory dbConnectionFactory = new NpgsqlConnectionFactory(_connectionString);
        
        var repo = new MovieRepositoryPostgres(dbConnectionFactory);
        
        // Arrange
        var movie = new Movie
        {
            // Initialize your movie object here
            // For example:
            Id = Guid.NewGuid(),
            Title = "Test Title",
            YearOfRelease = 2022,
            Genres = new List<string> { "Test Genre" }
        };

        var movieValidator = new MovieValidator(repo);

        // Act
        var validationResult = await movieValidator.ValidateAsync(movie);

        // Assert
        Assert.True(validationResult.IsValid);
    }
    
    
    
    
   
}