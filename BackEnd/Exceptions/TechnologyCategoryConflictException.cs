namespace BackEnd.Exceptions;

public sealed class TechnologyCategoryConflictException(Exception innerException)
    : Exception("A technology category already uses this name or slug.", innerException);
