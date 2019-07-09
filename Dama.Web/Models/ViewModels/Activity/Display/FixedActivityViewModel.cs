using Dama.Data.Models;
using System.Collections.Generic;

namespace Dama.Web.Models.ViewModels.Activity.Display
{
    public class FixedActivityViewModel : BaseActivityViewModel
    {
        public string Priority { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Repeat { get; set; }

        public List<FixedActivity> FixedActivityCollection { get; set; }
    }
}