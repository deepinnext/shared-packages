namespace Deepin.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException()
    {

    }
    public EntityNotFoundException(string message) : base(message)
    {

    }
    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {

    }
    public EntityNotFoundException(string name, object key)
        : base($"Entity '{name}' ({key}) was not found.")
    {

    }
}