using Dama.Data.Models;
using System.Collections.Generic;

namespace Dama.Web.Models.ViewModels.Activity.Display
{
    public class UnfixedActivityViewModel : BaseActivityViewModel
    {
        public string Priority { get; set; }
        public string Start { get; set; }
        public string Timespan { get; set; }
        public string Repeat { get; set; }

        public List<UnfixedActivity> UnfixedActivityCollection { get; set; }
    }
}