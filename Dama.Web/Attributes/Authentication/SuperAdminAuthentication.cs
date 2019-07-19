using Dama.Data.Enums;
using Dama.Organizer.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Error = Dama.Organizer.Resources.Error;

namespace Dama.Web.Attributes
{
    /// <summary>
    /// Check if the user has superAdmin or any other passed role
    /// </summary>
    public class SuperAdminAuthentication : AuthorizeAttribute
    {
        private List<string> _acceptedRoles;
        private ActionNames _redirectToAction;
        private readonly string _authenticationError;

        public SuperAdminAuthentication(ActionNames redirectToAction = ActionNames.AccessDenied, params string[] roles)
        {
            _redirectToAction = redirectToAction;
            _acceptedRoles = roles.ToList();
            _acceptedRoles.Add(UserRole.SuperAdmin.ToString());
            _authenticationError = Error.AuthenticationError;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            var user = httpContext.User;
            return _acceptedRoles.Where(r => user.IsInRole(r)).Any();
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            /*In case a simpleuser is logged in, tried to reach the List Users site, 
            and then after a log out, a superadmin reached the listUser site, he got an invalid errormessage
            because the filtercontext.Controller.TempData still contained value.*/

            filterContext.Controller.TempData.Remove(_redirectToAction.ToString());
            bool authorized = AuthorizeCore(filterContext.HttpContext);

            if (!authorized)
                HandleUnauthorizedRequest(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var redirect = _redirectToAction.ToString();
            filterContext.Controller.TempData[redirect] = _authenticationError;
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", redirect }, { "controller", "Account" } });
        }
    }
}