using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IAdminMessageService
{
    Task<IReadOnlyList<AdminMessage>> GetAllAsync();
    Task SendAsync(AdminMessage message);
    Task SetReadStatusAsync(Guid id, bool isRead);
    Task DeleteAsync(Guid id);
}
