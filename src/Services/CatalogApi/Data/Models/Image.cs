using Serilog;
using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Data.Models
{
    public class Image : BaseAuditableEntity
    {
        [Key]
        public long ImageID { get; set; }

        [Required]
        [StringLength(255)]
        public string FileLocation { get; set; }
        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(255)]
        public string ContentType { get; set; }

  
    }
}
