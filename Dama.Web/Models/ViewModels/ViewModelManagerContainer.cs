using Dama.Web.Models.ViewModels.Activity.Manage;

namespace Dama.Web.Models.ViewModels
{
    public class ViewModelManagerContainer
    {
        public FixedActivityManageViewModel FixedActivityManageViewModel { get; set; }

        public UnfixedActivityManageViewModel UnfixedActivityManageViewModel { get; set; }

        public UndefinedActivityManageViewModel UndefinedActivityManageViewModel { get; set; }

        public DeadlineActivityManageViewModel DeadlineActivityManageViewModel { get; set; }
    }
}