using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Dama.Data.Models;
using Dama.Data.Sql.SQL;
using Dama.Organizer.Enums;
using Dama.Web.Controllers;
using Dama.Web.Exceptions;
using Dama.Web.Models.ViewModels.Account;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dama.Web.AccountControllerTest
{
    [TestClass]
    public class AccountControllerTest
    {
        User user1, user2, user3;
        List<User> users;
        UserSpecificIRepository userRepository;
        UnitOfWork unitOfWork;
        AccountController accountController;

        public AccountControllerTest()
        {
            CreateNewUsers();

            users = new List<User>() { user1, user2, user3 };
            userRepository = new UserSpecificIRepository(users);
            unitOfWork = new UnitOfWork(userRepository: userRepository);

            var userStore = new Mock<IUserStore<User>>();
            var userManager = new MockedUserManager(userStore.Object);

            accountController = new AccountController(unitOfWork,userManager);
        }

        private void CreateNewUsers()
        {
            user1 = new User()
            {
                Id = "id1",
                UserName = "UserName1",
                Email = "user1@mail.com",
                PasswordHash = "user1hash",
            };

            user2 = new User()
            {
                Id = "id2",
                UserName = "UserName2",
                Email = "user2@mail.com",
                PasswordHash = "user2hash",
            };

            user3 = new User()
            {
                Id = "id3",
                UserName = "UserName3",
                Email = "user3@mail.com",
                PasswordHash = "user3hash",
            };
        }

        [TestMethod]
        public async Task ListUsersTestAsync()
        {
            var viewResult = await accountController.ListUsersAsync() as ViewResult;
            var users = (viewResult.Model as IEnumerable<User>).ToList();
            
            CollectionAssert.Contains(users, user1);
            CollectionAssert.Contains(users, user1);
            CollectionAssert.Contains(users, user1);
        }

        [TestMethod]
        public void LoginReturnsValidModelTest()
        {
            var userStore = new Mock<IUserStore<User>>();
            userStore.As<IUserPasswordStore<User>>()
                .Setup(u => u.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string s) => new User() { UserName = s, Password = "password" });

            var controller = new AccountController(unitOfWork, new MockedUserManager(userStore.Object));
            var viewModel = new LoginViewModel()
            {
                UserName = "username",
                Password = "password"
            };

            var result = accountController.Login(viewModel).Result;
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            
            var viewResult = result as ViewResult;
            Assert.IsTrue(viewResult.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public async Task Block_RedirectToListUsers_WithInvalidId_WithNullValue_Test()
        {
            string id = null;

            var user = (RedirectToRouteResult)(await accountController.Block(id));
            Assert.AreEqual(typeof(InvalidIdException).Name, user.RouteValues["message"]);
            Assert.AreEqual("ListUsersAsync", user.RouteValues["action"]);
        }

        [TestMethod]
        public async Task Block_RedirectToListUsers_WithInvalidId_WithEmptyStringValue_Test()
        {
            string id = "";

            var user = (RedirectToRouteResult)(await accountController.Block(id));
            Assert.AreEqual(typeof(InvalidIdException).Name, user.RouteValues["message"]);
            Assert.AreEqual("ListUsersAsync", user.RouteValues["action"]);
        }

        [TestMethod]
        public async Task Block_RedirectToListUsers_WithInvalidId_WithOwnId_Test()
        {
            string id = "1";

            var user = (RedirectToRouteResult)(await accountController.Block(id));
            Assert.AreEqual(typeof(ChangeOwnAccountException).Name, user.RouteValues["message"]);
            Assert.AreEqual("ListUsersAsync", user.RouteValues["action"]);
        }

        [TestMethod]
        public async Task Block_RedirectToListUsers_WithValidId_Test()
        {
            string id = "id2";
            accountController.UserId = "id1";

            var user = (RedirectToRouteResult)(await accountController.Block(id));
            Assert.AreEqual("success", user.RouteValues["message"]);
            Assert.AreEqual("ListUsersAsync", user.RouteValues["action"]);
        }

        [TestMethod]
        public void Manage_WithPasswordChangedSuccessfully_Test()
        {
            var message = AccountMessage.PasswordChangedSuccessfully;
            var result = (ViewResult)accountController.Manage(message);

            Assert.IsNotNull(result.ViewBag.SuccessMessage);
        }

        [TestMethod]
        public void Manage_WithUserNotFound_Test()
        {
            var message = AccountMessage.UserNotFound;
            var result = (ViewResult)accountController.Manage(message);

            Assert.IsNotNull(result.ViewBag.UserNotFound);
        }

        [TestMethod]
        public void Manage_WithCannotDeleteSuperAdmin_Test()
        {
            var message = AccountMessage.CannotDeleteSuperAdmin;
            var result = (ViewResult)accountController.Manage(message);

            Assert.IsNotNull(result.ViewBag.CannotDeleteSuperAdmin);
        }

        [TestMethod]
        public void AccessDenied_Test()
        {
            var result = accountController.AccessDenied() as ViewResult;
            Assert.AreEqual("AccessDenied", result.ViewName);
        }
    }
}
