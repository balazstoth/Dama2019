using System.ComponentModel.DataAnnotations;

namespace Dama.Web.Models.ViewModels.Label
{
    public class AddNewLabelViewModel
    {
        private const int minNameLength = 3;
        private const int maxNameLength = 30;

        [Required]
        [MinLength(minNameLength, ErrorMessage = "Minimum name length: {1} characters")]
        [MaxLength(maxNameLength, ErrorMessage = "Maximum name length: {1} characters")]
        public string Name { get; set; }
    }
}