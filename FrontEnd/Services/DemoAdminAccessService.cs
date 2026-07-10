namespace FrontEnd.Services;

public sealed class DemoAdminAccessService : IAdminAccessService
{
    private const string DemoUsername = "admin";
    private const string DemoPassword = "portfolio-demo";

    public bool IsAuthenticated { get; private set; }

    public Task<bool> SignInAsync(string username, string password)
    {
        IsAuthenticated = username == DemoUsername && password == DemoPassword;
        return Task.FromResult(IsAuthenticated);
    }

    public Task SignOutAsync()
    {
        IsAuthenticated = false;
        return Task.CompletedTask;
    }
}
