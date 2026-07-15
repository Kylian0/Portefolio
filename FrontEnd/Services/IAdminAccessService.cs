namespace FrontEnd.Services;

public interface IAdminAccessService
{
    bool IsAuthenticated { get; }

    Task InitializeAsync();

    Task<bool> SignInAsync(string username, string password);

    Task SignOutAsync();
}
