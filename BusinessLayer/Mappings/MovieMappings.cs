using DtoLayer.Genres;
using DtoLayer.Movies;
using EntityLayer.Entities;

namespace BusinessLayer.Mappings;

public static class MovieMappings
{
    public static GenreResponse ToResponse(this Genre genre) => new(genre.Id, genre.Name);

    public static MovieResponse ToResponse(this Movie movie) => new(
        movie.Id,
        movie.Title,
        movie.Description,
        movie.PosterUrl,
        movie.DurationMinutes,
        movie.ReleaseDate,
        movie.Genres.OrderBy(g => g.Name).Select(g => g.ToResponse()).ToList(),
        movie.CreatedAt);
}
