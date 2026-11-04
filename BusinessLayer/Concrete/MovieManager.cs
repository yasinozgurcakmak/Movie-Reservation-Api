using BusinessLayer.Common;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Common;
using DtoLayer.Movies;
using EntityLayer.Entities;
using FluentValidation;

namespace BusinessLayer.Concrete;

public class MovieManager : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IValidator<MovieRequest> _movieValidator;
    private readonly IValidator<MovieQueryParameters> _queryValidator;
    private readonly IValidator<MovieStatusRequest> _statusValidator;
    private readonly CatalogCache _catalogCache;

    public MovieManager(
        IMovieRepository movieRepository,
        IGenreRepository genreRepository,
        IValidator<MovieRequest> movieValidator,
        IValidator<MovieQueryParameters> queryValidator,
        IValidator<MovieStatusRequest> statusValidator,
        CatalogCache catalogCache)
    {
        _movieRepository = movieRepository;
        _genreRepository = genreRepository;
        _movieValidator = movieValidator;
        _queryValidator = queryValidator;
        _statusValidator = statusValidator;
        _catalogCache = catalogCache;
    }

    public async Task<PagedResponse<MovieResponse>> GetPagedAsync(MovieQueryParameters query, CancellationToken cancellationToken = default)
    {
        await _queryValidator.ValidateAndThrowAsync(query);

        var cacheKey = $"movies:genre:{query.GenreId?.ToString() ?? "all"}:page:{query.Page}:size:{query.PageSize}";
        return await _catalogCache.GetOrCreateAsync(cacheKey, () => BuildPageAsync(query, cancellationToken));
    }

    private async Task<PagedResponse<MovieResponse>> BuildPageAsync(MovieQueryParameters query, CancellationToken cancellationToken)
    {
        var (movies, totalCount) = await _movieRepository.GetPagedAsync(query.GenreId, query.Page, query.PageSize, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResponse<MovieResponse>(
            movies.Select(m => m.ToResponse()).ToList(),
            query.Page,
            query.PageSize,
            totalCount,
            totalPages);
    }

    public async Task<MovieResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var movie = await GetExistingMovieAsync(id, cancellationToken);
        return movie.ToResponse();
    }

    public async Task<MovieResponse> CreateAsync(MovieRequest request)
    {
        await _movieValidator.ValidateAndThrowAsync(request);

        var movie = new Movie { CreatedAt = DateTimeOffset.UtcNow };
        await ApplyRequestAsync(movie, request);

        await _movieRepository.AddAsync(movie);
        await _movieRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        return movie.ToResponse();
    }

    public async Task<MovieResponse> UpdateAsync(int id, MovieRequest request)
    {
        await _movieValidator.ValidateAndThrowAsync(request);

        var movie = await GetExistingMovieAsync(id);
        if (request.Version is not null && request.Version != movie.Version)
            throw new ConflictException($"Movie with id {id} was modified by someone else. Reload it and try again.");

        await ApplyRequestAsync(movie, request);

        await _movieRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        return movie.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var movie = await GetExistingMovieAsync(id);
        await EnsureNoUpcomingReservationsAsync(id);

        movie.IsDeleted = true;
        movie.DeletedAt = DateTimeOffset.UtcNow;
        await _movieRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();
    }

    public async Task<MovieResponse> UpdateStatusAsync(int id, MovieStatusRequest request)
    {
        await _statusValidator.ValidateAndThrowAsync(request);

        var movie = await _movieRepository.GetByIdIncludingDeletedAsync(id)
            ?? throw new NotFoundException($"Movie with id {id} was not found.");

        var isDeleted = request.IsDeleted!.Value;
        if (movie.IsDeleted == isDeleted)
            throw new ConflictException($"Movie with id {id} is already {(isDeleted ? "deleted" : "active")}.");

        if (isDeleted) await EnsureNoUpcomingReservationsAsync(id);

        movie.IsDeleted = isDeleted;
        movie.DeletedAt = isDeleted ? DateTimeOffset.UtcNow : null;
        await _movieRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        return movie.ToResponse();
    }

    private async Task ApplyRequestAsync(Movie movie, MovieRequest request)
    {
        var genreIds = request.GenreIds.Distinct().ToList();
        var genres = await _genreRepository.GetByIdsAsync(genreIds);

        var missingIds = genreIds.Except(genres.Select(g => g.Id)).ToList();
        if (missingIds.Count > 0) throw new BadRequestException($"Genres not found: {string.Join(", ", missingIds)}.");

        movie.Title = request.Title.Trim();
        movie.Description = request.Description.Trim();
        movie.PosterUrl = request.PosterUrl.Trim();
        movie.DurationMinutes = request.DurationMinutes;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Genres.Clear();
        movie.Genres.AddRange(genres);
    }

    // A deleted movie hides its showtimes, which would strand the tickets already sold for them.
    private async Task EnsureNoUpcomingReservationsAsync(int id)
    {
        if (await _movieRepository.HasUpcomingReservationsAsync(id, DateTimeOffset.UtcNow))
            throw new ConflictException($"Movie with id {id} has upcoming showtimes with reservations. Cancel those showtimes first.");
    }

    private async Task<Movie> GetExistingMovieAsync(int id, CancellationToken cancellationToken = default) =>
        await _movieRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Movie with id {id} was not found.");
}
