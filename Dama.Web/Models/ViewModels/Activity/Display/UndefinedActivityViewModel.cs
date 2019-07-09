using Dama.Data.Models;
using System.Collections.Generic;

namespace Dama.Web.Models.ViewModels.Activity.Display
{
    public class UndefinedActivityViewModel : BaseActivityViewModel
    {
        public string MinimumTime { get; set; }

        public string MaximumTime { get; set; }

        public List<UndefinedActivity> UndefinedActivityCollection { get; set; }
    }
}