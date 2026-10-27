using DtoLayer.Halls;

namespace BusinessLayer.Interfaces;

public interface IHallService
{
    Task<List<HallResponse>> GetAllAsync();
    Task<HallResponse> GetByIdAsync(int id);
    Task<HallResponse> CreateAsync(HallRequest request);
    Task<HallResponse> UpdateAsync(int id, HallRequest request);
    Task DeleteAsync(int id);
}
