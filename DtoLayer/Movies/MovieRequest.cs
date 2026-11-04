namespace DtoLayer.Movies;

// Version is optional; when sent on update, the change is rejected if someone else modified the movie since it was read.
public record MovieRequest(
    string Title,
    string Description,
    string PosterUrl,
    int DurationMinutes,
    DateOnly ReleaseDate,
    List<int> GenreIds,
    uint? Version = null);
