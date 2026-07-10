using FrontEnd.Models;

namespace FrontEnd.Services;

public interface IAdminProjectService
{
    Task<IReadOnlyList<AdminProject>> GetAllAsync();
    Task UpdateAsync(AdminProject project);
    Task DeleteAsync(string slug);
}
