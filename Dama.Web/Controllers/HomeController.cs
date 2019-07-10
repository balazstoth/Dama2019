using Dama.Web.Attributes;
using System.Web.Mvc;

namespace Dama.Web.Controllers
{
    [DisableUser]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}