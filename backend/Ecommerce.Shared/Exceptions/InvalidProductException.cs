namespace Ecommerce.Shared.Exceptions;

public class InvalidProductException : Exception
{
    public InvalidProductException(string message)
        : base(message)
    {
    }
}
