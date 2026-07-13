using MySqlConnector;

namespace BackEnd.Models;

public sealed class ProjectDocument
{
    private readonly IConfiguration? configuration;

    public ProjectDocument() { }
    public ProjectDocument(IConfiguration configuration) => this.configuration = configuration;

    public uint Id { get; init; }
    public uint ProjectId { get; init; }
    public required string Title { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    private const string Columns = "id, project_id, title, created_at, updated_at";

    public Task<IReadOnlyList<ProjectDocument>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetManyAsync($"SELECT {Columns} FROM project_documents ORDER BY created_at DESC;", null, cancellationToken);

    public async Task<ProjectDocument?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        return await GetByIdAsync(connection, id, cancellationToken);
    }

    public Task<IReadOnlyList<ProjectDocument>> GetByProjectIdAsync(
        uint projectId,
        CancellationToken cancellationToken = default) =>
        GetManyAsync(
            $"SELECT {Columns} FROM project_documents WHERE project_id = @projectId ORDER BY created_at DESC;",
            projectId,
            cancellationToken);

    public async Task<ProjectDocument> CreateAsync(ProjectDocument document, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO project_documents (project_id, title) VALUES (@projectId, @title);";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        AddWriteParameters(command, document);
        await command.ExecuteNonQueryAsync(cancellationToken);

        var id = checked((uint)command.LastInsertedId);
        return await GetByIdAsync(connection, id, cancellationToken)
            ?? throw new InvalidOperationException("The document was created but could not be retrieved.");
    }

    public async Task<ProjectDocument?> UpdateAsync(
        uint id,
        ProjectDocument document,
        CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE project_documents SET project_id = @projectId, title = @title WHERE id = @id;";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        AddWriteParameters(command, document);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return await GetByIdAsync(connection, id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand("DELETE FROM project_documents WHERE id = @id;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private async Task<IReadOnlyList<ProjectDocument>> GetManyAsync(
        string sql,
        uint? projectId,
        CancellationToken cancellationToken)
    {
        var documents = new List<ProjectDocument>();
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        if (projectId.HasValue)
        {
            command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = projectId.Value;
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            documents.Add(Map(reader));
        }

        return documents;
    }

    private static async Task<ProjectDocument?> GetByIdAsync(
        MySqlConnection connection,
        uint id,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand($"SELECT {Columns} FROM project_documents WHERE id = @id LIMIT 1;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private static void AddWriteParameters(MySqlCommand command, ProjectDocument document)
    {
        command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = document.ProjectId;
        command.Parameters.Add("@title", MySqlDbType.VarChar).Value = document.Title;
    }

    private string GetConnectionString() =>
        configuration?.GetConnectionString("PortfolioDatabase") is { Length: > 0 } connectionString
            ? connectionString
            : throw new InvalidOperationException("The PortfolioDatabase connection string is not configured.");

    private static ProjectDocument Map(MySqlDataReader reader) => new()
    {
        Id = reader.GetUInt32("id"),
        ProjectId = reader.GetUInt32("project_id"),
        Title = reader.GetString("title"),
        CreatedAt = reader.GetDateTime("created_at"),
        UpdatedAt = reader.GetDateTime("updated_at")
    };
}
