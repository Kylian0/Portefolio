namespace BackEnd.Exceptions;

public sealed class TechnologyCategoryInUseException(Exception innerException)
    : Exception("The technology category is still used by one or more technologies.", innerException);
