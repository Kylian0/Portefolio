using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IAdminMessageService
{
    Task<IReadOnlyList<AdminMessage>> GetAllAsync();
    Task SetReadStatusAsync(Guid id, bool isRead);
    Task DeleteAsync(Guid id);
}
