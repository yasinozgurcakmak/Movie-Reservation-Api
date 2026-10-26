namespace EntityLayer.Entities;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PosterUrl { get; set; } = null!;
    public int DurationMinutes { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public List<Genre> Genres { get; set; } = new();
}
