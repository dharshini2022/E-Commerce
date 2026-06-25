using System;
using System.Collections.Generic;

namespace Ecommerce.Models.DTOs
{
    public class RevenueBreakdownDTO
    {
        public ICollection<RevenuePointDTO> Monthly { get; set; } = [];
    }
}