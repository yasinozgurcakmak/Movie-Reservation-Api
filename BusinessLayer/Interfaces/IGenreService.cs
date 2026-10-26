using DtoLayer.Genres;

namespace BusinessLayer.Interfaces;

public interface IGenreService
{
    Task<List<GenreResponse>> GetAllAsync();
    Task<GenreResponse> GetByIdAsync(int id);
    Task<GenreResponse> CreateAsync(GenreRequest request);
    Task<GenreResponse> UpdateAsync(int id, GenreRequest request);
    Task DeleteAsync(int id);
}
