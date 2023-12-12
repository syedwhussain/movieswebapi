using Movies.Application.Models;
using Movies.Application.Validators;

namespace Movies.Tests;

public class ValidatorTests
{
    [Fact]
    public async Task show_run_validation_all_ok()
    {
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

        var movieValidator = new MovieValidator();

        // Act
        var validationResult = await movieValidator.ValidateAsync(movie);

        // Assert
        Assert.True(validationResult.IsValid);


    }
    
    [Fact]
    public async Task should_fail_with_year_too_high()
    {
        // Arrange
        var movie = new Movie
        {
            // Initialize your movie object here
            // For example:
            Id = Guid.NewGuid(),
            Title = "Test Title",
            YearOfRelease = 2015,
            Genres = new List<string> { "Test Genre" }
        };

        var movieValidator = new MovieValidator();

        // Act
        var validationResult = await movieValidator.ValidateAsync(movie);

        // Assert
        Assert.False(validationResult.IsValid,validationResult.Errors.ToString());


    }
}