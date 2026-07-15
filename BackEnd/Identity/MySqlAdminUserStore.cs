using Microsoft.AspNetCore.Identity;
using MySqlConnector;

namespace BackEnd.Identity;

public sealed class MySqlAdminUserStore(IConfiguration configuration) :
    IUserPasswordStore<AdminUser>, IUserSecurityStampStore<AdminUser>, IUserLockoutStore<AdminUser>
{
    private string ConnectionString => configuration.GetConnectionString("PortfolioDatabase")
        ?? throw new InvalidOperationException("The PortfolioDatabase connection string is not configured.");

    public async Task<IdentityResult> CreateAsync(AdminUser user, CancellationToken cancellationToken)
    {
        const string sql = "INSERT INTO admin_users (id,user_name,normalized_user_name,password_hash,security_stamp,access_failed_count,lockout_end,lockout_enabled) VALUES (@id,@name,@normalized,@hash,@stamp,@failed,@lockout,@enabled);";
        try { await ExecuteAsync(sql, user, cancellationToken); return IdentityResult.Success; }
        catch (MySqlException ex) when (ex.Number == 1062) { return IdentityResult.Failed(new IdentityError { Code = "DuplicateUserName", Description = "Cet identifiant existe déjà." }); }
    }

    public async Task<IdentityResult> UpdateAsync(AdminUser user, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE admin_users SET user_name=@name,normalized_user_name=@normalized,password_hash=@hash,security_stamp=@stamp,access_failed_count=@failed,lockout_end=@lockout,lockout_enabled=@enabled WHERE id=@id;";
        await ExecuteAsync(sql, user, cancellationToken); return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(AdminUser user, CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(ConnectionString); await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand("DELETE FROM admin_users WHERE id=@id", connection); command.Parameters.AddWithValue("@id", user.Id);
        await command.ExecuteNonQueryAsync(cancellationToken); return IdentityResult.Success;
    }

    public Task<AdminUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => FindAsync("id", userId, cancellationToken);
    public Task<AdminUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => FindAsync("normalized_user_name", normalizedUserName, cancellationToken);
    public Task<string> GetUserIdAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
    public Task<string?> GetUserNameAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.UserName);
    public Task SetUserNameAsync(AdminUser user, string? userName, CancellationToken cancellationToken) { user.UserName = userName ?? string.Empty; return Task.CompletedTask; }
    public Task<string?> GetNormalizedUserNameAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(AdminUser user, string? normalizedName, CancellationToken cancellationToken) { user.NormalizedUserName = normalizedName ?? string.Empty; return Task.CompletedTask; }
    public Task SetPasswordHashAsync(AdminUser user, string? passwordHash, CancellationToken cancellationToken) { user.PasswordHash = passwordHash; return Task.CompletedTask; }
    public Task<string?> GetPasswordHashAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash);
    public Task<bool> HasPasswordAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash is not null);
    public Task SetSecurityStampAsync(AdminUser user, string stamp, CancellationToken cancellationToken) { user.SecurityStamp = stamp; return Task.CompletedTask; }
    public Task<string?> GetSecurityStampAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.SecurityStamp);
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult(user.LockoutEnd);
    public Task SetLockoutEndDateAsync(AdminUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken) { user.LockoutEnd = lockoutEnd; return Task.CompletedTask; }
    public Task<int> IncrementAccessFailedCountAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult(++user.AccessFailedCount);
    public Task ResetAccessFailedCountAsync(AdminUser user, CancellationToken cancellationToken) { user.AccessFailedCount = 0; return Task.CompletedTask; }
    public Task<int> GetAccessFailedCountAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult(user.AccessFailedCount);
    public Task<bool> GetLockoutEnabledAsync(AdminUser user, CancellationToken cancellationToken) => Task.FromResult(user.LockoutEnabled);
    public Task SetLockoutEnabledAsync(AdminUser user, bool enabled, CancellationToken cancellationToken) { user.LockoutEnabled = enabled; return Task.CompletedTask; }
    public void Dispose() { }

    private async Task ExecuteAsync(string sql, AdminUser user, CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(ConnectionString); await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection); AddParameters(command, user); await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<AdminUser?> FindAsync(string column, string value, CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(ConnectionString); await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand($"SELECT id,user_name,normalized_user_name,password_hash,security_stamp,access_failed_count,lockout_end,lockout_enabled FROM admin_users WHERE {column}=@value LIMIT 1", connection);
        command.Parameters.AddWithValue("@value", value); await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new AdminUser { Id=reader.GetGuid("id").ToString(), UserName=reader.GetString("user_name"), NormalizedUserName=reader.GetString("normalized_user_name"), PasswordHash=reader.IsDBNull(reader.GetOrdinal("password_hash"))?null:reader.GetString("password_hash"), SecurityStamp=reader.GetString("security_stamp"), AccessFailedCount=reader.GetInt32("access_failed_count"), LockoutEnd=reader.IsDBNull(reader.GetOrdinal("lockout_end"))?null:new DateTimeOffset(reader.GetDateTime("lockout_end"),TimeSpan.Zero), LockoutEnabled=reader.GetBoolean("lockout_enabled") };
    }

    private static void AddParameters(MySqlCommand command, AdminUser user)
    {
        command.Parameters.AddWithValue("@id",user.Id);command.Parameters.AddWithValue("@name",user.UserName);command.Parameters.AddWithValue("@normalized",user.NormalizedUserName);command.Parameters.AddWithValue("@hash",user.PasswordHash);command.Parameters.AddWithValue("@stamp",user.SecurityStamp);command.Parameters.AddWithValue("@failed",user.AccessFailedCount);command.Parameters.AddWithValue("@lockout",user.LockoutEnd?.UtcDateTime);command.Parameters.AddWithValue("@enabled",user.LockoutEnabled);
    }
}
