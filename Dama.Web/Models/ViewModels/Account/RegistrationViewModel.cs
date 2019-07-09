using System.ComponentModel.DataAnnotations;

namespace Dama.Web.Models.ViewModels.Account
{
    public class RegistrationViewModel
    {
        private const int minPasswordLength = 6;
        private const int maxPasswordLength = 40;

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [MinLength(minPasswordLength, ErrorMessage = "Minimum lenght: {0}")]
        [MaxLength(maxPasswordLength, ErrorMessage = "Maximum lenght: {0}")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}