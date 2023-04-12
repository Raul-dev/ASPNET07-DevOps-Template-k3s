using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Data.Models
{
    public class Product : BaseAuditableEntity
    {
        [Key]
        public long ProductID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        
        [MaxLength(25)]
        public string? ProductNumber { get; set; }
        public ProductSubCategory? ProductSubCategory { get; set; }
        
    }
}
