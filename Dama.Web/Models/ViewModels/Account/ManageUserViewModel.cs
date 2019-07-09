using System.ComponentModel.DataAnnotations;

namespace Dama.Web.Models.ViewModels.Account
{
    public class ManageUserViewModel
    {
        private const int minPasswordLength = 6;
        private const int maxPasswordLength = 40;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(minPasswordLength, ErrorMessage = "Minimum lenght: {0}")]
        [MaxLength(maxPasswordLength, ErrorMessage = "Maximum lenght: {0}")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new and confirmation passwords are different")]
        public string ConfirmPassword { get; set; }
    }
}