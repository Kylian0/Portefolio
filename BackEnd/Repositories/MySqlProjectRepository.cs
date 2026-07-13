using BackEnd.Models;
using BackEnd.Exceptions;
using MySqlConnector;

namespace BackEnd.Repositories;

public sealed class MySqlProjectRepository(IConfiguration configuration) : IProjectRepository
{
    private const string ProjectColumns = """
            id,
            title,
            slug,
            short_description,
            thumbnail_url,
            repository_url,
            demo_url,
            status,
            is_featured,
            display_order,
            started_at,
            completed_at,
            created_at,
            updated_at,
            published_at
        """;

    private const string GetAllProjectsSql =
        "SELECT\n" + ProjectColumns + "\n" +
        "FROM projects\n" +
        "ORDER BY display_order ASC, created_at DESC;";

    private const string GetProjectByIdSql =
        "SELECT\n" + ProjectColumns + "\n" +
        "FROM projects\n" +
        "WHERE id = @id\n" +
        "LIMIT 1;";

    private const string CreateProjectSql = """
        INSERT INTO projects (
            title,
            slug,
            short_description,
            thumbnail_url,
            repository_url,
            demo_url,
            status,
            is_featured,
            display_order,
            started_at,
            completed_at,
            published_at
        ) VALUES (
            @title,
            @slug,
            @shortDescription,
            @thumbnailUrl,
            @repositoryUrl,
            @demoUrl,
            @status,
            @isFeatured,
            @displayOrder,
            @startedAt,
            @completedAt,
            @publishedAt
        );
        """;

    private const string UpdateProjectSql = """
        UPDATE projects
        SET
            title = @title,
            slug = @slug,
            short_description = @shortDescription,
            thumbnail_url = @thumbnailUrl,
            repository_url = @repositoryUrl,
            demo_url = @demoUrl,
            status = @status,
            is_featured = @isFeatured,
            display_order = @displayOrder,
            started_at = @startedAt,
            completed_at = @completedAt,
            published_at = @publishedAt
        WHERE id = @id;
        """;

    private const string DeleteProjectSql = """
        DELETE FROM projects
        WHERE id = @id;
        """;

    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = new List<Project>();

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(GetAllProjectsSql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var ordinals = ProjectColumnOrdinals.Create(reader);

        while (await reader.ReadAsync(cancellationToken))
        {
            projects.Add(MapProject(reader, ordinals));
        }

        return projects;
    }

    public async Task<Project?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(GetProjectByIdSql, connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapProject(reader, ProjectColumnOrdinals.Create(reader));
    }

    public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = new MySqlCommand(CreateProjectSql, connection);
            AddWriteParameters(command, project);
            await command.ExecuteNonQueryAsync(cancellationToken);

