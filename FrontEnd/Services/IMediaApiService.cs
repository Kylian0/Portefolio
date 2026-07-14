using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IMediaApiService
{
    Task<IReadOnlyList<MediaApiDto>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default);
}
