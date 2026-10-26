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

    public MovieManager(
        IMovieRepository movieRepository,
        IGenreRepository genreRepository,
        IValidator<MovieRequest> movieValidator,
        IValidator<MovieQueryParameters> queryValidator,
        IValidator<MovieStatusRequest> statusValidator)
    {
        _movieRepository = movieRepository;
        _genreRepository = genreRepository;
        _movieValidator = movieValidator;
        _queryValidator = queryValidator;
        _statusValidator = statusValidator;
    }

    public async Task<PagedResponse<MovieResponse>> GetPagedAsync(MovieQueryParameters query)
    {
        await _queryValidator.ValidateAndThrowAsync(query);

        var (movies, totalCount) = await _movieRepository.GetPagedAsync(query.GenreId, query.Page, query.PageSize);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResponse<MovieResponse>(
            movies.Select(m => m.ToResponse()).ToList(),
            query.Page,
            query.PageSize,
            totalCount,
            totalPages);
    }

    public async Task<MovieResponse> GetByIdAsync(int id)
    {
        var movie = await GetExistingMovieAsync(id);
        return movie.ToResponse();
    }

    public async Task<MovieResponse> CreateAsync(MovieRequest request)
    {
        await _movieValidator.ValidateAndThrowAsync(request);

        var movie = new Movie { CreatedAt = DateTimeOffset.UtcNow };
        await ApplyRequestAsync(movie, request);

        await _movieRepository.AddAsync(movie);
        await _movieRepository.SaveChangesAsync();

        return movie.ToResponse();
    }

    public async Task<MovieResponse> UpdateAsync(int id, MovieRequest request)
    {
        await _movieValidator.ValidateAndThrowAsync(request);

        var movie = await GetExistingMovieAsync(id);
        await ApplyRequestAsync(movie, request);

        await _movieRepository.SaveChangesAsync();

        return movie.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var movie = await GetExistingMovieAsync(id);
        movie.IsDeleted = true;
        movie.DeletedAt = DateTimeOffset.UtcNow;
        await _movieRepository.SaveChangesAsync();
    }

    public async Task<MovieResponse> UpdateStatusAsync(int id, MovieStatusRequest request)
    {
        await _statusValidator.ValidateAndThrowAsync(request);

        var movie = await _movieRepository.GetByIdIncludingDeletedAsync(id)
            ?? throw new NotFoundException($"Movie with id {id} was not found.");

        var isDeleted = request.IsDeleted!.Value;
        if (movie.IsDeleted == isDeleted)
            throw new ConflictException($"Movie with id {id} is already {(isDeleted ? "deleted" : "active")}.");

        movie.IsDeleted = isDeleted;
        movie.DeletedAt = isDeleted ? DateTimeOffset.UtcNow : null;
        await _movieRepository.SaveChangesAsync();

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

    private async Task<Movie> GetExistingMovieAsync(int id) =>
        await _movieRepository.GetByIdAsync(id)
        ?? throw new NotFoundException($"Movie with id {id} was not found.");
}
