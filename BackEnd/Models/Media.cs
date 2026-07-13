using BackEnd.Exceptions;
using MySqlConnector;

namespace BackEnd.Models;

public sealed class Media
{
    private const string Columns = "id, project_id, media_type, original_filename, stored_filename, file_path, public_url, mime_type, file_size, alt_text, caption, created_at";
    private readonly IConfiguration? configuration;

    public Media() { }
    public Media(IConfiguration configuration) => this.configuration = configuration;

    public uint Id { get; init; }
    public uint? ProjectId { get; init; }
    public required string MediaType { get; init; }
    public required string OriginalFilename { get; init; }
    public required string StoredFilename { get; init; }
    public required string FilePath { get; init; }
    public string? PublicUrl { get; init; }
    public required string MimeType { get; init; }
    public ulong FileSize { get; init; }
    public string? AltText { get; init; }
    public string? Caption { get; init; }
    public DateTime CreatedAt { get; init; }

    public Task<IReadOnlyList<Media>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetManyAsync($"SELECT {Columns} FROM media ORDER BY created_at DESC;", null, cancellationToken);

    public async Task<Media?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        return await GetByIdAsync(connection, id, cancellationToken);
    }

    public Task<IReadOnlyList<Media>> GetByProjectIdAsync(uint projectId, CancellationToken cancellationToken = default) =>
        GetManyAsync($"SELECT {Columns} FROM media WHERE project_id = @projectId ORDER BY created_at DESC;", projectId, cancellationToken);

    public async Task<Media> CreateAsync(Media media, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO media (project_id, media_type, original_filename, stored_filename, file_path, public_url, mime_type, file_size, alt_text, caption) VALUES (@projectId, @mediaType, @originalFilename, @storedFilename, @filePath, @publicUrl, @mimeType, @fileSize, @altText, @caption);";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new MySqlCommand(sql, connection);
            AddParameters(command, media);
            await command.ExecuteNonQueryAsync(cancellationToken);
            var id = checked((uint)command.LastInsertedId);
            return await GetByIdAsync(connection, id, cancellationToken)
                ?? throw new InvalidOperationException("The media item was created but could not be retrieved.");
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            throw new MediaStoredFilenameConflictException(media.StoredFilename, exception);
        }
    }

    public async Task<Media?> UpdateAsync(uint id, Media media, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE media SET project_id = @projectId, media_type = @mediaType, original_filename = @originalFilename, stored_filename = @storedFilename, file_path = @filePath, public_url = @publicUrl, mime_type = @mimeType, file_size = @fileSize, alt_text = @altText, caption = @caption WHERE id = @id;";
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new MySqlCommand(sql, connection);
            AddParameters(command, media);
            command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
            await command.ExecuteNonQueryAsync(cancellationToken);
            return await GetByIdAsync(connection, id, cancellationToken);
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            throw new MediaStoredFilenameConflictException(media.StoredFilename, exception);
        }
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand("DELETE FROM media WHERE id = @id;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private async Task<IReadOnlyList<Media>> GetManyAsync(string sql, uint? projectId, CancellationToken cancellationToken)
    {
        var items = new List<Media>();
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        if (projectId.HasValue) command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = projectId.Value;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) items.Add(Map(reader));
        return items;
    }

    private static async Task<Media?> GetByIdAsync(MySqlConnection connection, uint id, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand($"SELECT {Columns} FROM media WHERE id = @id LIMIT 1;", connection);
        command.Parameters.Add("@id", MySqlDbType.UInt32).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private static void AddParameters(MySqlCommand command, Media media)
    {
        command.Parameters.Add("@projectId", MySqlDbType.UInt32).Value = media.ProjectId is null ? DBNull.Value : media.ProjectId.Value;
        command.Parameters.Add("@mediaType", MySqlDbType.VarChar).Value = media.MediaType;
        command.Parameters.Add("@originalFilename", MySqlDbType.VarChar).Value = media.OriginalFilename;
        command.Parameters.Add("@storedFilename", MySqlDbType.VarChar).Value = media.StoredFilename;
        command.Parameters.Add("@filePath", MySqlDbType.VarChar).Value = media.FilePath;
        command.Parameters.Add("@publicUrl", MySqlDbType.VarChar).Value = media.PublicUrl is null ? DBNull.Value : media.PublicUrl;
        command.Parameters.Add("@mimeType", MySqlDbType.VarChar).Value = media.MimeType;
        command.Parameters.Add("@fileSize", MySqlDbType.UInt64).Value = media.FileSize;
        command.Parameters.Add("@altText", MySqlDbType.VarChar).Value = media.AltText is null ? DBNull.Value : media.AltText;
        command.Parameters.Add("@caption", MySqlDbType.VarChar).Value = media.Caption is null ? DBNull.Value : media.Caption;
    }

    private string GetConnectionString() => configuration?.GetConnectionString("PortfolioDatabase") is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException("The PortfolioDatabase connection string is not configured.");

    private static Media Map(MySqlDataReader reader) => new()
    {
        Id = reader.GetUInt32("id"),
        ProjectId = reader.IsDBNull(reader.GetOrdinal("project_id")) ? null : reader.GetUInt32("project_id"),
        MediaType = reader.GetString("media_type"),
        OriginalFilename = reader.GetString("original_filename"),
        StoredFilename = reader.GetString("stored_filename"),
        FilePath = reader.GetString("file_path"),
        PublicUrl = reader.IsDBNull(reader.GetOrdinal("public_url")) ? null : reader.GetString("public_url"),
        MimeType = reader.GetString("mime_type"),
        FileSize = reader.GetUInt64("file_size"),
        AltText = reader.IsDBNull(reader.GetOrdinal("alt_text")) ? null : reader.GetString("alt_text"),
        Caption = reader.IsDBNull(reader.GetOrdinal("caption")) ? null : reader.GetString("caption"),
        CreatedAt = reader.GetDateTime("created_at")
    };
}
