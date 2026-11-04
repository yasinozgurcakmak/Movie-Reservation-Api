using BusinessLayer.Common;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Genres;
using EntityLayer.Entities;
using FluentValidation;

namespace BusinessLayer.Concrete;

public class GenreManager : IGenreService
{
    private readonly IGenreRepository _genreRepository;
    private readonly IValidator<GenreRequest> _genreValidator;
    private readonly CatalogCache _catalogCache;

    public GenreManager(IGenreRepository genreRepository, IValidator<GenreRequest> genreValidator, CatalogCache catalogCache)
    {
        _genreRepository = genreRepository;
        _genreValidator = genreValidator;
        _catalogCache = catalogCache;
    }

    public Task<List<GenreResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _catalogCache.GetOrCreateAsync("genres", async () =>
        {
            var genres = await _genreRepository.GetAllAsync(cancellationToken);
            return genres.Select(g => g.ToResponse()).ToList();
        });

    public async Task<GenreResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var genre = await GetExistingGenreAsync(id, cancellationToken);
        return genre.ToResponse();
    }

    public async Task<GenreResponse> CreateAsync(GenreRequest request)
    {
        await _genreValidator.ValidateAndThrowAsync(request);

        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name);

        var genre = new Genre { Name = name };
        await _genreRepository.AddAsync(genre);
        await _genreRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        return genre.ToResponse();
    }

    public async Task<GenreResponse> UpdateAsync(int id, GenreRequest request)
    {
        await _genreValidator.ValidateAndThrowAsync(request);

        var genre = await GetExistingGenreAsync(id);
        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name, id);

        genre.Name = name;
        await _genreRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        return genre.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var genre = await GetExistingGenreAsync(id);

        var movieCount = await _genreRepository.CountMoviesUsingAsync(id);
        if (movieCount > 0)
            throw new ConflictException($"Genre with id {id} is used by {movieCount} movie(s) and cannot be deleted.");

        _genreRepository.Remove(genre);
        await _genreRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();
    }

    private async Task EnsureNameIsUniqueAsync(string name, int? excludeId = null)
    {
        if (await _genreRepository.NameExistsAsync(name, excludeId))
            throw new ConflictException($"A genre with name '{name}' already exists.");
    }

    private async Task<Genre> GetExistingGenreAsync(int id, CancellationToken cancellationToken = default) =>
        await _genreRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Genre with id {id} was not found.");
}
