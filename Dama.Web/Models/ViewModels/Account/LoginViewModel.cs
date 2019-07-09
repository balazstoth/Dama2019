using System.ComponentModel.DataAnnotations;

namespace Dama.Web.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Keep me logged in")]
        public bool RememberMe { get; set; }
    }
}