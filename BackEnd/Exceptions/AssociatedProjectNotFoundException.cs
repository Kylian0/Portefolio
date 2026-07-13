namespace BackEnd.Exceptions;

public sealed class AssociatedProjectNotFoundException(uint projectId) : Exception
{
    public uint ProjectId { get; } = projectId;
}
