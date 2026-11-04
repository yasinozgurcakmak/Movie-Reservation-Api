using BusinessLayer.Common;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Halls;
using EntityLayer.Entities;
using FluentValidation;

namespace BusinessLayer.Concrete;

public class HallManager : IHallService
{
    private readonly IHallRepository _hallRepository;
    private readonly IValidator<HallRequest> _hallValidator;
    private readonly CatalogCache _catalogCache;

    public HallManager(IHallRepository hallRepository, IValidator<HallRequest> hallValidator, CatalogCache catalogCache)
    {
        _hallRepository = hallRepository;
        _hallValidator = hallValidator;
        _catalogCache = catalogCache;
    }

    public Task<List<HallResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _catalogCache.GetOrCreateAsync("halls", async () =>
        {
            var halls = await _hallRepository.GetAllAsync(cancellationToken);
            return halls.Select(h => h.ToResponse()).ToList();
        });

    public async Task<HallResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var hall = await GetExistingHallAsync(id, cancellationToken);
        return hall.ToResponse();
    }

    public async Task<HallResponse> CreateAsync(HallRequest request)
    {
        await _hallValidator.ValidateAndThrowAsync(request);

        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name);

        var hall = new Hall { Name = name, RowCount = request.RowCount, SeatsPerRow = request.SeatsPerRow };
        await _hallRepository.AddAsync(hall);
        await _hallRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        return hall.ToResponse();
    }

    public async Task<HallResponse> UpdateAsync(int id, HallRequest request)
    {
        await _hallValidator.ValidateAndThrowAsync(request);

        var hall = await GetExistingHallAsync(id);
        if (request.Version is not null && request.Version != hall.Version)
            throw new ConflictException($"Hall with id {id} was modified by someone else. Reload it and try again.");

        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name, id);

        var isResized = hall.RowCount != request.RowCount || hall.SeatsPerRow != request.SeatsPerRow;
        if (isResized && await _hallRepository.CountShowtimesAsync(id) > 0)
            throw new ConflictException($"Hall with id {id} has showtimes, so its size cannot be changed. Create a new hall instead.");

        hall.Name = name;
        hall.RowCount = request.RowCount;
        hall.SeatsPerRow = request.SeatsPerRow;
        await _hallRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        return hall.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var hall = await GetExistingHallAsync(id);

        var showtimeCount = await _hallRepository.CountShowtimesAsync(id);
        if (showtimeCount > 0)
            throw new ConflictException($"Hall with id {id} has {showtimeCount} showtime(s) and cannot be deleted.");

        _hallRepository.Remove(hall);
        await _hallRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();
    }

    private async Task EnsureNameIsUniqueAsync(string name, int? excludeId = null)
    {
        if (await _hallRepository.NameExistsAsync(name, excludeId))
            throw new ConflictException($"A hall with name '{name}' already exists.");
    }

    private async Task<Hall> GetExistingHallAsync(int id, CancellationToken cancellationToken = default) =>
        await _hallRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Hall with id {id} was not found.");
}
