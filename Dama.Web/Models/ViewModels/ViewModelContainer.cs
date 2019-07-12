using Dama.Web.Models.ViewModels.Activity.Display;

namespace Dama.Web.Models.ViewModels
{
    public class ViewModelContainer
    {
        public FixedActivityViewModel FixedActivityViewModel { get; set; }

        public UnfixedActivityViewModel UnfixedActivityViewModel { get; set; }

        public UndefinedActivityViewModel UndefinedActivityViewModel { get; set; }

        public DeadlineActivityViewModel DeadlineActivityViewModel { get; set; }
    }
}