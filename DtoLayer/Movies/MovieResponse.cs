using DtoLayer.Genres;

namespace DtoLayer.Movies;

public record MovieResponse(
    int Id,
    string Title,
    string Description,
    string PosterUrl,
    int DurationMinutes,
    DateOnly ReleaseDate,
    List<GenreResponse> Genres,
    DateTimeOffset CreatedAt,
    uint Version);
