using System.Net.Http.Headers;
using Dapper;
using Movies.Application.Models;
using Npgsql.Internal;

namespace Movies.Application;

public class MovieRepositoryPostgres : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepositoryPostgres(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie)
    {
        //create the connection 
        //so the using declarations were intriduced in C# 8.0!!!
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        //being tranction
        var transaction = connection.BeginTransaction();

        //create the movie
        var result = await connection.ExecuteAsync(@"
            INSERT INTO movies (id, slug, title, yearofrelease)
            VALUES (@Id, @Slug, @Title, @YearOfRelease);", movie);


        //result is > 0, then create the genres
        if (result > 0)
        {
            //create the genres
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO genres (movieId, name)
                    VALUES (@MovieId, @Name);", new { MovieId = movie.Id, Name = genre });
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        //write code to get Movie by id from database
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                commandText: "SELECT * FROM movies WHERE id = @Id;",
                parameters: new { Id = id }
            ));

        if (movie is null)
        {
            return null;
        }

        //get all the genres for the movie
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                commandText: "SELECT name FROM genres WHERE movieId = @MovieId;",
                parameters: new { MovieId = id }
            ));

        //add tje genres to init only property movie.genres
        movie.Genres = genres.ToList();


        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        //write code to get Movie by id from database
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                commandText: "SELECT * FROM movies WHERE slug = @slug;",
                parameters: new { slug = slug }
            ));

        if (movie is null)
        {
            return null;
        }

        //get all the genres for the movie
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                commandText: "SELECT name FROM genres WHERE movieId = @MovieId;",
                parameters: new { MovieId = movie.Id }
            ));

        //add tje genres to init only property movie.genres
        movie.Genres = genres.ToList();


        return movie;
    }


    public async Task<IEnumerable<Movie>> GetAllAsynch()
    {
        //get all movies
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        string sql = @"SELECT m.*, string_agg(g.name,',') as genres  FROM movies m  
                        left outer join genres g on m.id = g.movieid 
                        group by m.id";

        var result = await connection.QueryAsync(
            new CommandDefinition(
              commandText: sql
            ));

        //create movie object from result records
        return result.Select(r => new Movie
        {
            Id = r.id,
            Title = r.title,
            YearOfRelease = r.yearofrelease,
            Genres = Enumerable.ToList(r.genres.Split(','))
        });

    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(@"
                        UPDATE movies 
                        SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease 
                        WHERE id = @Id;", movie);

        if (result > 0)
        {
            // Delete existing genres
            await connection.ExecuteAsync(@"
                        DELETE FROM genres 
                        WHERE movieId = @MovieId;", new { MovieId = movie.Id });

            // Insert new genres
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(@"
                INSERT INTO genres (movieId, name)
                VALUES (@MovieId, @Name);", new { MovieId = movie.Id, Name = genre });
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var transaction = connection.BeginTransaction();
    
        // Delete existing genres
        await connection.ExecuteAsync(@"
                    DELETE FROM genres 
                    WHERE movieid = @Id;", new { Id = id });
    
        //delete the main record
        var result = await connection.ExecuteAsync(@"
                    DELETE FROM movies 
                    WHERE id = @Id;", new { Id = id });

    
        transaction.Commit();

        return result > 0;
    }
    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var movie = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            commandText: "SELECT EXISTS(SELECT 1 FROM movies WHERE id = @Id);",
            parameters: new { Id = id }
        ));

        return movie != null;
    }

}