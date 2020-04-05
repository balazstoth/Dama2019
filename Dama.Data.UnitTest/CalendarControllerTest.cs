using Dama.Data.Enums;
using Dama.Data.Models;
using Dama.Data.Sql.SQL;
using Dama.Organizer;
using Dama.Organizer.Enums;
using Dama.Web.AccountControllerTest;
using Dama.Web.Controllers;
using Dama.Web.Models.ViewModels;
using Dama.Web.Models.ViewModels.Activity.Display;
using Dama.Web.Models.ViewModels.Activity.Manage;
using Dama.Web.Models.ViewModels.Category;
using Dama.Web.Models.ViewModels.Label;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Dama.Data.UnitTest
{
    [TestClass]
    public class CalendarControllerTest
    {
        FixedActivity fa1, fa2;
        UnfixedActivity ufa1, ufa2;
        UndefinedActivity uda1, uda2;
        DeadlineActivity da1, da2;
        Category cat1, cat2;
        Label lab1, lab2;
        UnitOfWork unitOfWork;
        CalendarController controller;
        GenericIRepository<FixedActivity> fixedIRepository;
        GenericIRepository<UnfixedActivity>unfixedIRepository;
        GenericIRepository<UndefinedActivity> undefinedIRepository;
        GenericIRepository<DeadlineActivity> deadlineIRepository;
        GenericIRepository<Category> categoryIRepository;
        GenericIRepository<Label> labelIRepository;
        GenericIRepository<Milestone> milestoneIRepository;

        public CalendarControllerTest()
        {
            InitializeActivities();

            fixedIRepository = new GenericIRepository<FixedActivity>(new List<FixedActivity>() { fa1, fa2 });
            unfixedIRepository = new GenericIRepository<UnfixedActivity>(new List<UnfixedActivity>() { ufa1, ufa2 });
            undefinedIRepository = new GenericIRepository<UndefinedActivity>(new List<UndefinedActivity>() { uda1, uda2 });
            deadlineIRepository = new GenericIRepository<DeadlineActivity>(new List<DeadlineActivity>() { da1, da2 });
            categoryIRepository = new GenericIRepository<Category>(new List<Category>() { cat1, cat2 });
            labelIRepository = new GenericIRepository<Label>(new List<Label>() { lab1, lab2 });
            milestoneIRepository = new GenericIRepository<Milestone>(new List<Milestone>());

            unitOfWork = new UnitOfWork(fixedActivityRepository: fixedIRepository,
                                        unfixedActivityRepository: unfixedIRepository,
                                        undefinedActivityRepository: undefinedIRepository,
                                        deadlineActivityRepository: deadlineIRepository,
                                        categoryRepository: categoryIRepository,
                                        labelRepository: labelIRepository,
                                        milestoneRepository: milestoneIRepository,
                                        context: new MockContext());

            var userStore = new Mock<IUserStore<User>>();
            var userManager = new MockedUserManager(userStore.Object);

            controller = new CalendarController(unitOfWork, userManager, "1");
        }

        private void InitializeActivities()
        {
            lab1 = new Label("Lab1", "1");
            lab1.Id = 1;

            lab2 = new Label("Lab2", "1");
            lab2.Id = 2;

            cat1 = new Category("Cat1", "", Color.Red, 1, "1");
            cat1.Id = 1;

            cat2 = new Category("Cat2", "", Color.Red, 3, "1");
            cat2.Id = 2;

            fa1 = new FixedActivity("FA1", "desc", Color.Black, CreationType.ManuallyCreated, null, null, "1", 5, DateTime.Today, DateTime.Today.AddHours(2), false);
            fa1.Id = 1;
            
            fa2 = new FixedActivity("FA2", "desc", Color.Gray, CreationType.ManuallyCreated, null, null, "1", 3, DateTime.Today.AddHours(1), DateTime.Today.AddHours(4), false);

            ufa1 = new UnfixedActivity("UFA1", "desc", Color.Orange, CreationType.ManuallyCreated, null, null, "1", 2, new TimeSpan(3, 50, 0), false);
            ufa1.Id = 1;
            ufa1.Start = DateTime.Today;

            ufa2 = new UnfixedActivity("UFA2", "desc", Color.Brown, CreationType.ManuallyCreated, null, null, "1", 2, new TimeSpan(1, 0, 0), false);
            ufa2.Start = DateTime.Today;

            uda1 = new UndefinedActivity("UDA1", "desc", Color.Red, CreationType.ManuallyCreated, null, null, "1",10,40,false);
            uda1.Start = DateTime.Today;
            uda1.Id = 1;

            uda2 = new UndefinedActivity("UDA2", "desc", Color.White, CreationType.ManuallyCreated, null, null, "1", 55, 90, false);
            uda2.Start = DateTime.Today;

            da1 = new DeadlineActivity("DA1", "desc", Color.White, CreationType.ManuallyCreated, null, null, "1", DateTime.Today, DateTime.Today.AddHours(2), null, false);
            da1.Id = 1;

            da2 = new DeadlineActivity("DA2", "desc", Color.Purple, CreationType.ManuallyCreated, null, null, "1", DateTime.Today, DateTime.Today.AddHours(1), null, false);
        }

        [TestMethod]
        public void Index_Test()
        {
            var result = controller.Index() as ViewResult;
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void GetActivitiesToDisplayInCalendar_Test()
        {
            var userId = "1";
            var result = new JsonResult 
            { 
                Data = new ActivityQuery(unitOfWork, userId).GetActivities(), 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet 
            };

            var expectedJsonResult = (result.Data as IEnumerable<Activity>).Select(a => a.Name).ToList();
            var actualJsonResult = (controller.GetActivitiesToDisplayInCalendar().Data as IEnumerable<Activity>).Select(a => a.Name).ToList();

            CollectionAssert.AreEqual(expectedJsonResult, actualJsonResult);
        }

        [TestMethod]
        public void AddNewCategory_Test()
        {
            var colors = Enum.GetNames(typeof(Color));
            var view = (ViewResult)controller.AddNewCategory();
            var result = (view.Model as AddNewCategoryViewModel).Color.Select(c => c.Text).ToArray();

            CollectionAssert.AreEqual(colors, result);
        }

        [TestMethod]
        public void ManageCategories_Test()
        {
            var categories = unitOfWork.CategoryRepository.Get().ToArray();
            var view = (ViewResult)controller.ManageCategories();
            var result = (view.Model as IEnumerable<Category>).ToArray();

            CollectionAssert.AreEqual(categories, result);
        }

        [TestMethod]
        public void DeleteCategory_WithValidId_Test()
        {
            var categoryId = 1;

            var actionResult = controller.DeleteCategory(categoryId.ToString());
            var routeResult = (RedirectToRouteResult)actionResult;

            Assert.IsNotNull(controller.ViewBag.CategoryRemovedSuccessFully);
            Assert.IsNull(controller.ViewBag.CategoryNotFoundError);
            Assert.IsFalse(categoryIRepository.Get(c => c.Id == categoryId).Any());
            Assert.AreEqual(ActionNames.ManageCategories.ToString(), routeResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteCategory_WithInvalidId_Test()
        {
            var categoryId = 0;

            var actionResult = controller.DeleteCategory(categoryId.ToString());
            var routeResult = (RedirectToRouteResult)actionResult;

            Assert.IsNotNull(controller.ViewBag.CategoryNotFoundError);
            Assert.IsNull(controller.ViewBag.CategoryRemovedSuccessFully);
            Assert.IsTrue(categoryIRepository.Get().Any());
            Assert.AreEqual(ActionNames.ManageCategories.ToString(), routeResult.RouteValues["action"]);
        }

        [TestMethod]
        public void AddNewCategory_CategoryNotExists_Test()
        {
            var model = new AddNewCategoryViewModel()
            {
                SelectedColor = Color.Brown.ToString(),
                Name = "NewCategory",
                Description = "Desc1",
                Priority = 2
            };

            var result = (ViewResult)controller.AddNewCategory(model);
            var newItem = categoryIRepository.Get(c => c.Name == "NewCategory").Single();
            
            Assert.AreEqual(model.SelectedColor, newItem.Color.ToString());
            Assert.AreEqual(model.Name, newItem.Name);
            Assert.AreEqual(model.Description, newItem.Description);
            Assert.AreEqual(model.Priority, newItem.Priority);
            Assert.AreEqual("1", newItem.UserId);
            Assert.IsNotNull(controller.ViewBag.CategoryCreatedSuccessFully);
            Assert.AreEqual(result.Model, model);
        }

        [TestMethod]
        public void AddNewCategory_CategoryExist_Test()
        {
            var model = new AddNewCategoryViewModel()
            {
                SelectedColor = Color.Brown.ToString(),
                Name = "Cat1",
                Description = "Desc1",
                Priority = 2
            };

            var result = (ViewResult)controller.AddNewCategory(model);
            Assert.IsNotNull(controller.ViewBag.CategoryAlreadyExists);
            Assert.AreEqual(result.Model, model);
        }

        [TestMethod]
        public void EditCategory_CategoryNotExist()
        {
            var viewModel = new EditCategoryViewModel()
            {
                Id = "1",
                Description = "",
                Name = "ChangedCat",
                Priority = 7,
                SelectedColor = "Green"
            };

            var result = (ViewResult)controller.EditCategory(viewModel);
            var changedCategory = categoryIRepository.GetById(viewModel.Id);

            Assert.AreEqual(viewModel.Name, changedCategory.Name);
            Assert.AreEqual(viewModel.Description, changedCategory.Description);
            Assert.AreEqual(viewModel.Priority, changedCategory.Priority);
            Assert.AreEqual(viewModel.SelectedColor, changedCategory.Color.ToString());
            Assert.IsNotNull(controller.ViewBag.CategoryChangedSuccessfully);
            Assert.AreEqual(result.Model, viewModel);
        }

        [TestMethod]
        public void EditCategory_CategoryExist()
        {
            var viewModel = new EditCategoryViewModel()
            {
                Id = "1",
                Description = "",
                Name = "Cat2",
                Priority = 7,
                SelectedColor = "Green"
            };

            var result = (ViewResult)controller.EditCategory(viewModel);

            Assert.IsNotNull(controller.ViewBag.CategoryAlreadyExists);
            Assert.AreEqual(result.Model, viewModel);
        }

        [TestMethod]
        public void AddNewLabel_Test()
        {
            var result = controller.AddNewLabel() as ViewResult;
            Assert.AreEqual("AddNewLabel", result.ViewName);
        }

        [TestMethod]
        public void ManageLabels_Test()
        {
            var expectedResult = labelIRepository.Get().GroupBy(l => l.Name).Select(l => l.First()).ToList();

            var result = (ViewResult)controller.ManageLabels();
            CollectionAssert.AreEqual(expectedResult.ToList(), (result.Model as IEnumerable<Label>).ToList());
        }

        [TestMethod]
        public void DeleteLabel_WithValidId_Test()
        {
            var id = 1;
            var labelToRemove = labelIRepository.GetById(id);
            
            var result = (RedirectToRouteResult)controller.DeleteLabel(id.ToString());
            var labelStillExists = labelIRepository.Get().Any(l => l.Name == labelToRemove.Name);

            Assert.IsFalse(labelStillExists);
            Assert.IsNotNull(controller.ViewBag.LabelRemovedSuccessFully);
            Assert.AreEqual(ActionNames.ManageLabels.ToString(), result.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteLabel_WithInvalidId_Test()
        {
            var id = 0;

            var result = (RedirectToRouteResult)controller.DeleteLabel(id.ToString());

            Assert.IsNotNull(controller.ViewBag.LabelNotFoundError);
            Assert.AreEqual(ActionNames.ManageLabels.ToString(), result.RouteValues["action"]);
        }

        [TestMethod]
        public void AddNewLabel_LabelNotExists_Test()
        {
            var model = new AddNewLabelViewModel()
            {
                Name = "NewValue"
            };

            var result = (ViewResult)controller.AddNewLabel(model);
            var newItem = labelIRepository.Get(l => l.Name == "NewValue").Single();

            Assert.AreEqual(model.Name, newItem.Name);
            Assert.AreEqual("1", newItem.UserId);
            Assert.IsNotNull(controller.ViewBag.LabelCreatedSuccessFully);
        }

        [TestMethod]
        public void AddNewLabel_LabelExist_Test()
        {
            var model = new AddNewLabelViewModel()
            {
                Name = "Lab2"
            };

            var result = (ViewResult)controller.AddNewLabel(model);
            Assert.IsNotNull(controller.ViewBag.LabelAlreadyExists);
        }

        [TestMethod]
        public void ManageActivities_AsSortedCategory_Test()
        {
            var userId = "1";
            var categoryId = 1;

            Predicate<Activity> predicate = a => a.UserId == userId &&
                                 a.CreationType == CreationType.ManuallyCreated &&
                                 a.Category?.Id == categoryId &&
                                 a.BaseActivity;

            var fixedActivities = fixedIRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name));
            var unfixedActivities = unfixedIRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name));
            var undefinedActivities = undefinedIRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name));
            var deadlineActivities = deadlineIRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name));

            var result = (ViewResult)controller.ManageActivities(categoryId);
            var container = result.Model as ViewModelContainer;

            Assert.AreEqual(ViewNames.ListSortedByCategoryActivities.ToString(), result.ViewName);
            CollectionAssert.AreEqual(fixedActivities.ToList(), container.FixedActivityViewModel.FixedActivityCollection);
            CollectionAssert.AreEqual(unfixedActivities.ToList(), container.UnfixedActivityViewModel.UnfixedActivityCollection);
            CollectionAssert.AreEqual(undefinedActivities.ToList(), container.UndefinedActivityViewModel.UndefinedActivityCollection);
            CollectionAssert.AreEqual(deadlineActivities.ToList(), container.DeadlineActivityViewModel.DeadlineActivityCollection);
        }

        [TestMethod]
        public void ManageActivities_Test()
        {
            var userId = "1";
            var categoryId = -1;

            Predicate<Activity> predicate = a => a.UserId == userId &&
                                 a.CreationType == CreationType.ManuallyCreated &&
                                 a.BaseActivity;

            var fixedActivities = fixedIRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name));
            var unfixedActivities = unfixedIRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name));
            var undefinedActivities = undefinedIRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name));
            var deadlineActivities = deadlineIRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name));

            var result = (ViewResult)controller.ManageActivities(categoryId);
            var container = result.Model as ViewModelContainer;

            Assert.AreEqual(ViewNames.ManageActivities.ToString(), result.ViewName);
            CollectionAssert.AreEqual(fixedActivities.ToList(), container.FixedActivityViewModel.FixedActivityCollection);
            CollectionAssert.AreEqual(unfixedActivities.ToList(), container.UnfixedActivityViewModel.UnfixedActivityCollection);
            CollectionAssert.AreEqual(undefinedActivities.ToList(), container.UndefinedActivityViewModel.UndefinedActivityCollection);
            CollectionAssert.AreEqual(deadlineActivities.ToList(), container.DeadlineActivityViewModel.DeadlineActivityCollection);
        }

        [TestMethod]
        public void ActivityDetails_WithFixedActivityType_Test()
        {
            var activityId = 1;

            var result = (ViewResult)controller.ActivityDetails(activityId.ToString(), ActivityType.FixedActivity);
            var selectedActivity = fixedIRepository.GetById(activityId);

            Assert.AreEqual(selectedActivity, (result.Model as FixedActivityViewModel).FixedActivityCollection.First());
            Assert.AreEqual(ViewNames.FixedActivityDetails.ToString(), result.ViewName);
        }

        [TestMethod]
        public void ActivityDetails_WithUnfixedActivityType_Test()
        {
            var activityId = 1;

            var result = (ViewResult)controller.ActivityDetails(activityId.ToString(), ActivityType.UnfixedActivity);
            var selectedActivity = unfixedIRepository.GetById(activityId);

            Assert.AreEqual(selectedActivity, (result.Model as UnfixedActivityViewModel).UnfixedActivityCollection.First());
            Assert.AreEqual(ViewNames.UnfixedActivityDetails.ToString(), result.ViewName);
        }

        [TestMethod]
        public void ActivityDetails_WithUndefinedActivityType_Test()
        {
            var activityId = 1;

            var result = (ViewResult)controller.ActivityDetails(activityId.ToString(), ActivityType.UndefinedActivity);
            var selectedActivity = undefinedIRepository.GetById(activityId);

            Assert.AreEqual(selectedActivity, (result.Model as UndefinedActivityViewModel).UndefinedActivityCollection.First());
            Assert.AreEqual(ViewNames.UndefinedActivityDetails.ToString(), result.ViewName);
        }

        [TestMethod]
        public void ActivityDetails_WithDeadlineActivityType_Test()
        {
            var activityId = 1;

            var result = (ViewResult)controller.ActivityDetails(activityId.ToString(), ActivityType.DeadlineActivity);
            var selectedActivity = deadlineIRepository.GetById(activityId);

            Assert.AreEqual(selectedActivity, (result.Model as DeadlineActivityViewModel).DeadlineActivityCollection.First());
            Assert.AreEqual(ViewNames.DeadlineActivityDetails.ToString(), result.ViewName);
        }

        [TestMethod]
        public void DeleteActivity_WithFixedActivityType()
        {
            int activityId = 1;

            var activityExists = fixedIRepository.GetById(activityId) != null;
            var result = (RedirectToRouteResult)controller.DeleteActivity(activityId.ToString(), ActivityType.FixedActivity);
            var activityIsRemoved = fixedIRepository.GetById(activityId) == null;

            Assert.IsTrue(activityExists);
            Assert.IsTrue(activityIsRemoved);
            Assert.IsNotNull(controller.ViewBag.ActivityRemovedSuccessfully);
        }

        [TestMethod]
        public void DeleteActivity_WithUnfixedActivityType()
        {
            int activityId = 1;

            var activityExists = unfixedIRepository.GetById(activityId) != null;
            var result = (RedirectToRouteResult)controller.DeleteActivity(activityId.ToString(), ActivityType.UnfixedActivity);
            var activityIsRemoved = unfixedIRepository.GetById(activityId) == null;

            Assert.IsTrue(activityExists);
            Assert.IsTrue(activityIsRemoved);
            Assert.IsNotNull(controller.ViewBag.ActivityRemovedSuccessfully);
        }

        [TestMethod]
        public void DeleteActivity_WithUndefinedActivityType()
        {
            int activityId = 1;

            var activityExists = undefinedIRepository.GetById(activityId) != null;
            var result = (RedirectToRouteResult)controller.DeleteActivity(activityId.ToString(), ActivityType.UndefinedActivity);
            var activityIsRemoved = undefinedIRepository.GetById(activityId) == null;

            Assert.IsTrue(activityExists);
            Assert.IsTrue(activityIsRemoved);
            Assert.IsNotNull(controller.ViewBag.ActivityRemovedSuccessfully);
        }

        [TestMethod]
        public void DeleteActivity_WithDeadlineActivityType()
        {
            int activityId = 1;

            var activityExists = deadlineIRepository.GetById(activityId) != null;
            var result = (RedirectToRouteResult)controller.DeleteActivity(activityId.ToString(), ActivityType.DeadlineActivity);
            var activityIsRemoved = deadlineIRepository.GetById(activityId) == null;

            Assert.IsTrue(activityExists);
            Assert.IsTrue(activityIsRemoved);
            Assert.IsNotNull(controller.ViewBag.ActivityRemovedSuccessfully);
        }

        [TestMethod]
        public void AddNewActivity_Get_Test()
        {
            var result = (ViewResult)controller.AddNewActivity();
            var model = result.Model as ViewModelManagerContainer;

            Assert.IsNotNull(model.FixedActivityManageViewModel);
            Assert.IsNotNull(model.UnfixedActivityManageViewModel);
            Assert.IsNotNull(model.UndefinedActivityManageViewModel);
            Assert.IsNotNull(model.DeadlineActivityManageViewModel);
            Assert.AreEqual("AddNewActivity", result.ViewName);
        }
    }
}
