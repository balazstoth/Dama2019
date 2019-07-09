using Dama.Data.Models;
using System.Collections.Generic;

namespace Dama.Web.Models.ViewModels.Activity.Display
{
    public class DeadlineActivityViewModel : BaseActivityViewModel
    {
        public string Start { get; set; }

        public string End { get; set; }

        public string MileStones { get; set; }

        public List<DeadlineActivity> DeadlineActivityCollection { get; set; }
    }
}