using Microsoft.AspNetCore.Identity;
using MySqlConnector;

namespace BackEnd.Identity;

public sealed class AdminIdentityInitializer(IConfiguration configuration, IServiceProvider services, ILogger<AdminIdentityInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("PortfolioDatabase") ?? throw new InvalidOperationException("PortfolioDatabase is not configured.");
        await using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            const string sql = """
                CREATE TABLE IF NOT EXISTS admin_users (
                    id CHAR(36) PRIMARY KEY,
                    user_name VARCHAR(256) NOT NULL,
                    normalized_user_name VARCHAR(256) NOT NULL UNIQUE,
                    password_hash TEXT NULL,
                    security_stamp VARCHAR(100) NOT NULL,
                    access_failed_count INT NOT NULL DEFAULT 0,
                    lockout_end DATETIME NULL,
                    lockout_enabled BOOLEAN NOT NULL DEFAULT TRUE
                );
                """;
            await using var command = new MySqlCommand(sql, connection); await command.ExecuteNonQueryAsync(cancellationToken);
        }

        var username = configuration["AdminBootstrap:Username"];
        var password = configuration["AdminBootstrap:Password"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) { logger.LogWarning("No bootstrap administrator credentials were supplied."); return; }
        await using var scope = services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
        if (await manager.FindByNameAsync(username) is not null) return;
        var result = await manager.CreateAsync(new AdminUser { UserName = username }, password);
        if (!result.Succeeded) throw new InvalidOperationException("Unable to create the initial administrator: " + string.Join("; ", result.Errors.Select(x => x.Description)));
        logger.LogInformation("Initial administrator {UserName} created.", username);
    }
}
