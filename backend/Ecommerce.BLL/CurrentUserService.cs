using System;
using System.Security.Claims; 
using Microsoft.AspNetCore.Http;
using Ecommerce.Contracts.Services;
namespace Ecommerce.BLL
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal currentUser
        {
            get{
                var context = _httpContextAccessor.HttpContext;
                if (context == null)
                    throw new InvalidOperationException("No HttpContext found");
                
                return context.User;
            }
        }

        public int UserId => int.Parse( currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value  ?? throw new UnauthorizedAccessException("User is not authenticated"));
        public string Email    => currentUser.FindFirstValue(ClaimTypes.Email)    ?? string.Empty;
        public string Role     => currentUser.FindFirstValue(ClaimTypes.Role)     ?? string.Empty;
        public string FullName => currentUser.FindFirstValue("fullName")          ?? string.Empty;
        public bool IsAuthenticated => currentUser.Identity?.IsAuthenticated ?? false;
    }
}