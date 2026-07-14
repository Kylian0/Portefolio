namespace FrontEnd.Services;

public sealed class ProjectApiException(string message, IReadOnlyList<string>? errors = null) : Exception(message)
{
    public IReadOnlyList<string> Errors { get; } = errors ?? [message];
}
