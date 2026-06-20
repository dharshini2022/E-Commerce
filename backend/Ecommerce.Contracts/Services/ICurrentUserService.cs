namespace Ecommerce.Contracts.Services
{
    public interface ICurrentUserService
    {
        int    UserId   { get; }
        string Email    { get; }
        string Role     { get; }
        string FullName { get; }
        bool   IsAuthenticated { get; }
    }
}