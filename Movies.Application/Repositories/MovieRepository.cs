using Dapper;
using Movies.Application.Models;
using Serilog;

namespace Movies.Application;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, Guid? userId = default,CancellationToken cancellationToken = default)
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

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default,CancellationToken cancellationToken = default)
    {
        //write code to get Movie by id from database
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                commandText: @"select m.*, round(avg(r.rating), 1) as rating, myr.rating as userrating 
                                from movies m
                                left join ratings r on m.id = r.movieid
                                left join ratings myr on m.id = myr.movieid
                                    and myr.userid = @userId
                                where id = @id
                                group by id, userrating",
                parameters: new { Id = id, userId },cancellationToken:cancellationToken
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

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default,CancellationToken cancellationToken = default)
    {
        //write code to get Movie by id from database
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                commandText: @"select m.*, round(avg(r.rating), 1) as rating, myr.rating as userrating 
                                from movies m
                                left join ratings r on m.id = r.movieid
                                left join ratings myr on m.id = myr.movieid
                                    and myr.userid = @userId
                                WHERE slug = @slug;
                                group by id, userrating ",
                parameters: new { slug = slug, userId },cancellationToken:cancellationToken
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


    public async Task<IEnumerable<Movie>> GetAllAsynch(Guid? userId = default,CancellationToken cancellationToken = default)
    {
                using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync(new CommandDefinition("""
            select m.*, 
                   string_agg(distinct g.name, ',') as genres , 
                   round(avg(r.rating), 1) as rating, 
                   myr.rating as userrating
            from movies m 
            left join genres g on m.id = g.movieid
            left join ratings r on m.id = r.movieid
            left join ratings myr on m.id = myr.movieid
                and myr.userid = @userId
            group by id, userrating
            """, new { userId }, cancellationToken: cancellationToken));
        
        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Rating = (float?)x.rating,
            UserRating = (int?)x.userrating,
            Genres = Enumerable.ToList(x.genres.Split(','))
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

    public async Task<bool> DeleteByIdAsync(Guid id, Guid? userId = default,CancellationToken cancellationToken = default)
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