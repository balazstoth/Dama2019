using Dama.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Dama.Web.Models.ViewModels.Editor
{
    public class CalendarEditorViewModel
    {
        [Required]
        [Display(Name = "Select day")]
        public DateTime SelectedDate { get; set; }

        //Contains the selected items (by user)
        public IEnumerable<string> SelectedActivityCollection { get; set; }

        //For dropdownlist
        [Display(Name = "Activity type")]
        public string SelectedType { get; set; }

        public List<SelectListItem> ActivityTypes
        {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem { Text = ActivityType.FixedActivity.ToString(), Value = ActivityType.FixedActivity.ToString() },
                    new SelectListItem { Text = ActivityType.UnfixedActivity.ToString(), Value = ActivityType.UnfixedActivity.ToString() },
                    new SelectListItem { Text = ActivityType.UndefinedActivity.ToString(), Value = ActivityType.UndefinedActivity.ToString() },
                    new SelectListItem { Text = ActivityType.DeadlineActivity.ToString(), Value = ActivityType.DeadlineActivity.ToString() },
                };
            }
            set
            {
                ActivityTypes = value;
            }
        }

        //Different lists for different activityTypes
        [Display(Name = "Activities")]
        public List<SelectListItem> ActivityCollectionForActivityTypes { get; set; }

        //Filters
        [Display(Name = "Categories")]
        public string SelectedCategory { get; set; }

        public List<SelectListItem> CategoryFilterSourceCollection { get; set; }

        [Display(Name = "Labels")]
        public string SelectedLabel { get; set; }

        public List<SelectListItem> LabelFilterSourceCollection { get; set; }

        [Display(Name = "Priority")]
        public string SelectedPriorityFilter { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        public bool IsAscendant { get; set; }

        public List<SelectListItem> ActivitiesSelectedToReplaceByUser { get; set; }

        [Display(Name = "Fix activities:")]
        public List<SelectListItem> MandatoryActivitiesSelectedByUser { get; set; }

        [Display(Name = "Optional activities:")]
        public List<SelectListItem> OptionalActivitiesSelectedByUser { get; set; }

        public CalendarEditorViewModel()
        {
            SelectedActivityCollection = new List<string>();
            ActivityCollectionForActivityTypes = new List<SelectListItem>();
            CategoryFilterSourceCollection = new List<SelectListItem>();
            LabelFilterSourceCollection = new List<SelectListItem>();
            ActivitiesSelectedToReplaceByUser = new List<SelectListItem>();
            MandatoryActivitiesSelectedByUser = new List<SelectListItem>();
            OptionalActivitiesSelectedByUser = new List<SelectListItem>();
        }
    }
}