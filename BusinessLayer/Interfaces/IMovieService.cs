using DtoLayer.Common;
using DtoLayer.Movies;

namespace BusinessLayer.Interfaces;

public interface IMovieService
{
    Task<PagedResponse<MovieResponse>> GetPagedAsync(MovieQueryParameters query);
    Task<MovieResponse> GetByIdAsync(int id);
    Task<MovieResponse> CreateAsync(MovieRequest request);
    Task<MovieResponse> UpdateAsync(int id, MovieRequest request);
    Task DeleteAsync(int id);
    Task<MovieResponse> UpdateStatusAsync(int id, MovieStatusRequest request);
}
