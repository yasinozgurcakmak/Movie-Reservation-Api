using DtoLayer.Halls;

namespace BusinessLayer.Interfaces;

public interface IHallService
{
    Task<List<HallResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<HallResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<HallResponse> CreateAsync(HallRequest request);
    Task<HallResponse> UpdateAsync(int id, HallRequest request);
    Task DeleteAsync(int id);
}
