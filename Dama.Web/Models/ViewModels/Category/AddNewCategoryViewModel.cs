using Dama.Web.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Dama.Web.Models.ViewModels.Category
{
    public class AddNewCategoryViewModel
    {
        private const int minNameLength = 3;
        private const int maxNameLength = 30;
        private const int maxDescriptionLength = 50;

        [Required]
        [MinLength(minNameLength, ErrorMessage = "Minimum name length: {0} characters")]
        [MaxLength(maxNameLength, ErrorMessage = "Maximum name length: {0} characters")]
        public string Name { get; set; }

        [MaxLength(maxDescriptionLength, ErrorMessage = "Maximum description length: {0} characters")]
        public string Description { get; set; }

        public IEnumerable<SelectListItem> Color { get; set; }

        [Required]
        public string SelectedColor { get; set; }

        [Required]
        [Priority(0,0, true)]
        public int Priority { get; set; }
    }
}