using Dama.Web.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Dama.Web.Models.ViewModels.Activity.Manage
{
    public class UndefinedActivityManageViewModel : ActivityManageBaseViewModel
    {
        public bool CalledFromEditor { get; set; }

        public IEnumerable<string> Labels { get; set; }

        public string Category { get; set; }

        [Required]
        [MinTime]
        public int MinimumTime { get; set; }

        [Required]
        [MaxTime("MinimumTime")]
        public int MaximumTime { get; set; }

        public IEnumerable<SelectListItem> ColorSourceCollection { get; set; }
        public IEnumerable<SelectListItem> LabelSourceCollection { get; set; }
        public IEnumerable<SelectListItem> CategorySourceCollection { get; set; }
    }
}