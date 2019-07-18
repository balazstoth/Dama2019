using System.Collections.Generic;
using System.Web.Mvc;
using Dama.Data.Enums;

namespace Dama.Web.Models
{
    public class EditDetails
    {
        public int ActivityId { get; set; }

        public List<SelectListItem> Colors { get; set; }

        public List<SelectListItem> Labels { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public List<SelectListItem> RepeatTypes { get; set; }

        public bool IsOptional { get; set; }

        public ActivityType? ActivityType { get; set; }

        public bool CalledFromEditor { get;  set; }
    }
}