using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Models
{
    public class ReviewImage
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int ImageOrder { get; set; } // To maintain the order of images for a review
        
        //Relation
        public Review Review { get; set; } = null!;
    }
}