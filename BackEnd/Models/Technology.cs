using BackEnd.Exceptions;
using MySqlConnector;

namespace BackEnd.Models;

public sealed class Technology
{
    private const string Columns = "t.id, t.category_id, tc.name AS category_name, tc.slug AS category_slug, t.name, t.slug, t.icon_url, t.official_url, t.created_at, t.updated_at";
    private const string FromClause = " FROM technologies t INNER JOIN technology_categories tc ON tc.id = t.category_id ";
    private readonly IConfiguration? configuration;

    public Technology() { }
    public Technology(IConfiguration configuration) => this.configuration = configuration;

    public uint Id { get; init; }
    public uint CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required string CategorySlug { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? IconUrl { get; init; }
    public string? OfficialUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public Task<IReadOnlyList<Technology>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetManyAsync("SELECT " + Columns + FromClause + "ORDER BY tc.display_order ASC, tc.name ASC, t.name ASC;", null, cancellationToken);

    public async Task<Technology?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        return await GetByIdAsync(connection, id, cancellationToken);
    }

    public Task<IReadOnlyList<Technology>> GetByCategoryIdAsync(uint categoryId, CancellationToken cancellationToken = default) =>
        GetManyAsync("SELECT " + Columns + FromClause + "WHERE t.category_id = @categoryId ORDER BY t.name ASC;", categoryId, cancellationToken);

    public async Task<Technology> CreateAsync(Technology technology, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO technologies (category_id, name, slug, icon_url, official_url) VALUES (@categoryId, @name, @slug, @iconUrl, @officialUrl);";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new MySqlCommand(sql, connection);
            AddParameters(command, technology);
            await command.ExecuteNonQueryAsync(cancellationToken);
            var id = checked((uint)command.LastInsertedId);
            return await GetByIdAsync(connection, id, cancellationToken)
                ?? throw new InvalidOperationException("The technology was created but could not be retrieved.");
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            throw new TechnologyConflictException(exception);
        }
    }

    public async Task<Technology?> UpdateAsync(uint id, Technology technology, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE technologies SET category_id = @categoryId, name = @name, slug = @slug, icon_url = @iconUrl, official_url = @officialUrl WHERE id = @id;";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new MySqlCommand(sql, connection);
            AddParameters(command, technology);
            command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
            await command.ExecuteNonQueryAsync(cancellationToken);
            return await GetByIdAsync(connection, id, cancellationToken);
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            throw new TechnologyConflictException(exception);
        }
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using (var usageCommand = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM project_technologies WHERE technology_id = @id);", connection))
            {
                usageCommand.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
                if (Convert.ToBoolean(await usageCommand.ExecuteScalarAsync(cancellationToken)))
                {
                    throw new TechnologyInUseException();
                }
            }

            await using var command = new MySqlCommand("DELETE FROM technologies WHERE id = @id;", connection);
            command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (MySqlException exception) when (exception.Number == 1451)
        {
            throw new TechnologyInUseException(exception);
        }
    }

    private async Task<IReadOnlyList<Technology>> GetManyAsync(string sql, uint? categoryId, CancellationToken cancellationToken)
    {
        var technologies = new List<Technology>();
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        if (categoryId.HasValue) command.Parameters.Add("@categoryId", MySqlDbType.UInt32).Value = categoryId.Value;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) technologies.Add(Map(reader));
        return technologies;
    }

    private static async Task<Technology?> GetByIdAsync(MySqlConnection connection, uint id, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand("SELECT " + Columns + FromClause + "WHERE t.id = @id LIMIT 1;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private static void AddParameters(MySqlCommand command, Technology technology)
    {
        command.Parameters.Add("@categoryId", MySqlDbType.UInt32).Value = technology.CategoryId;
        command.Parameters.Add("@name", MySqlDbType.VarChar).Value = technology.Name;
        command.Parameters.Add("@slug", MySqlDbType.VarChar).Value = technology.Slug;
        command.Parameters.Add("@iconUrl", MySqlDbType.VarChar).Value = technology.IconUrl is null ? DBNull.Value : technology.IconUrl;
        command.Parameters.Add("@officialUrl", MySqlDbType.VarChar).Value = technology.OfficialUrl is null ? DBNull.Value : technology.OfficialUrl;
    }

    private string GetConnectionString() => configuration?.GetConnectionString("PortfolioDatabase") is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException("The PortfolioDatabase connection string is not configured.");

    private static Technology Map(MySqlDataReader reader) => new()
    {
        Id = reader.GetUInt32("id"),
        CategoryId = reader.GetUInt32("category_id"),
        CategoryName = reader.GetString("category_name"),
        CategorySlug = reader.GetString("category_slug"),
        Name = reader.GetString("name"),
        Slug = reader.GetString("slug"),
        IconUrl = reader.IsDBNull(reader.GetOrdinal("icon_url")) ? null : reader.GetString("icon_url"),
        OfficialUrl = reader.IsDBNull(reader.GetOrdinal("official_url")) ? null : reader.GetString("official_url"),
        CreatedAt = reader.GetDateTime("created_at"),
        UpdatedAt = reader.GetDateTime("updated_at")
    };
}
