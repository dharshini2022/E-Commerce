namespace Ecommerce.Shared.Exceptions
{
    public class InvalidEmailCredsException : Exception
    {
        public InvalidEmailCredsException(string message) : base(message) { }
    }

}