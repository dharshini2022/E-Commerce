using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Models
{
    public enum ReturnStatus
    {
        Requested = 1,
        Approved = 2,
        PartiallyApproved = 3,
        Picked = 4,
        Rejected = 5
    }

    public enum ReturnItemStatus
    {
        Requested = 1,
        Approved = 2,
        Rejected = 3
    }

    public enum ReturnItemRefundStatus
    {
        Pending = 1,
        Refunded = 2,
        Failed = 3
    }
    public class Return
    {
        public int Id { get; set; }
        public string? ReturnNumber { get; set; }
        public int OrderId { get; set; }
        public int? ShipmentId { get; set; }
        public int PaymentId { get; set; }
        public string? Reason { get; set; }          
        public ReturnStatus Status { get; set; }            
        public decimal TotalRefundAmount { get; set; }
        public bool IsRefunded { get; set; } = false;
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        
        public Order Order { get; set; } = null!;
        public ICollection<ReturnItem> Items { get; set; } = [];
        public Shipment Shipment { get; set; } = null!;
        public Payment Payment { get; set; } = null!;
    }

    public class ReturnItem
    {
        public int Id { get; set; }
        public int ReturnId { get; set; }
        public ReturnItemStatus Status { get; set; } = ReturnItemStatus.Requested;
        public ReturnItemRefundStatus? RefundStatus { get; set; }
        public int OrderItemId { get; set; }               
        public string Reason { get; set; }  =  null!;         
        public int Quantity { get; set; }   
        public decimal UnitPrice { get; set; }                
        public decimal RefundAmount { get; set; }                   
        
        // Relations
        public Return Return { get; set; } = null!;
        public OrderItem OrderItem { get; set; } = null!;

        
    }
}