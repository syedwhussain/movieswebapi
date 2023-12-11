using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        
        _movieService = movieService;
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        //hmm extension method approach rather than using
        //AutoMapperD
        var movie = request.MapToMovie();

        var createResult = await _movieService.CreateAsync(movie);

        //return Ok(movie);
        //return    Created($"{ApiEndpoints.Movies.Create}/{movie.Id}", movie);
        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id.ToString() }, movie);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug)
    {
        var movie = Guid.TryParse(idOrSlug.ToString(), out var id) ? 
                    await _movieService.GetByIdAsync(id):
                    await _movieService.GetBySlugAsync(idOrSlug);

        //var movie = await _movieRepository.GetByIdAsync(idOrSlug);

        if (movie == null)
        {
            return NotFound();
        }
        return Ok(movie.MapToResponse());

    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()//[FromRoute] Guid id)
    {
        var movies = await _movieService.GetAllAsynch();

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
        var movie = await _movieService.GetByIdAsync(id);

        if (movie is null)
        {
            return NotFound();
        }

        var movieUpdated = request.MapToMovie(id);

        var movieFromUpdate = await _movieService.UpdateAsync(movieUpdated);

        if (movieFromUpdate is null)
        {
            return NotFound();
        }

        return Ok(movieUpdated);
    }

    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        //first get the movie
        var movie = await _movieService.GetByIdAsync(id);

        if (movie is null)
        {
            return NotFound();
        }

        var result = await _movieService.DeleteByIdAsync(id);

        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }

}