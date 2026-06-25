using System;
using System.Collections.Generic;

namespace Ecommerce.Models.DTOs
{
    public class RevenuePointDTO
    {
        public string Label { get; set; } = null!;
        public decimal Revenue { get; set; }
    }
}