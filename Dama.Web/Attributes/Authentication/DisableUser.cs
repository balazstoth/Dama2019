﻿using Dama.Organizer.Enums;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Dama.Web.Attributes
{
    public class DisableUser : AuthorizeAttribute
    {
        private User _user;
        private RedirectToAction _redirectToResult;
        private ControllerNames _redirectToController;

        public DisableUser(RedirectToAction redirectToAction = RedirectToAction.ForceLogoffUser, ControllerNames redirectToController = ControllerNames.Account)
        {
            _redirectToResult = redirectToAction;
            _redirectToController = redirectToController;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //If the user is not logged in return true because other methods will handle this case
            if (!base.AuthorizeCore(httpContext))
                return true;

            var userName = HttpContext.Current.User.Identity.Name;
            
            using (Database db = new Database())
            {
                _user = db.Users
                          .Where(u => u.UserName == userName)
                          .FirstOrDefault();
            }

            if (_user == null || _user.Blocked)
                return false;

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var redirect = _redirectToResult.ToString();
            var controller = _redirectToController.ToString();
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", redirect }, { "controller", controller } });
        }
    }
}