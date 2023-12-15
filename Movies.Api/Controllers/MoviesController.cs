using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Serilog;

namespace Movies.Api.Controllers;


[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MoviesController> _logger;
    private readonly IUserIdentityProvider _userIdentityProvider;
    

    public MoviesController(IMovieService movieService, ILogger<MoviesController> logger, IUserIdentityProvider userIdentityProvider)
    {
        
        _movieService = movieService;
        _logger = logger;
        _userIdentityProvider = userIdentityProvider;
    }

    //only admin can create
   // [Authorize("trusted_member")]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request,CancellationToken cancellationToken)
    {
        //hmm extension method approach rather than using
        //AutoMapperD
        var movie = request.MapToMovie();

        var createResult = await _movieService.CreateAsync(movie,_userIdentityProvider.GetUserId(HttpContext), cancellationToken);

        //return Ok(movie);
        //return    Created($"{ApiEndpoints.Movies.Create}/{movie.Id}", movie);
        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id.ToString() }, movie);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug,CancellationToken cancellationToken)
    {
        var movie = Guid.TryParse(idOrSlug.ToString(), out var id) ? 
                    await _movieService.GetByIdAsync(id,_userIdentityProvider.GetUserId(HttpContext),cancellationToken):
                    await _movieService.GetBySlugAsync(idOrSlug,_userIdentityProvider.GetUserId(HttpContext),cancellationToken);

        //var movie = await _movieRepository.GetByIdAsync(idOrSlug);

        if (movie == null)
        {
            return NotFound();
        }
        return Ok(movie.MapToResponse());

    }

    //[Authorize]
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)//[FromRoute] Guid id)
    {
        var userId = _userIdentityProvider.GetUserId(HttpContext);

        var movies = await _movieService.GetAllAsynch(_userIdentityProvider.GetUserId(HttpContext),cancellationToken);

        if (movies is null)
        {
            return NotFound();
        }
        
        _logger.LogCritical($"Got {movies.Count()} movies");

        return Ok(movies.MapToResponse());
    }
    
    
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request,CancellationToken cancellationToken)
    {
        //first get the movie
        var movie = await _movieService.GetByIdAsync(id,_userIdentityProvider.GetUserId(HttpContext),cancellationToken);

        if (movie is null)
        {
            return NotFound();
        }

        var movieUpdated = request.MapToMovie(id);

        var movieFromUpdate = await _movieService.UpdateAsync(movieUpdated,_userIdentityProvider.GetUserId(HttpContext),cancellationToken);

        if (movieFromUpdate is null)
        {
            return NotFound();
        }

        return Ok(movieUpdated);
    }

    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id,CancellationToken cancellationToken)
    {
        //first get the movie
        var movie = await _movieService.GetByIdAsync(id,_userIdentityProvider.GetUserId(HttpContext),cancellationToken);

        if (movie is null)
        {
            return NotFound();
        }

        var result = await _movieService.DeleteByIdAsync(id,_userIdentityProvider.GetUserId(HttpContext),cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }

}