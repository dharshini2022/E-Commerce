using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Models
{
    public class UserAddress
    {
        public int Id {get; set; }
        public int UserId { get; set; }
        public string RecipientName { get; set; } = String.Empty;
        public string Phone         { get; set; } = string.Empty;  
        public string  Line1        { get; set; } = string.Empty;
        public string? Line2        { get; set; }
        public string? Landmark     { get; set; }          
        public string  City         { get; set; } = string.Empty;
        public string  State        { get; set; } = string.Empty;
        public string  PostalCode   { get; set; } = string.Empty;   
        public string  Country  { get; set; } = "India";   
        public string?  Label    { get; set; }          

        //Relation
        public User User   { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}