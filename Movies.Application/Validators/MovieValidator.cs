using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class MovieValidator : AbstractValidator<Movie>
{
    public MovieValidator()
    {
        RuleFor(movie => movie.Id).NotEmpty();//knows the types automatically does it
        RuleFor(movie => movie.Genres).NotEmpty();//know its a count
        RuleFor(movie => movie.Title).NotEmpty();//know its a count
        
        
        
    }
}