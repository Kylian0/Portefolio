namespace BackEnd.Exceptions;

public sealed class TechnologyConflictException(Exception innerException)
    : Exception("A technology already uses this name or slug.", innerException);
