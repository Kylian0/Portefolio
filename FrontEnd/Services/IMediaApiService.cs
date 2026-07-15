using FrontEnd.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace FrontEnd.Services;

public interface IMediaApiService
{
    Task<IReadOnlyList<MediaApiDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaApiDto>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default);
    Task<MediaApiDto> UploadAsync(IBrowserFile file, uint? projectId, IProgress<int>? progress = null, CancellationToken cancellationToken = default);
    Task<MediaApiDto> UpdateAsync(uint id, MediaApiDto media, CancellationToken cancellationToken = default);
    Task DeleteAsync(uint id, CancellationToken cancellationToken = default);
}
