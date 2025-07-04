namespace Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() : base("Not found")
    {
    }
}