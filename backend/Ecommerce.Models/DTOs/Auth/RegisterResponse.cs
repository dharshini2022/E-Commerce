using Ecommerce.Models;

namespace Ecommerce.Models.DTOs
{
    public class RegisterResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = UserRole.Customer.ToString();
    }
}
