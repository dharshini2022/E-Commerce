using Ecommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.DTOs
{


    

    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int VendorId { get; set; }
        public string VendorStoreName { get; set; } = null!;
    }


    public class ReturnRequest
    {
        [Required(ErrorMessage = "OrderId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "OrderId must be a positive integer.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = null!;

        [Required(ErrorMessage = "Items are required.")]
        [MinLength(1, ErrorMessage = "At least one return item must be requested.")]
        public ICollection<ReturnItemRequest> Items { get; set; } = new List<ReturnItemRequest>();
    }

    public class ReturnItemRequest
    {
        [Required(ErrorMessage = "OrderItemId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "OrderItemId must be a positive integer.")]
        public int OrderItemId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = null!;
    }

    public class ReturnSummaryDTO
    {
        public int Id { get; set; }
        public string? ReturnNumber { get; set; }
        public int OrderId { get; set; }
        public string? Reason { get; set; }
        public ReturnStatus Status { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public bool IsRefunded { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ICollection<ReturnItemDTO> Items { get; set; } = new List<ReturnItemDTO>();
    }

    public class ReturnItemDTO
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RefundAmount { get; set; }
        public ReturnItemStatus Status { get; set; }
        public ReturnItemRefundStatus? RefundStatus { get; set; }
    }

    // ─── Admin / Vendor DTOs ──────────────────────────────────────────────────────

    public class AdminRevenueDTO
    {
        public decimal TotalRevenue { get; set; }
        public decimal PlatformCommissionsFromOrders { get; set; }
        public decimal PlatformCommissionsFromSettlements { get; set; }
    }

    public class VendorSettlementDTO
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string VendorStoreName { get; set; } = null!;
        public int OrderId { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal PlatformCommissionAmount { get; set; }
        public decimal NetPayoutAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? SettledAt { get; set; }
    }
}
