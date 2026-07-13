using BackEnd.Exceptions;
using MySqlConnector;

namespace BackEnd.Models;

public sealed class ProjectTechnology
{
    private const string Columns = "pt.project_id, p.title AS project_title, p.slug AS project_slug, pt.technology_id, t.name AS technology_name, t.slug AS technology_slug, t.category_id, tc.name AS category_name, pt.is_primary, pt.display_order";
    private const string FromClause = " FROM project_technologies pt INNER JOIN projects p ON p.id = pt.project_id INNER JOIN technologies t ON t.id = pt.technology_id INNER JOIN technology_categories tc ON tc.id = t.category_id ";
    private readonly IConfiguration? configuration;

    public ProjectTechnology() { }
    public ProjectTechnology(IConfiguration configuration) => this.configuration = configuration;

    public uint ProjectId { get; init; }
    public required string ProjectTitle { get; init; }
    public required string ProjectSlug { get; init; }
    public uint TechnologyId { get; init; }
    public required string TechnologyName { get; init; }
    public required string TechnologySlug { get; init; }
    public uint CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }

    public Task<IReadOnlyList<ProjectTechnology>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetManyAsync("SELECT " + Columns + FromClause + "ORDER BY p.title ASC, pt.is_primary DESC, pt.display_order ASC, t.name ASC;", null, null, cancellationToken);

    public Task<IReadOnlyList<ProjectTechnology>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default) =>
        GetManyAsync("SELECT " + Columns + FromClause + "WHERE pt.project_id = @projectId ORDER BY pt.is_primary DESC, pt.display_order ASC, t.name ASC;", projectId, null, cancellationToken);

    public Task<IReadOnlyList<ProjectTechnology>> GetByTechnologyIdAsync(uint technologyId, CancellationToken cancellationToken = default) =>
        GetManyAsync("SELECT " + Columns + FromClause + "WHERE pt.technology_id = @technologyId ORDER BY pt.display_order ASC, p.title ASC;", null, technologyId, cancellationToken);

    public async Task<ProjectTechnology?> GetByIdsAsync(uint projectId, uint technologyId, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        return await GetByIdsAsync(connection, projectId, technologyId, cancellationToken);
    }

    public async Task<ProjectTechnology> CreateAsync(ProjectTechnology relation, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO project_technologies (project_id, technology_id, is_primary, display_order) VALUES (@projectId, @technologyId, @isPrimary, @displayOrder);";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new MySqlCommand(sql, connection);
            AddParameters(command, relation);
            await command.ExecuteNonQueryAsync(cancellationToken);
            return await GetByIdsAsync(connection, relation.ProjectId, relation.TechnologyId, cancellationToken)
                ?? throw new InvalidOperationException("The project technology relation was created but could not be retrieved.");
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            throw new ProjectTechnologyConflictException(exception);
        }
    }

    public async Task<ProjectTechnology?> UpdateAsync(ProjectTechnology relation, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE project_technologies SET is_primary = @isPrimary, display_order = @displayOrder WHERE project_id = @projectId AND technology_id = @technologyId;";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, relation);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) return null;
        return await GetByIdsAsync(connection, relation.ProjectId, relation.TechnologyId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(uint projectId, uint technologyId, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand("DELETE FROM project_technologies WHERE project_id = @projectId AND technology_id = @technologyId;", connection);
        command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = projectId;
        command.Parameters.Add("@technologyId", MySqlDbType.UInt32).Value = technologyId;
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private async Task<IReadOnlyList<ProjectTechnology>> GetManyAsync(string sql, uint? projectId, uint? technologyId, CancellationToken cancellationToken)
    {
        var relations = new List<ProjectTechnology>();
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        if (projectId.HasValue) command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = projectId.Value;
        if (technologyId.HasValue) command.Parameters.Add("@technologyId", MySqlDbType.UInt32).Value = technologyId.Value;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) relations.Add(Map(reader));
        return relations;
    }

    private static async Task<ProjectTechnology?> GetByIdsAsync(MySqlConnection connection, uint projectId, uint technologyId, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand("SELECT " + Columns + FromClause + "WHERE pt.project_id = @projectId AND pt.technology_id = @technologyId LIMIT 1;", connection);
        command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = projectId;
        command.Parameters.Add("@technologyId", MySqlDbType.UInt32).Value = technologyId;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private static void AddParameters(MySqlCommand command, ProjectTechnology relation)
    {
        command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = relation.ProjectId;
        command.Parameters.Add("@technologyId", MySqlDbType.UInt32).Value = relation.TechnologyId;
        command.Parameters.Add("@isPrimary", MySqlDbType.Bool).Value = relation.IsPrimary;
        command.Parameters.Add("@displayOrder", MySqlDbType.Int32).Value = relation.DisplayOrder;
    }

    private string GetConnectionString() => configuration?.GetConnectionString("PortfolioDatabase") is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException("The PortfolioDatabase connection string is not configured.");

    private static ProjectTechnology Map(MySqlDataReader reader) => new()
    {
        ProjectId = reader.GetUInt32("project_id"),
        ProjectTitle = reader.GetString("project_title"),
        ProjectSlug = reader.GetString("project_slug"),
        TechnologyId = reader.GetUInt32("technology_id"),
        TechnologyName = reader.GetString("technology_name"),
        TechnologySlug = reader.GetString("technology_slug"),
        CategoryId = reader.GetUInt32("category_id"),
        CategoryName = reader.GetString("category_name"),
        IsPrimary = reader.GetBoolean("is_primary"),
        DisplayOrder = reader.GetInt32("display_order")
    };
}
