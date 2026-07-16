using MySqlConnector;

namespace BackEnd.Models;

public sealed class ContactMessage(IConfiguration configuration)
{
    private string ConnectionString => configuration.GetConnectionString("PortfolioDatabase") ?? throw new InvalidOperationException("PortfolioDatabase is not configured.");
    public Guid Id { get; init; } public string SenderName { get; init; } = ""; public string SenderEmail { get; init; } = ""; public string Subject { get; init; } = ""; public string Content { get; init; } = ""; public DateTime ReceivedAt { get; init; } public bool IsRead { get; init; }

    public async Task EnsureTableAsync(CancellationToken cancellationToken = default)
    {
        const string sql="CREATE TABLE IF NOT EXISTS contact_messages (id CHAR(36) PRIMARY KEY,sender_name VARCHAR(80) NOT NULL,sender_email VARCHAR(254) NOT NULL,subject VARCHAR(120) NOT NULL,content VARCHAR(2000) NOT NULL,received_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,is_read BOOLEAN NOT NULL DEFAULT FALSE);";
        await using var connection=new MySqlConnection(ConnectionString);await connection.OpenAsync(cancellationToken);await using var command=new MySqlCommand(sql,connection);await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<ContactMessage>> GetAllAsync(CancellationToken cancellationToken=default){var items=new List<ContactMessage>();await using var connection=new MySqlConnection(ConnectionString);await connection.OpenAsync(cancellationToken);await using var command=new MySqlCommand("SELECT id,sender_name,sender_email,subject,content,received_at,is_read FROM contact_messages ORDER BY received_at DESC",connection);await using var reader=await command.ExecuteReaderAsync(cancellationToken);while(await reader.ReadAsync(cancellationToken))items.Add(Map(reader));return items;}
    public async Task<ContactMessage> CreateAsync(ContactMessage value,CancellationToken cancellationToken=default){var id=Guid.NewGuid();await using var connection=new MySqlConnection(ConnectionString);await connection.OpenAsync(cancellationToken);await using var command=new MySqlCommand("INSERT INTO contact_messages(id,sender_name,sender_email,subject,content) VALUES(@id,@name,@email,@subject,@content)",connection);command.Parameters.AddWithValue("@id",id);command.Parameters.AddWithValue("@name",value.SenderName);command.Parameters.AddWithValue("@email",value.SenderEmail);command.Parameters.AddWithValue("@subject",value.Subject);command.Parameters.AddWithValue("@content",value.Content);await command.ExecuteNonQueryAsync(cancellationToken);return new ContactMessage(configuration){Id=id,SenderName=value.SenderName,SenderEmail=value.SenderEmail,Subject=value.Subject,Content=value.Content,ReceivedAt=DateTime.UtcNow};}
    public async Task<bool> SetReadAsync(Guid id,bool isRead,CancellationToken cancellationToken=default){await using var connection=new MySqlConnection(ConnectionString);await connection.OpenAsync(cancellationToken);await using var command=new MySqlCommand("UPDATE contact_messages SET is_read=@read WHERE id=@id",connection);command.Parameters.AddWithValue("@read",isRead);command.Parameters.AddWithValue("@id",id);return await command.ExecuteNonQueryAsync(cancellationToken)>0;}
    public async Task<bool> DeleteAsync(Guid id,CancellationToken cancellationToken=default){await using var connection=new MySqlConnection(ConnectionString);await connection.OpenAsync(cancellationToken);await using var command=new MySqlCommand("DELETE FROM contact_messages WHERE id=@id",connection);command.Parameters.AddWithValue("@id",id);return await command.ExecuteNonQueryAsync(cancellationToken)>0;}
    private ContactMessage Map(MySqlDataReader r)=>new(configuration){Id=r.GetGuid("id"),SenderName=r.GetString("sender_name"),SenderEmail=r.GetString("sender_email"),Subject=r.GetString("subject"),Content=r.GetString("content"),ReceivedAt=r.GetDateTime("received_at"),IsRead=r.GetBoolean("is_read")};
}
