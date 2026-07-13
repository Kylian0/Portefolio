namespace BackEnd.Exceptions;

public sealed class TechnologyInUseException : Exception
{
    public TechnologyInUseException()
        : base("The technology is still used by one or more projects.") { }

    public TechnologyInUseException(Exception innerException)
        : base("The technology is still used by one or more projects.", innerException) { }
}
