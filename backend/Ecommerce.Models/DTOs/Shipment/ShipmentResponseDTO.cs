using Ecommerce.Models;
using System;
using System.Collections.Generic;

namespace Ecommerce.Models.DTOs
{
    public class ShipmentResponseDTO
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = null!;
        public int UserAddressId { get; set; }
        public DateTime EstimatedFullfillement { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? FulfilledAt { get; set; }
        public ShipmentStatus Status { get; set; }
        public decimal ShippingFee { get; set; }
        public ICollection<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();
    }
}