            var id = checked((uint)command.LastInsertedId);
            return await GetByIdAsync(connection, id, cancellationToken)
                ?? throw new InvalidOperationException("The project was created but could not be retrieved.");
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            throw new ProjectSlugConflictException(project.Slug, exception);
        }
    }

    public async Task<Project?> UpdateAsync(
        uint id,
        Project project,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = new MySqlCommand(UpdateProjectSql, connection);
            AddWriteParameters(command, project);
            command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
            await command.ExecuteNonQueryAsync(cancellationToken);

            return await GetByIdAsync(connection, id, cancellationToken);
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            throw new ProjectSlugConflictException(project.Slug, exception);
        }
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(DeleteProjectSql, connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static async Task<Project?> GetByIdAsync(
        MySqlConnection connection,
        uint id,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(GetProjectByIdSql, connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? MapProject(reader, ProjectColumnOrdinals.Create(reader))
            : null;
    }

    private static void AddWriteParameters(MySqlCommand command, Project project)
    {
        command.Parameters.Add("@title", MySqlDbType.VarChar).Value = project.Title;
        command.Parameters.Add("@slug", MySqlDbType.VarChar).Value = project.Slug;
        command.Parameters.Add("@shortDescription", MySqlDbType.VarChar).Value = project.ShortDescription;
        command.Parameters.Add("@thumbnailUrl", MySqlDbType.VarChar).Value = DbValue(project.ThumbnailUrl);
        command.Parameters.Add("@repositoryUrl", MySqlDbType.VarChar).Value = DbValue(project.RepositoryUrl);
        command.Parameters.Add("@demoUrl", MySqlDbType.VarChar).Value = DbValue(project.DemoUrl);
        command.Parameters.Add("@status", MySqlDbType.VarChar).Value = project.Status;
        command.Parameters.Add("@isFeatured", MySqlDbType.Bool).Value = project.IsFeatured;
        command.Parameters.Add("@displayOrder", MySqlDbType.Int32).Value = project.DisplayOrder;
        command.Parameters.Add("@startedAt", MySqlDbType.Date).Value = DbValue(project.StartedAt);
        command.Parameters.Add("@completedAt", MySqlDbType.Date).Value = DbValue(project.CompletedAt);
        command.Parameters.Add("@publishedAt", MySqlDbType.DateTime).Value = DbValue(project.PublishedAt);
    }

    private static object DbValue<T>(T? value) => value is null ? DBNull.Value : value;

    private string GetConnectionString()
    {
        var connectionString = configuration.GetConnectionString("PortfolioDatabase");
        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new InvalidOperationException("The PortfolioDatabase connection string is not configured.")
            : connectionString;
    }

    private static Project MapProject(MySqlDataReader reader, ProjectColumnOrdinals columns) => new()
    {
        Id = reader.GetUInt32(columns.Id),
        Title = reader.GetString(columns.Title),
        Slug = reader.GetString(columns.Slug),
        ShortDescription = reader.GetString(columns.ShortDescription),
        ThumbnailUrl = GetNullableString(reader, columns.ThumbnailUrl),
        RepositoryUrl = GetNullableString(reader, columns.RepositoryUrl),
        DemoUrl = GetNullableString(reader, columns.DemoUrl),
        Status = reader.GetString(columns.Status),
        IsFeatured = reader.GetBoolean(columns.IsFeatured),
        DisplayOrder = reader.GetInt32(columns.DisplayOrder),
        StartedAt = GetNullableDateOnly(reader, columns.StartedAt),
        CompletedAt = GetNullableDateOnly(reader, columns.CompletedAt),
        CreatedAt = reader.GetDateTime(columns.CreatedAt),
        UpdatedAt = reader.GetDateTime(columns.UpdatedAt),
        PublishedAt = GetNullableDateTime(reader, columns.PublishedAt)
    };

    private static string? GetNullableString(MySqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    private static DateOnly? GetNullableDateOnly(MySqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : DateOnly.FromDateTime(reader.GetDateTime(ordinal));

    private static DateTime? GetNullableDateTime(MySqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);

    private sealed record ProjectColumnOrdinals(
        int Id,
        int Title,
        int Slug,
        int ShortDescription,
        int ThumbnailUrl,
        int RepositoryUrl,
        int DemoUrl,
        int Status,
        int IsFeatured,
        int DisplayOrder,
        int StartedAt,
        int CompletedAt,
        int CreatedAt,
        int UpdatedAt,
        int PublishedAt)
    {
        public static ProjectColumnOrdinals Create(MySqlDataReader reader) => new(
            reader.GetOrdinal("id"),
            reader.GetOrdinal("title"),
            reader.GetOrdinal("slug"),
            reader.GetOrdinal("short_description"),
            reader.GetOrdinal("thumbnail_url"),
            reader.GetOrdinal("repository_url"),
            reader.GetOrdinal("demo_url"),
            reader.GetOrdinal("status"),
            reader.GetOrdinal("is_featured"),
            reader.GetOrdinal("display_order"),
            reader.GetOrdinal("started_at"),
            reader.GetOrdinal("completed_at"),
            reader.GetOrdinal("created_at"),
            reader.GetOrdinal("updated_at"),
            reader.GetOrdinal("published_at"));
    }
}
