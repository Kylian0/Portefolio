using MySqlConnector;

namespace BackEnd.Models;

public sealed class ProjectLearning
{
    private const string Columns = "pl.id, pl.project_id, p.title AS project_title, pl.content, pl.display_order, pl.created_at";
    private const string FromClause = " FROM project_learnings pl INNER JOIN projects p ON p.id = pl.project_id ";
    private readonly IConfiguration? configuration;

    public ProjectLearning() { }
    public ProjectLearning(IConfiguration configuration) => this.configuration = configuration;

    public uint Id { get; init; }
    public uint ProjectId { get; init; }
    public required string ProjectTitle { get; init; }
    public required string Content { get; init; }
    public int DisplayOrder { get; init; }
    public DateTime CreatedAt { get; init; }

    public Task<IReadOnlyList<ProjectLearning>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetManyAsync("SELECT " + Columns + FromClause + "ORDER BY p.title ASC, pl.display_order ASC, pl.id ASC;", null, cancellationToken);

    public Task<IReadOnlyList<ProjectLearning>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default) =>
        GetManyAsync("SELECT " + Columns + FromClause + "WHERE pl.project_id = @projectId ORDER BY pl.display_order ASC, pl.id ASC;", projectId, cancellationToken);

    public async Task<ProjectLearning?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        return await GetByIdAsync(connection, id, cancellationToken);
    }

    public async Task<ProjectLearning> CreateAsync(ProjectLearning learning, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO project_learnings (project_id, content, display_order) VALUES (@projectId, @content, @displayOrder);";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, learning);
        await command.ExecuteNonQueryAsync(cancellationToken);
        var id = checked((uint)command.LastInsertedId);
        return await GetByIdAsync(connection, id, cancellationToken)
            ?? throw new InvalidOperationException("The project learning was created but could not be retrieved.");
    }

    public async Task<ProjectLearning?> UpdateAsync(uint id, ProjectLearning learning, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE project_learnings SET project_id = @projectId, content = @content, display_order = @displayOrder WHERE id = @id;";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, learning);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return await GetByIdAsync(connection, id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand("DELETE FROM project_learnings WHERE id = @id;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private async Task<IReadOnlyList<ProjectLearning>> GetManyAsync(string sql, uint? projectId, CancellationToken cancellationToken)
    {
        var learnings = new List<ProjectLearning>();
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        if (projectId.HasValue) command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = projectId.Value;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) learnings.Add(Map(reader));
        return learnings;
    }

    private static async Task<ProjectLearning?> GetByIdAsync(MySqlConnection connection, uint id, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand("SELECT " + Columns + FromClause + "WHERE pl.id = @id LIMIT 1;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private static void AddParameters(MySqlCommand command, ProjectLearning learning)
    {
        command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = learning.ProjectId;
        command.Parameters.Add("@content", MySqlDbType.VarChar).Value = learning.Content;
        command.Parameters.Add("@displayOrder", MySqlDbType.Int32).Value = learning.DisplayOrder;
    }

    private string GetConnectionString() => configuration?.GetConnectionString("PortfolioDatabase") is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException("The PortfolioDatabase connection string is not configured.");

    private static ProjectLearning Map(MySqlDataReader reader) => new()
    {
        Id = reader.GetUInt32("id"),
        ProjectId = reader.GetUInt32("project_id"),
        ProjectTitle = reader.GetString("project_title"),
        Content = reader.GetString("content"),
        DisplayOrder = reader.GetInt32("display_order"),
        CreatedAt = reader.GetDateTime("created_at")
    };
}
