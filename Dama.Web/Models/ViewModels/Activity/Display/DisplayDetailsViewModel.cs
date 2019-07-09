using Dama.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Dama.Web.Models.ViewModels.Activity.Display
{
    public class DisplayDetailsViewModel
    {
        public FixedActivity FixedActivity { get; set; }
        public UnfixedActivity UnfixedActivity { get; set; }
        public UndefinedActivity UndefinedActivity { get; set; }
        public DeadlineActivity DeadlineActivity { get; set; }

        //Properties are necessary to be displayed correctly in view
        [Display(Name = "Minimum time")]
        public string GetShorterMinTime => UndefinedActivity == null ? "0" : UndefinedActivity.MinimumTime.ToString();

        [Display(Name = "Maximum time")]
        public string GetShorterMaxTime => UndefinedActivity == null ? "0" : UndefinedActivity.MaximumTime.ToString();

        public string GetFixedActStartValue => FixedActivity?.Start?.ToShortTimeString();

        public string GetFixedActEndValue => FixedActivity?.End.ToShortTimeString();

    }
}