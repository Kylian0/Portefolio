namespace BackEnd.Exceptions;

public sealed class MediaStoredFilenameConflictException(string storedFilename, Exception innerException)
    : Exception($"A media item already uses the stored filename '{storedFilename}'.", innerException);
