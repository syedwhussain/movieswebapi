using Dapper;

public class DbInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DbInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

  public async Task InitializeAsync()
  {
    using var connection = await _connectionFactory.CreateConnectionAsync();

    await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS movies
            (
                id UUID PRIMARY KEY,
                slug TEXT not null,
                title TEXT not null,
                yearofrelease integer not null,
                created_date timestamp without time zone default (now() at time zone 'utc')
            );
        ");

    //add index on slug field
    await connection.ExecuteAsync(@"
            CREATE unique index concurrently IF NOT EXISTS movies_slug_idx
            ON movies
            USING btree(slug);");

    //add genres table
    await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS genres
            (
                movieId UUID references movies (id),
                name TEXT not null
            );
        ");

// Insert test records into the movies table
    for (int i = 1; i <= 5; i++)
    {
        var movieId = Guid.NewGuid();
        var slug = $"test-movie-{i}";
        var title = $"Test Movie {i}";
        var yearOfRelease = 2000 + i;

        var existingMovie = await connection.QueryFirstOrDefaultAsync(@"
            SELECT * FROM movies WHERE title = @Title",
            new { Title = title });

        if (existingMovie == null)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO movies (id, slug, title, yearofrelease)
                VALUES (@Id, @Slug, @Title, @YearOfRelease)",
                new { Id = movieId, Slug = slug, Title = title, YearOfRelease = yearOfRelease });

            // Insert test records into the genres table
            var genres = new[] { "Action", "Comedy", "Drama", "Horror", "Sci-Fi" };
            foreach (var genre in genres)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO genres (movieId, name)
                    VALUES (@MovieId, @Name)",
                    new { MovieId = movieId, Name = genre });
            }
        }
    }

    //add ratings table
    await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS ratings
            (
                userid uuid,
                movieId UUID references movies (id),
                rating integer not null,
                created_date timestamp without time zone default (now() at time zone 'utc'),
                PRIMARY KEY (userid, movieId)
            );
        ");



  }
}