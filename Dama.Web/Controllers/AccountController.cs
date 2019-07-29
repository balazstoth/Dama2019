using Dama.Data.Enums;
using Dama.Data.Models;
using Dama.Data.Sql.Interfaces;
using Dama.Data.Sql.SQL;
using Dama.Organizer.Extensions;
using Dama.Web.Attributes;
using Dama.Web.Exceptions;
using Dama.Web.Models.ViewModels.Account;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AccountMessage = Dama.Organizer.Enums.AccountMessage;
using ActionNames = Dama.Organizer.Enums.ActionNames;
using ControllerNames = Dama.Organizer.Enums.ControllerNames;
using ErrorMessage = Dama.Organizer.Resources.Error;
using SuccessMessage = Dama.Organizer.Resources.Success;

namespace Dama.Web.Controllers
{
    [Authorize]
    [DisableUser]
    public class AccountController : BaseController
    {
        private readonly string _superAdmin;
        private readonly string _executeError;
        private readonly string _invalidIdError;
        private readonly string _accessDeniedError;
        private readonly string _redirectToListUsers;
        private readonly string _defaultAction;
        private readonly string _defaultController;
        private readonly IUnitOfWork _unitOfWork;

        public UserManager<User> UserManager { get; private set; }

        public UserStore<User> UserStore { get; private set; }

        public IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        public AccountController(IUnitOfWork unitOfWork)
        {
            _defaultAction = ActionNames.Index.ToString();
            _defaultController = ControllerNames.Home.ToString();
            _accessDeniedError = AccountMessage.AccessDenied.ToString();
            _executeError = AccountMessage.ExecuteError.ToString();
            _invalidIdError = AccountMessage.InvalidId.ToString();
            _redirectToListUsers = ActionNames.ListUsersAsync.ToString();
            _superAdmin = "superAdmin";

            _unitOfWork = unitOfWork;
            UserStore = new UserStore<User>(new DamaContext());
            UserManager = new UserManager<User>(UserStore);
        }

        [DisableUser]
        [SuperAdminAuthentication(roles: "Admin")]
        public async Task<ActionResult> ListUsersAsync()
        {
            ViewBag.ExecuteError = TempData[_executeError];
            ViewBag.InvalidId = TempData[_invalidIdError];
            ViewBag.AccessDenied = TempData[_accessDeniedError];

            IEnumerable<User> users = _unitOfWork.UserRepository.Get();

            foreach (var user in users)
                user.RolesCollection = await GetUserRolesAsync(user);

            return View(users);
        }

        [DisableUser]
        public async Task<List<UserRole>> GetUserRolesAsync(User user)
        {
            var roles = await UserManager.GetRolesAsync(user.Id);
            return roles.ToUserRole().ToList();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(_defaultAction, _defaultController);

            return View();
        }

