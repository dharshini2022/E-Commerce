namespace Ecommerce.Shared.Exceptions
{
    public class UniquenessViolationException : Exception
    {
        public UniquenessViolationException(string message) : base(message)
        {
        }
    }
}