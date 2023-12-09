using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Models;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieRespository _movieRespository;

    public MoviesController(IMovieRespository movieRespository)
    {
        
        _movieRespository = movieRespository;
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        //hmm extension method approach rather than using
        //AutoMapperD
        var movie = request.MapToMovie();

        var createResult = await _movieRespository.CreateAsync(movie);

        //return Ok(movie);
        //return    Created($"{ApiEndpoints.Movies.Create}/{movie.Id}", movie);
        return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movie);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> GetMovieById([FromRoute] Guid id)
    {
        var movie = await _movieRespository.GetByIdAsync(id);

        if (movie == null)
        {
            return NotFound();
        }

        return Ok(movie.MapToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAllMovie([FromRoute] Guid id)
    {
        var movies = await _movieRespository.GetAllAsynch();

        if (movies is null)
        {
            return NotFound();
        }

        return Ok(movies.MapToResponse());
    }
    
    
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
    {
        //first get the movie
        var movie = await _movieRespository.GetByIdAsync(id);

        if (movie is null)
        {
            return NotFound();
        }

        var movieUpdated = request.MapToMovie(id);

        var updated = await _movieRespository.UpdateAsync(movieUpdated);

        if (!updated)
        {
            return NotFound();
        }

        return Ok(movieUpdated);
    }
}