namespace Ecommerce.Shared.Exceptions
{
    public class InvalidAncestorException : Exception
    {
        public InvalidAncestorException(string message) : base(message)
        {
        }
    }
}