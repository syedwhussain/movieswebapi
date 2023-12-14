using Dapper;
using Movies.Application.Models;
using Serilog;

namespace Movies.Application;

public class MovieRepositoryPostgres : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepositoryPostgres(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        //create the connection 
        //so the using declarations were intriduced in C# 8.0!!!
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        //being tranction
        var transaction = connection.BeginTransaction();

        string commandText = $@"
                INSERT INTO movies (id, slug, title, yearofrelease)
                VALUES ({movie.Id}, {movie.Slug}, {movie.Title}, {movie.YearOfRelease})";
        
        Log.Information($"CommandText: {commandText}");
        
        var commandDefinition = new CommandDefinition(commandText,movie,cancellationToken:cancellationToken);
        
        //create the movie
        var result = await connection.ExecuteAsync(commandDefinition);


        //result is > 0, then create the genres
        if (result > 0)
        {
           
            //create the genres
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition(@"
                    INSERT INTO genres (movieId, name)
                    VALUES (@MovieId, @Name);"
                    , new { MovieId = movie.Id, Name = genre }, cancellationToken:cancellationToken));
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        //write code to get Movie by id from database
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                commandText: "SELECT * FROM movies WHERE id = @Id;",
                parameters: new { Id = id },cancellationToken:cancellationToken
            ));

        if (movie is null)
        {
            return null;
        }

        //get all the genres for the movie
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                commandText: "SELECT name FROM genres WHERE movieId = @MovieId;",
                parameters: new { MovieId = id },cancellationToken:cancellationToken
            ));

        //add tje genres to init only property movie.genres
        movie.Genres = genres.ToList();


        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        //write code to get Movie by id from database
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                commandText: "SELECT * FROM movies WHERE slug = @slug;",
                parameters: new { slug = slug },cancellationToken:cancellationToken
            ));

        if (movie is null)
        {
            return null;
        }

        //get all the genres for the movie
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                commandText: "SELECT name FROM genres WHERE movieId = @MovieId;",
                parameters: new { MovieId = movie.Id },cancellationToken:cancellationToken
            ));

        //add tje genres to init only property movie.genres
        movie.Genres = genres.ToList();


        return movie;
    }


    public async Task<IEnumerable<Movie>> GetAllAsynch(CancellationToken cancellationToken = default)
    {
        //get all movies
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        string sql = @"SELECT m.*, string_agg(g.name,',') as genres  FROM movies m  
                        left outer join genres g on m.id = g.movieid 
                        group by m.id";

        var result = await connection.QueryAsync(
            new CommandDefinition(
              commandText: sql
              ,cancellationToken:cancellationToken));

        //create movie object from result records
        return result.Select(r => new Movie
        {
            Id = r.id,
            Title = r.title,
            YearOfRelease = r.yearofrelease,
            Genres = Enumerable.ToList(r.genres.Split(','))
        });

    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(
                    new CommandDefinition(@"UPDATE movies 
                        SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease 
                        WHERE id = @Id;"
                        , movie
                        ,cancellationToken:cancellationToken));

        if (result > 0)
        {
            // Delete existing genres
            await connection.ExecuteAsync(new CommandDefinition(@"
                        DELETE FROM genres 
                        WHERE movieId = @MovieId;", new { MovieId = movie.Id },cancellationToken:cancellationToken));

            // Insert new genres
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO genres (movieId, name)
                VALUES (@MovieId, @Name);", new { MovieId = movie.Id, Name = genre },cancellationToken:cancellationToken));
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var transaction = connection.BeginTransaction();
    
        // Delete existing genres
        await connection.ExecuteAsync(new CommandDefinition(@"
                    DELETE FROM genres 
                    WHERE movieid = @Id;", new { Id = id },cancellationToken:cancellationToken));
    
        //delete the main record
        var result = await connection.ExecuteAsync(new CommandDefinition(@"
                    DELETE FROM movies 
                    WHERE id = @Id;", new { Id = id },cancellationToken:cancellationToken));

    
        transaction.Commit();

        return result > 0;
    }
    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var movie = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            commandText: "SELECT EXISTS(SELECT 1 FROM movies WHERE id = @Id);",
            parameters: new { Id = id },cancellationToken:cancellationToken
        ));

        return movie != null;
    }

}