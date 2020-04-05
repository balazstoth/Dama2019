using System.ComponentModel.DataAnnotations;

namespace Dama.Web.Models.ViewModels.Activity.Manage
{
    public abstract class ActivityManageBaseViewModel
    {
        private const int minNameLength = 3;
        private const int maxNameLength = 30;
        private const int maxDescriptionLength = 50;

        public string Id { get; set; }

        [Required]
        [MinLength(minNameLength, ErrorMessage = "Minimum name lenght: {1} characters")]
        [MaxLength(maxNameLength, ErrorMessage = "Maximum name lenght: {1} characters")]
        public string Name { get; set; }

        [MaxLength(maxDescriptionLength, ErrorMessage = "Maximum description length: {1} characters")]
        public string Description { get; set; }

        public string Color { get; set; }
    }
}