using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Services;

namespace Movies.Application.Validators;

public class MovieValidator : AbstractValidator<Movie>
{
    private readonly IMovieService _movieService;
    public MovieValidator()
    {
        RuleFor(movie => movie.Id).NotEmpty();//knows the types automatically does it
        RuleFor(movie => movie.Genres).NotEmpty();//know its a count
        RuleFor(movie => movie.Title).NotEmpty();//know its a count
        //custom validation for YearOfRelease
        RuleFor(m => m.YearOfRelease).GreaterThan(1940).LessThanOrEqualTo(DateTime.UtcNow.Year);
        
        //add custom validation rule
        
        //on create it doesnt exist, on update it does....so if its update, it must be there.
        RuleFor(m => m.Slug)
            .MustAsync(ValidateSlug)
            .WithMessage("This item already exists");

    }

    public async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken cancellationToken)
    {
        //get the movie based on the slug value first. this will coe from the presentation layer
        var existingDbMovie = await _movieService.GetBySlugAsync(slug);

        bool isValid = false;

        //an existing item has this already, so we need to check if its the same item
        if (existingDbMovie is not null && 
                    existingDbMovie.Id == movie.Id)       {
            isValid = true;
        }

        if(existingDbMovie is null)//in this casser its unique no other movie has this slug
        {
            isValid = true; //this is a create since we dont have it in the db. All good.
        }

        return isValid;
    }
}