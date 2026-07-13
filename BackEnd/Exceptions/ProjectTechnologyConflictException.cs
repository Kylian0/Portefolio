namespace BackEnd.Exceptions;

public sealed class ProjectTechnologyConflictException(Exception innerException)
    : Exception("This project is already associated with this technology.", innerException);
