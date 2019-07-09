using Dama.Web.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Dama.Web.Models.ViewModels.Activity.Manage
{
    public class UnfixedActivityManageViewModel : ActivityManageBaseViewModel
    {
        public UnfixedActivityManageViewModel()
        {
            IsOptional = null;
            EnableRepeatChange = false;
        }

        public bool? IsOptional { get; set; }

        public bool EnableRepeatChange { get; set; }

        public IEnumerable<string> Labels { get; set; }

        public string Category { get; set; }

        [Priority(0, 0, false)]
        public int Priority { get; set; }

        [Required]
        [Duration]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH}", ApplyFormatInEditMode = true)]
        public TimeSpan Timespan { get; set; }

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