        private async Task SignInAsync(User user, bool remember)
        {
            AuthenticationManager.SignOut();
            var id = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = remember }, id);
        }

        [DisableUser]
        [SuperAdminAuthentication(roles: "Admin")]
        public async Task<ActionResult> Block(string id)
        {
            User user;

            try
            {
                user = await UserValidation(id, true);
            }
            catch (InvalidIdException)
            {
                return RedirectToAction(_redirectToListUsers);
            }
            catch (ChangeOwnAccountException)
            {
                return RedirectToAction(_redirectToListUsers);
            }

            user.Blocked = true;
            await UserManager.UpdateAsync(user);

            return RedirectToAction(_redirectToListUsers);
        }

        [DisableUser]
        [SuperAdminAuthentication(roles: "Admin")]
        public async Task<ActionResult> Unblock(string id)
        {
            User user;

            try
            {
                user = await UserValidation(id, true);
            }
            catch (InvalidIdException)
            {
                return RedirectToAction(_redirectToListUsers);
            }
            catch (ChangeOwnAccountException)
            {
                return RedirectToAction(_redirectToListUsers);
            }

            user.Blocked = false;
            await UserManager.UpdateAsync(user);

            return RedirectToAction(_redirectToListUsers);
        }

        [DisableUser]
        [SuperAdminAuthentication(roles: "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            User user;

            try
            {
                user = await UserValidation(id, true);
            }
            catch (InvalidIdException)
            {
                return RedirectToAction(_redirectToListUsers);
            }
            catch (ChangeOwnAccountException)
            {
                return RedirectToAction(_redirectToListUsers);
            }

            await UserManager.DeleteAsync(user);

            return RedirectToAction(_redirectToListUsers);
        }

        [SuperAdminAuthentication(redirectToAction: ActionNames.ListUsersAsync)]
        public async Task<ActionResult> SetAdminRight(string id)
        {
            User user;

            try
            {
                user = await UserValidation(id, false);
            }
            catch (InvalidIdException)
            {
                return RedirectToAction(_redirectToListUsers);
            }
            catch (ChangeOwnAccountException)
            {
                return RedirectToAction(_redirectToListUsers);
            }

            await UserManager.AddToRoleAsync(id, UserRole.Admin.ToString());

            return RedirectToAction(_redirectToListUsers);
        }

        [SuperAdminAuthentication(redirectToAction: ActionNames.ListUsersAsync)]
        public async Task<ActionResult> RevokeAdminRight(string id)
        {
            User user;

            try
            {
                user = await UserValidation(id, false);
            }
            catch (InvalidIdException)
            {
                return RedirectToAction(_redirectToListUsers);
            }
            catch (ChangeOwnAccountException)
            {
                return RedirectToAction(_redirectToListUsers);
            }

            await UserManager.RemoveFromRoleAsync(id, UserRole.Admin.ToString());

            return RedirectToAction(_redirectToListUsers);
        }

        private async Task<bool> IsActionEnabledAsync(string tartgetId)
        {
            User currentUser, targetUser;
            var currentUserId = User.Identity.GetUserId();

            currentUser = _unitOfWork.UserRepository.Get(u => u.Id == currentUserId).FirstOrDefault();
            targetUser = _unitOfWork.UserRepository.Get(u => u.Id == tartgetId).FirstOrDefault();

            if (currentUser == null || targetUser == null)
                return false;

            currentUser.RolesCollection = await GetUserRolesAsync(currentUser);
            targetUser.RolesCollection = await GetUserRolesAsync(targetUser);

            var currentUserHighestRole = currentUser.RolesCollection.Select(r => (int)r).ToList().Max();
            var tartgetUserHighestRole = targetUser.RolesCollection.Select(r => (int)r).Max();
            int diff = currentUserHighestRole - tartgetUserHighestRole;

            return diff > 0;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", ErrorMessage.InvalidData);
                }
                else
                {
                    if (user.Blocked)
                    {
                        ModelState.AddModelError("", ErrorMessage.UserIsBlocked);
                    }
                    else
                    {
                        await SignInAsync(user, model.RememberMe);
                        return RedirectToAction(_defaultAction, _defaultController);
                    }
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Register(AccountMessage? message)
        {
            if (message == AccountMessage.UserCreatedSuccessfully)
                ViewBag.SuccessMessage = SuccessMessage.UserRegisteresSuccessfully;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegistrationViewModel model)
        {
            var redirectToAction = ActionNames.Register.ToString();
            var redirectToController = ControllerNames.Account.ToString();

            if (ModelState.IsValid)
            {
                var user = new User() { UserName = model.UserName };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, UserRole.SimpleUser.ToString());
                    await SignInAsync(user, false);
                    return RedirectToAction(redirectToAction, redirectToController, new { Message = AccountMessage.UserCreatedSuccessfully.ToString() });
                }
                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error);
                }
            }

            return View(model);
        }

        [DisableUser]
        public ActionResult Manage(AccountMessage? message)
        {
            switch (message)
            {
                case AccountMessage.PasswordChangedSuccessfully:
                    ViewBag.SuccessMessage = SuccessMessage.PasswordChangedSeccessfully;
                    break;

                case AccountMessage.UserNotFound:
                    ViewBag.UserNotFound = ErrorMessage.UserNotFound;
                    break;

                case AccountMessage.CannotDeleteSuperAdmin:
                    ViewBag.CannotDeleteSuperAdmin = ErrorMessage.SuperAdminCannotBeDeleted;
                    break;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();

                try
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

                    if (result.Succeeded)
                        return RedirectToAction(ActionNames.Manage.ToString(), new { Message = AccountMessage.PasswordChangedSuccessfully.ToString() });
                    else
                        foreach (var error in result.Errors)
                            ModelState.AddModelError("", error);
                }
                catch (System.Exception)
                {
                    ModelState.AddModelError("", ErrorMessage.ClearCache);
                }
            }

            return View(model);
        }

        public async Task<ActionResult> DeleteOwnAccount()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                return RedirectToAction(ActionNames.Manage.ToString(), new { Message = AccountMessage.UserNotFound.ToString() });
            }
            else
            {
                if (user.UserName == _superAdmin)
                    return RedirectToAction(ActionNames.Manage.ToString(), new { Message = AccountMessage.CannotDeleteSuperAdmin.ToString() });

                await UserManager.DeleteAsync(user);
                AuthenticationManager.SignOut();

                return RedirectToAction(_defaultAction, _defaultController);
            }
        }

        [DisableUser]
        public ActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction(_defaultAction, _defaultController);
        }

        //Get method for attribute
        public ActionResult ForceLogoffUser()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction(_defaultAction, _defaultController);
        }

        private async Task<User> UserValidation(string id, bool authorize)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData[_invalidIdError] = ErrorMessage.InvalidUserID;
                throw new InvalidIdException(nameof(id));
            }

            if (id == User.Identity.GetUserId() || (authorize && !(await IsActionEnabledAsync(id))))
            {
                TempData[_executeError] = ErrorMessage.RestrictAccount;
                throw new ChangeOwnAccountException("Cannot change your own account!");
            }

            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData[_invalidIdError] = ErrorMessage.InvalidUserID;
                throw new InvalidIdException(nameof(id));
            }

            return user;
        }
    }
}