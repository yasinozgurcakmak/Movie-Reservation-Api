namespace DtoLayer.Movies;

public record MovieRequest(
    string Title,
    string Description,
    string PosterUrl,
    int DurationMinutes,
    DateOnly ReleaseDate,
    List<int> GenreIds);
