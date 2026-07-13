using MySqlConnector;

namespace BackEnd.Models;

public sealed class ProjectDocumentBlock
{
    private readonly IConfiguration? configuration;

    public ProjectDocumentBlock() { }
    public ProjectDocumentBlock(IConfiguration configuration) => this.configuration = configuration;

    public uint Id { get; init; }
    public uint DocumentId { get; init; }
    public required string BlockType { get; init; }
    public string? Content { get; init; }
    public string? Settings { get; init; }
    public int DisplayOrder { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    private const string Columns = "id, document_id, block_type, content, settings, display_order, created_at, updated_at";

    public Task<IReadOnlyList<ProjectDocumentBlock>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetManyAsync($"SELECT {Columns} FROM project_document_blocks ORDER BY display_order ASC, id ASC;", null, cancellationToken);

    public async Task<ProjectDocumentBlock?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        return await GetByIdAsync(connection, id, cancellationToken);
    }

    public Task<IReadOnlyList<ProjectDocumentBlock>> GetByDocumentIdAsync(uint documentId, CancellationToken cancellationToken = default) =>
        GetManyAsync($"SELECT {Columns} FROM project_document_blocks WHERE document_id = @documentId ORDER BY display_order ASC, id ASC;", documentId, cancellationToken);

    public async Task<ProjectDocumentBlock> CreateAsync(ProjectDocumentBlock block, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO project_document_blocks (document_id, block_type, content, settings, display_order) VALUES (@documentId, @blockType, @content, @settings, @displayOrder);";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, block);
        await command.ExecuteNonQueryAsync(cancellationToken);
        var id = checked((uint)command.LastInsertedId);
        return await GetByIdAsync(connection, id, cancellationToken)
            ?? throw new InvalidOperationException("The block was created but could not be retrieved.");
    }

    public async Task<ProjectDocumentBlock?> UpdateAsync(uint id, ProjectDocumentBlock block, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE project_document_blocks SET document_id = @documentId, block_type = @blockType, content = @content, settings = @settings, display_order = @displayOrder WHERE id = @id;";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, block);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return await GetByIdAsync(connection, id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand("DELETE FROM project_document_blocks WHERE id = @id;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private async Task<IReadOnlyList<ProjectDocumentBlock>> GetManyAsync(string sql, uint? documentId, CancellationToken cancellationToken)
    {
        var blocks = new List<ProjectDocumentBlock>();
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        if (documentId.HasValue) command.Parameters.Add("@documentId", MySqlDbType.UInt32).Value = documentId.Value;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) blocks.Add(Map(reader));
        return blocks;
    }

    private static async Task<ProjectDocumentBlock?> GetByIdAsync(MySqlConnection connection, uint id, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand($"SELECT {Columns} FROM project_document_blocks WHERE id = @id LIMIT 1;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private static void AddParameters(MySqlCommand command, ProjectDocumentBlock block)
    {
        command.Parameters.Add("@documentId", MySqlDbType.UInt32).Value = block.DocumentId;
        command.Parameters.Add("@blockType", MySqlDbType.VarChar).Value = block.BlockType;
        command.Parameters.Add("@content", MySqlDbType.LongText).Value = block.Content is null ? DBNull.Value : block.Content;
        command.Parameters.Add("@settings", MySqlDbType.JSON).Value = block.Settings is null ? DBNull.Value : block.Settings;
        command.Parameters.Add("@displayOrder", MySqlDbType.Int32).Value = block.DisplayOrder;
    }

    private string GetConnectionString() => configuration?.GetConnectionString("PortfolioDatabase") is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException("The PortfolioDatabase connection string is not configured.");

    private static ProjectDocumentBlock Map(MySqlDataReader reader) => new()
    {
        Id = reader.GetUInt32("id"),
        DocumentId = reader.GetUInt32("document_id"),
        BlockType = reader.GetString("block_type"),
        Content = reader.IsDBNull(reader.GetOrdinal("content")) ? null : reader.GetString("content"),
        Settings = reader.IsDBNull(reader.GetOrdinal("settings")) ? null : reader.GetString("settings"),
        DisplayOrder = reader.GetInt32("display_order"),
        CreatedAt = reader.GetDateTime("created_at"),
        UpdatedAt = reader.GetDateTime("updated_at")
    };
}
