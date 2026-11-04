using DtoLayer.Genres;

namespace BusinessLayer.Interfaces;

public interface IGenreService
{
    Task<List<GenreResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GenreResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GenreResponse> CreateAsync(GenreRequest request);
    Task<GenreResponse> UpdateAsync(int id, GenreRequest request);
    Task DeleteAsync(int id);
}
