namespace Ecommerce.Models.DTOs
{
    public class WishListResponse
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public int TotalItems {get; set;}
        public List<WishListItemResponse> Items {get; set;} = new();
    }
}