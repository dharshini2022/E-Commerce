namespace Ecommerce.Models;

public class ProductImage
{
    public int Id { get; set; }
    public int VariantId { get; set; } 
    public string ImageUrl { get; set; } = string.Empty;
    public int ImageOrder { get; set; }

    // Relation
    public ProductVariant ProductVariant { get; set; } = null!;
}

/*Why Separate Table for Imagees?
Without a separate table, we would have to store all image URLs in the Product table, which would lead to 
-data redundancy 
- harder to manage and query image-related information (meta data)
- Costlier Updates (To add / delete images, we would need to update the entire product record, which can be inefficient and error-prone) 
*/
