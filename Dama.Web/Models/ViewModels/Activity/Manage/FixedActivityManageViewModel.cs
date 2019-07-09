using Dama.Web.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Dama.Web.Models.ViewModels.Activity.Manage
{
    public class FixedActivityManageViewModel : ActivityManageBaseViewModel
    {
        public FixedActivityManageViewModel()
        {
            EnableRepeatChange = false;
            IsOptional = null;
        }

        public bool? IsOptional { get; set; }

        public bool EnableRepeatChange { get; set; }

        public IEnumerable<string> Labels { get; set; }

        public string Category { get; set; }

        [Priority(0,0, false)]
        public int Priority { get; set; }

        [Required]
        [Display(Name = "Start time")]
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End time")]
        [DataType(DataType.Time)]
        [End("StartTime")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Repeat type")]
        public string RepeatType { get; set; }

        [Display(Name = "Repeat end")]
        [DataType(DataType.Date)]
        public DateTime RepeatEndDate { get; set; }

        public IEnumerable<SelectListItem> ColorSourceCollection { get; set; }
        public IEnumerable<SelectListItem> LabelSourceCollection { get; set; }
        public IEnumerable<SelectListItem> CategorySourceCollection { get; set; }
        public IEnumerable<SelectListItem> RepeatTypeSourceCollection { get; set; }
    }
}