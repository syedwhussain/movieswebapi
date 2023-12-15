
namespace Movies.Application.Models;

public class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }

    public string Slug => GenerateSlug();
    
    public float? Rating { get; set; }
    public int? UserRating { get; set; }

    private string GenerateSlug()
    {
        var slug = $"{Title.ToLower().Replace(' ', '-')}-{YearOfRelease}";
        return slug;
    }

    public required int YearOfRelease { get; set; }
    public required List<string> Genres { get; set; } = new();
    //mutable 

}

