namespace DtoLayer.Movies;

public record MovieQueryParameters
{
    public int? GenreId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
