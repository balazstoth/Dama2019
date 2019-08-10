using System;
using System.ComponentModel.DataAnnotations;

namespace Dama.Web.Models.ViewModels
{
    public class RequestTimeViewModel
    {
        [Display(Name = "Start time")]
        [Required]
        public DateTime StartTime { get; set; }

        public int ActivityId { get; set; }

        public RequestTimeViewModel()
        {
        }

        public RequestTimeViewModel(int activityId)
        {
            ActivityId = activityId;
        }
    }
}