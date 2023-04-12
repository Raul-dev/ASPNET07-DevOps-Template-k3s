using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Data.Models
{
    public class ProductSubCategory
    {
        [Key]
        public int ProductSubCategoryID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

    }
}
