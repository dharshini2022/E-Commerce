using System;
using System.Collections.Generic;

namespace Ecommerce.Models.DTOs
{
    public class StatCardDTO
    {
        public decimal Value { get; set; }
        public decimal ChangePercent { get; set; }
        public string ChangeDirection { get; set; } = "up"; // "up" | "down"
    }
}