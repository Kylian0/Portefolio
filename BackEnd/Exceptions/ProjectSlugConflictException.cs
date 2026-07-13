namespace BackEnd.Exceptions;

public sealed class ProjectSlugConflictException(string slug, Exception? innerException = null)
    : Exception($"A project with slug '{slug}' already exists.", innerException);
