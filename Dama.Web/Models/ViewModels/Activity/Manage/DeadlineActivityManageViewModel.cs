using Dama.Web.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Dama.Web.Models.ViewModels.Activity.Manage
{
    public class DeadlineActivityManageViewModel : ActivityManageBaseViewModel
    {
        public bool CalledFromEditor { get; set; }

        [Required]
        [Display(Name = "Start date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "Start time")]
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End time")]
        [DataType(DataType.Time)]
        [Deadline("StartDate", "StartTime", "EndDate")]
        public DateTime EndTime { get; set; }

        [Milestone("StartDate", "StartTime", "EndDate", "EndTime")]
        public string Milestones { get; set; }

        public IEnumerable<SelectListItem> ColorSourceCollection { get; set; }
    }
}