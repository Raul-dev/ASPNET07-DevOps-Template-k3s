using System.ComponentModel.DataAnnotations;

namespace ShopManager.Models
{
    public class LoginViewModel
    {

        [Required]
        [DataType(DataType.Text)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
