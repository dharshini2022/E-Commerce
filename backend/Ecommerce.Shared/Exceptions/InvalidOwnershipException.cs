namespace Ecommerce.Shared.Exceptions
{
    public class InvalidOwnershipException : Exception
    {
        public InvalidOwnershipException(string message) : base(message)
        {
        }
    }
}
