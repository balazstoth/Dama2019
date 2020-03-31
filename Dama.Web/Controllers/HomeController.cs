using Dama.Web.Attributes;
using System.Web.Mvc;

namespace Dama.Web.Controllers
{
    [DisableUser]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult About()
        {
            return View("About");
        }
    }
}