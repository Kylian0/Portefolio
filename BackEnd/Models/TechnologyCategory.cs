using BackEnd.Exceptions;
using MySqlConnector;

namespace BackEnd.Models;

public sealed class TechnologyCategory
{
    private const string Columns = "id, name, slug, display_order, created_at";
    private readonly IConfiguration? configuration;

    public TechnologyCategory() { }
    public TechnologyCategory(IConfiguration configuration) => this.configuration = configuration;

    public uint Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public int DisplayOrder { get; init; }
    public DateTime CreatedAt { get; init; }

    public async Task<IReadOnlyList<TechnologyCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = new List<TechnologyCategory>();
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand($"SELECT {Columns} FROM technology_categories ORDER BY display_order ASC, name ASC;", connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) categories.Add(Map(reader));
        return categories;
    }

    public async Task<TechnologyCategory?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        return await GetByIdAsync(connection, id, cancellationToken);
    }

    public async Task<TechnologyCategory> CreateAsync(TechnologyCategory category, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new MySqlCommand("INSERT INTO technology_categories (name, slug, display_order) VALUES (@name, @slug, @displayOrder);", connection);
            AddParameters(command, category);
            await command.ExecuteNonQueryAsync(cancellationToken);
            var id = checked((uint)command.LastInsertedId);
            return await GetByIdAsync(connection, id, cancellationToken)
                ?? throw new InvalidOperationException("The technology category was created but could not be retrieved.");
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            throw new TechnologyCategoryConflictException(exception);
        }
    }

    public async Task<TechnologyCategory?> UpdateAsync(uint id, TechnologyCategory category, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new MySqlCommand("UPDATE technology_categories SET name = @name, slug = @slug, display_order = @displayOrder WHERE id = @id;", connection);
            AddParameters(command, category);
            command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
            await command.ExecuteNonQueryAsync(cancellationToken);
            return await GetByIdAsync(connection, id, cancellationToken);
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            throw new TechnologyCategoryConflictException(exception);
        }
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new MySqlCommand("DELETE FROM technology_categories WHERE id = @id;", connection);
            command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (MySqlException exception) when (exception.Number == 1451)
        {
            throw new TechnologyCategoryInUseException(exception);
        }
    }

    private static async Task<TechnologyCategory?> GetByIdAsync(MySqlConnection connection, uint id, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand($"SELECT {Columns} FROM technology_categories WHERE id = @id LIMIT 1;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private static void AddParameters(MySqlCommand command, TechnologyCategory category)
    {
        command.Parameters.Add("@name", MySqlDbType.VarChar).Value = category.Name;
        command.Parameters.Add("@slug", MySqlDbType.VarChar).Value = category.Slug;
        command.Parameters.Add("@displayOrder", MySqlDbType.Int32).Value = category.DisplayOrder;
    }

    private string GetConnectionString() => configuration?.GetConnectionString("PortfolioDatabase") is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException("The PortfolioDatabase connection string is not configured.");

    private static TechnologyCategory Map(MySqlDataReader reader) => new()
    {
        Id = reader.GetUInt32("id"),
        Name = reader.GetString("name"),
        Slug = reader.GetString("slug"),
        DisplayOrder = reader.GetInt32("display_order"),
        CreatedAt = reader.GetDateTime("created_at")
    };
}
