using Dama.Data.Enums;
using Dama.Data.Models;
using Dama.Data.Sql.SQL;
using Dama.Organizer.Models;
using Dama.Organizer.Resources;
using Dama.Web.Attributes;
using Dama.Web.Models;
using Dama.Web.Models.ViewModels;
using Dama.Web.Models.ViewModels.Activity.Display;
using Dama.Web.Models.ViewModels.Activity.Manage;
using Dama.Web.Models.ViewModels.Category;
using Dama.Web.Models.ViewModels.Label;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Dama.Web.Manager;
using ActionNames = Dama.Organizer.Enums.ActionNames;
using ControllerNames = Dama.Organizer.Enums.ControllerNames;
using ViewNames = Dama.Organizer.Enums.ViewNames;
using Repeat = Dama.Data.Models.Repeat;
using Milestone = Dama.Data.Models.Milestone;
using Dama.Data.Sql.Interfaces;
using System.Linq.Expressions;

namespace Dama.Web.Controllers
{
    [Authorize]
    [DisableUser]
    public class CalendarController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly List<SelectListItem> _colors;
        private readonly CalendarControllerManager _calendarControllerManager;
        private readonly string[] _availableColors;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositorySettings _repositorySettings;

        public CalendarController(IUnitOfWork unitOfWork, IRepositorySettings repositorySettings)
        {
            _unitOfWork = unitOfWork;
            _repositorySettings = repositorySettings;
            _userManager = new UserManager<User>(new UserStore<User>(new DamaContext()));
            _colors = new List<SelectListItem>();
            _availableColors = Enum.GetNames(typeof(Color));
            _colors = _availableColors.Select(c => new SelectListItem() { Text = c.ToString() }).ToList();
            _calendarControllerManager = new CalendarControllerManager(_unitOfWork);
        }

        public string UserId => User.Identity.GetUserId();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetActivities() //TODO: implement correctly
        {
            var fixedActivities = _unitOfWork.FixedActivityRepository.Get(x => x.UserId == UserId).ToList();
            JsonResult jsonResult = new JsonResult { Data = fixedActivities, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return jsonResult;
        }

        #region Category
        public ActionResult AddNewCategory()
        {
            AddNewCategoryViewModel viewModel = new AddNewCategoryViewModel() { Color = _colors };
            return View(viewModel);
        }

        public ActionResult ManageCategories()
        {
            var categories = _unitOfWork.CategoryRepository.Get(x => x.UserId == UserId);
            return View(categories);
        }

        public ActionResult DeleteCategory(string categoryId)
        {
            var selectedCategory = _unitOfWork.CategoryRepository.GetByID(categoryId);

            if (selectedCategory == null)
            {
                ViewBag.CategoryNotFoundError = Error.CategoryNotFound;
            }
            else
            {
                RemoveCategoryFromTables(selectedCategory.Id);
                _unitOfWork.CategoryRepository.Delete(selectedCategory);
                _unitOfWork.Save();
                ViewBag.CategoryRemovedSuccessFully = Success.CategoryRemovedSuccessfully;
            }

            return RedirectToAction(ActionNames.ManageCategories.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewCategory(AddNewCategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var builder = new CategoryBuilder();
                Category newCategory = builder.CreateCategory(viewModel.Name)
                                             .WithDescription(viewModel.Description)
                                             .WithColor((Color)Enum.Parse(typeof(Color), viewModel.SelectedColor))
                                             .WithPriority(viewModel.Priority)
                                             .WithUserId(UserId);

                var categoryAlreadyExists = _unitOfWork.CategoryRepository
                                                         .Get(c => c.UserId == newCategory.UserId && c.Name == newCategory.Name)
                                                         .Any();
                if (categoryAlreadyExists)
                {
                    ViewBag.CategoryAlreadyExists = Error.CategoryAlreadyExists;
                }
                else
                {
                    _unitOfWork.CategoryRepository.Insert(newCategory);
                    _unitOfWork.Save();
                    ViewBag.CategoryCreatedSuccessFully = Success.CategoryCreatedSuccessfully;
                }
            }

            viewModel.Color = _colors;
            return View(viewModel);
        }

        public ActionResult EditCategory(string categoryId)
        {
            EditCategoryViewModel viewModel = null;
            var category = _unitOfWork.CategoryRepository.GetByID(categoryId);

            if (category != null)
            {
                viewModel = new EditCategoryViewModel()
                {
                    Id = category.Id.ToString(),
                    Color = _colors,
                    Description = category.Description,
                    Name = category.Name,
                    Priority = category.Priority,
                    CurrentColor = category.Color.ToString(),
                    SelectedColor = category.Color.ToString()
                };
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(EditCategoryViewModel viewModel)
        {
            var currentId = viewModel.Id;

            if (ModelState.IsValid)
            {
                var categories = _unitOfWork.CategoryRepository.Get(c => c.UserId == UserId).ToList();

                foreach (var category in categories)
                {
                    if (category.Name == viewModel.Name && category.Id.ToString() != viewModel.Id)
                    {
                        ViewBag.CategoryAlreadyExists = Error.CategoryAlreadyExists;
                        viewModel.Color = _colors;
                        return View(viewModel);
                    }
                }

                var currentCategory = _unitOfWork.CategoryRepository.GetByID(currentId);
                currentCategory.Name = viewModel.Name;
                currentCategory.Description = viewModel.Description;
                currentCategory.Priority = viewModel.Priority;
                currentCategory.Color = (Color)Enum.Parse(typeof(Color), viewModel.SelectedColor);

                _unitOfWork.CategoryRepository.Update(currentCategory);
                _unitOfWork.Save();
                ViewBag.CategoryChangedSuccessfully = Success.CategoryChangesSuccessfully;
            }

            viewModel.Color = _colors;
            return View(viewModel);
        }

        private void RemoveCategoryFromTables(int categoryId)
        {
            foreach (var record in _unitOfWork.FixedActivityRepository.Get(includeProperties: a => a.Category))
                if (record.Category != null && record.Category.Id.Equals(categoryId))
                    record.Category = null;

            foreach (var record in _unitOfWork.UnfixedActivityRepository.Get(includeProperties: a => a.Category))
                if (record.Category != null && record.Category.Id.Equals(categoryId))
                    record.Category = null;

            foreach (var record in _unitOfWork.UndefinedActivityRepository.Get(includeProperties: a => a.Category))
                if (record.Category != null && record.Category.Id.Equals(categoryId))
                    record.Category = null;

            foreach (var record in _unitOfWork.DeadlineActivityRepository.Get(includeProperties: a => a.Category))
                if (record.Category != null && record.Category.Id.Equals(categoryId))
                    record.Category = null;
        }
        #endregion

        #region Label
        public ActionResult AddNewLabel()
        {
            return View();
        }

        public ActionResult ManageLabels()
        {
            var labels = _unitOfWork.LabelRepository
                                    .Get(l => l.UserId == UserId)
                                    .GroupBy(l => l.Name)
                                    .Select(l => l.First());

            return View(labels);
        }

        public ActionResult DeleteLabel(string labelId)
        {
            var selectedLabel = _unitOfWork.LabelRepository.GetByID(labelId);

            if (selectedLabel == null)
            {
                ViewBag.LabelNotFoundError = Error.LabelNotFound; //Not diplayed correctly
            }
            else
            {
                var itemsToRemove = _unitOfWork.LabelRepository
                                               .Get(l => l.Name == selectedLabel.Name)
                                               .ToList();

                foreach (var item in itemsToRemove)
                    _unitOfWork.LabelRepository.Delete(item);

                _unitOfWork.Save();
                ViewBag.LabelRemovedSuccessFully = Success.LabelRemovedSuccessfully;
            }

            return RedirectToAction(ActionNames.ManageLabels.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewLabel(AddNewLabelViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var newLabel = new Label(viewModel.Name, UserId);

                var labelAlreadyExists = _unitOfWork.LabelRepository
                                                    .Get(l => l.UserId == newLabel.UserId && l.Name == newLabel.Name)
                                                    .Any();

                if (labelAlreadyExists)
                {
                    ViewBag.LabelAlreadyExists = Error.LabelAlreadyExists;
                    return View();
                }

                _unitOfWork.LabelRepository.Insert(newLabel);
                _unitOfWork.Save();

                ViewBag.LabelCreatedSuccessFully = Success.LabelCreatedSuccessfully;
            }

            return View();
        }
        #endregion

        #region Activity
        /// <param name="categoryId"> If the Id is not 0, it is managed as sorted by category</param>
        public ActionResult ManageActivities(int categoryId = -1)
        {
            Predicate<Activity> predicate;
            var sortedByCategory = categoryId != -1;

            if (sortedByCategory)
            {
                predicate = a => a.UserId == UserId &&
                                 a.CreationType == CreationType.ManuallyCreated &&
                                 a.Category?.Id == categoryId &&
                                 a.BaseActivity;
            }
            else
            {
                predicate = a => a.UserId == UserId &&
                                 a.CreationType == CreationType.ManuallyCreated
                                 && a.BaseActivity;
            }

            var fixedActivities = _unitOfWork.FixedActivityRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name), a => a.Labels, a => a.Category);
            var unfixedActivities = _unitOfWork.UnfixedActivityRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name), a => a.Labels, a => a.Category);
            var undefinedActivities = _unitOfWork.UndefinedActivityRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name), a => a.Labels, a => a.Category);
            var deadlineActivities = _unitOfWork.DeadlineActivityRepository.Get(a => predicate(a), a => a.OrderBy(aa => aa.Name), a => a.Labels, a => a.Category, a => a.Milestones);

            var container = new ViewModelContainer()
            {
                FixedActivityViewModel = new FixedActivityViewModel() { FixedActivityCollection = fixedActivities.ToList() },
                UnfixedActivityViewModel = new UnfixedActivityViewModel() { UnfixedActivityCollection = unfixedActivities.ToList() },
                UndefinedActivityViewModel = new UndefinedActivityViewModel() { UndefinedActivityCollection = undefinedActivities.ToList() },
                DeadlineActivityViewModel = new DeadlineActivityViewModel() { DeadlineActivityCollection = deadlineActivities.ToList() }
            };

            if (sortedByCategory)
                return View(ViewNames.ListSortedByCategoryActivities.ToString(), container);
            else
                return View(ViewNames.ManageActivities.ToString(), container);
        }

        public ActionResult ActivityDetails(string activityId, ActivityType activityType)
        {
            var id = int.Parse(activityId);

            switch (activityType)
            {
                case ActivityType.FixedActivity:
                    var fixedActivities = _unitOfWork.FixedActivityRepository.Get(a => a.Id == id, null, a => a.Category, a => a.Labels);
                    var fixedActivityModel = new FixedActivityViewModel() { FixedActivityCollection = fixedActivities.ToList() };
                    return View(ViewNames.FixedActivityDetails.ToString(), fixedActivityModel);

                case ActivityType.UnfixedActivity:
                    var unfixedActivities = _unitOfWork.UnfixedActivityRepository.Get(a => a.Id == id, null, a => a.Category, a => a.Labels);
                    var unfixedActivityModel = new UnfixedActivityViewModel() { UnfixedActivityCollection = unfixedActivities.ToList() };
                    return View(ViewNames.UnfixedActivityDetails.ToString(), unfixedActivityModel);

                case ActivityType.UndefinedActivity:
                    var undefinedActivities = _unitOfWork.UndefinedActivityRepository.Get(a => a.Id == id, null, a => a.Category, a => a.Labels);
                    var undefinedActivityModel = new UndefinedActivityViewModel() { UndefinedActivityCollection = undefinedActivities.ToList() };
                    return View(ViewNames.UndefinedActivityDetails.ToString(), undefinedActivityModel);

                case ActivityType.DeadlineActivity:
                    var deadlineActivities = _unitOfWork.DeadlineActivityRepository.Get(a => a.Id == id, null, a => a.Category, a => a.Labels, a => a.Milestones);
                    var deadlineActivityModel = new DeadlineActivityViewModel() { DeadlineActivityCollection = deadlineActivities.ToList() };
                    return View(ViewNames.DeadlineActivityDetails.ToString(), deadlineActivityModel);

                default:
                    return RedirectToAction(ActionNames.ManageActivities.ToString());
            }
        }

        public ActionResult DeleteActivity(string activityId, ActivityType activityType)
        {
            int id = int.Parse(activityId);

            switch (activityType)
            {
                case ActivityType.FixedActivity:
                    var fixedActivity = _unitOfWork.FixedActivityRepository.Get(a => a.Id == id, null, a => a.Category, a => a.Labels).FirstOrDefault();

                    if (fixedActivity != null)
                    {
                        if (fixedActivity.Labels != null)
                            _unitOfWork.LabelRepository.DeleteRange(fixedActivity.Labels);

                        fixedActivity.Category = null;
                        _unitOfWork.FixedActivityRepository.Delete(fixedActivity);
                        _unitOfWork.Save();
                        ViewBag.ActivityRemovedSuccessfully = Success.ActivityRemovedSuccessfully;
                    }
                    break;

                case ActivityType.UnfixedActivity:
                    var unfixedActivity = _unitOfWork.UnfixedActivityRepository.Get(a => a.Id == id, null, a => a.Category, a => a.Labels).FirstOrDefault();

                    if (unfixedActivity != null)
                    {
                        if (unfixedActivity.Labels != null)
                            _unitOfWork.LabelRepository.DeleteRange(unfixedActivity.Labels);

                        unfixedActivity.Category = null;

                        _unitOfWork.UnfixedActivityRepository.Delete(unfixedActivity);
                        _unitOfWork.Save();
                        ViewBag.ActivityRemovedSuccessfully = Success.ActivityRemovedSuccessfully;
                    }
                    break;


                case ActivityType.UndefinedActivity:
                    var undefinedActivity = _unitOfWork.UndefinedActivityRepository.Get(a => a.Id == id, null, a => a.Category, a => a.Labels).FirstOrDefault();

                    if (undefinedActivity != null)
                    {
                        if (undefinedActivity.Labels != null)
                            _unitOfWork.LabelRepository.DeleteRange(undefinedActivity.Labels);

                        undefinedActivity.Category = null;
                        _unitOfWork.UndefinedActivityRepository.Delete(undefinedActivity);
                        _unitOfWork.Save();
                        ViewBag.ActivityRemovedSuccessfully = Success.ActivityRemovedSuccessfully;
                    }
                    break;

                case ActivityType.DeadlineActivity:
                    var deadlineActivity = _unitOfWork.DeadlineActivityRepository.Get(a => a.Id == id, null, a => a.Category, a => a.Labels, a => a.Milestones).FirstOrDefault();

                    if (deadlineActivity != null)
                    {
                        if (deadlineActivity.Labels != null)
                            _unitOfWork.LabelRepository.DeleteRange(deadlineActivity.Labels);

                        deadlineActivity.Category = null;

                        if (deadlineActivity.Milestones != null)
                            _unitOfWork.MilestoneRepository.DeleteRange(deadlineActivity.Milestones);

                        _unitOfWork.DeadlineActivityRepository.Delete(deadlineActivity);
                        _unitOfWork.Save();
                        ViewBag.ActivityRemovedSuccessFully = Success.ActivityRemovedSuccessfully;
                    }
                    break;
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        public ActionResult AddNewActivity()
        {
            var colors = new List<SelectListItem>(_colors);
            var categories = AddCategoriesToProcess(UserId);
            var labels = AddLabelsToProcess(UserId);
            var repeatTypeList = AddRepeatTypeToProcess();

            var fixedActivityViewModel = new FixedActivityManageViewModel()
            {
                LabelSourceCollection = labels,
                CategorySourceCollection = categories,
                ColorSourceCollection = colors,
                RepeatTypeSourceCollection = repeatTypeList
            };

            var unfixedActivityViewModel = new UnfixedActivityManageViewModel()
            {
                LabelSourceCollection = labels,
                CategorySourceCollection = categories,
                ColorSourceCollection = colors,
                RepeatTypeSourceCollection = repeatTypeList
            };

            var undefinedActivityViewModel = new UndefinedActivityManageViewModel()
            {
                LabelSourceCollection = labels,
                CategorySourceCollection = categories,
                ColorSourceCollection = colors,
            };

            var deadlineActivityViewModel = new DeadlineActivityManageViewModel()
            {
                ColorSourceCollection = colors
            };

            var container = new ViewModelManagerContainer()
            {
                FixedActivityManageViewModel = fixedActivityViewModel,
                UnfixedActivityManageViewModel = unfixedActivityViewModel,
                UndefinedActivityManageViewModel = undefinedActivityViewModel,
                DeadlineActivityManageViewModel = deadlineActivityViewModel
            };

            return View(container);
        }

        public List<SelectListItem> AddRepeatTypeToProcess()
        {
            var enums = Enum.GetValues(typeof(RepeatPeriod)).Cast<RepeatPeriod>();

            return enums
                    .Select(e => 
                        new SelectListItem()
                        {
                            Text = e.ToString(),
                            Value = e.ToString()
                        })
                    .ToList();
        }

        public List<SelectListItem> AddCategoriesToProcess(string userId)
        {
            var categorySelectItemList = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = "Uncategorized",
                    Value = null
                }
            };

            var categories = _unitOfWork.CategoryRepository.Get(c => c.UserId == userId);
            categorySelectItemList.AddRange(categories.Select(c => new SelectListItem() { Text = c.Name }));

            return categorySelectItemList;
        }

        public List<SelectListItem> AddLabelsToProcess(string userId)
        {
            var labels = _unitOfWork.LabelRepository.Get(l => l.UserId == userId).Select(l => l.Name).Distinct();
            var labelSelectItems = labels.Select(l => new SelectListItem() { Text = l }).ToList();

            return labelSelectItems;
        }

        public ActionResult EditActivity(string activityId, ActivityType? activityType, bool calledFromEditor = false, bool optional = false)
        {
            var details = new EditDetails()
            {
                ActivityId = int.Parse(activityId),
                ActivityType = activityType,
                CalledFromEditor = calledFromEditor,
                IsOptional = optional,
                Categories = AddCategoriesToProcess(UserId),
                Colors = new List<SelectListItem>(_colors),
                Labels = AddLabelsToProcess(UserId),
                RepeatTypes = AddRepeatTypeToProcess(),
            };
            var path = "../Calendar";

            switch (activityType)
            {
                case ActivityType.FixedActivity:
                    {
                        var fixedActivityViewModel = _calendarControllerManager.AssembleFixedActivityManageViewModel(details);

                        if (calledFromEditor)
                            return PartialView(Path.Combine(path, ViewNames.EditFixedActivity.ToString()), fixedActivityViewModel);

                        return View(ViewNames.EditFixedActivity.ToString(), fixedActivityViewModel);
                    }

                case ActivityType.UnfixedActivity:
                    {
                        var unfixedActivityViewModel = _calendarControllerManager.AssembleUnfixedActivityViewModel(details);

                        if (calledFromEditor)
                            return PartialView(Path.Combine(path, ViewNames.EditUnfixedActivity.ToString()), unfixedActivityViewModel);

                        return View(ViewNames.EditUnfixedActivity.ToString(), unfixedActivityViewModel);
                    }

                case ActivityType.UndefinedActivity:
                    {
                        var undefinedActivityViewModel = _calendarControllerManager.AssembleUndefinedActivityViewModel(details);

                        if (calledFromEditor)
                            return PartialView(Path.Combine(path, ViewNames.EditUndefinedActivity.ToString()), undefinedActivityViewModel);

                        return View(ViewNames.EditUndefinedActivity.ToString(), undefinedActivityViewModel);
                    }

                case ActivityType.DeadlineActivity:
                    {
                        var deadlineActivityViewModel = _calendarControllerManager.AssembleDeadlineActivityViewModel(details);

                        if (calledFromEditor)
                            return PartialView(Path.Combine(path, ViewNames.EditDeadlineActivity.ToString()), deadlineActivityViewModel);

                        return View(ViewNames.EditDeadlineActivity.ToString(), deadlineActivityViewModel);
                    }

                default:
                    return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewActivity(ViewModelManagerContainer container)
        {
            return CreateNewActivityFromContainer(container);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFixedActivity(FixedActivityManageViewModel viewModel)
        {
            var changedFromEditor = viewModel.EnableRepeatChange;
            int activityId = int.Parse(Request.Form["id"]); //Get the Id from view

            if (ModelState.IsValid)
            {
                if (changedFromEditor) //No need to change the database
                {
                    using (var container = new ActivityContainer())
                    {
                        container.Reset = false;

                        if (viewModel.IsOptional == true)
                        {
                            var editedActivity = container.ActivitySelectedByUserForOptional
                                                                .FirstOrDefault(a => a.Id == activityId &&
                                                                                     a.ActivityType == ActivityType.FixedActivity) as FixedActivity;
                            editedActivity += CreateNewFixedActivityMethod(viewModel, false);
                        }
                        else
                        {
                            var editedActivity = container.ActivitySelectedByUserForSure
                                                                .FirstOrDefault(a => a.Id == activityId &&
                                                                                     a.ActivityType == ActivityType.FixedActivity) as FixedActivity;
                            editedActivity += CreateNewFixedActivityMethod(viewModel, false);
                        }
                    }

                    return RedirectToAction(ActionNames.Editor.ToString(), ControllerNames.CalendarEditor.ToString());
                }
                else
                {
                    var activity = _unitOfWork.FixedActivityRepository.Get(a => a.Id == activityId, null, a => a.Category, a => a.Labels).FirstOrDefault();

                    if (activity != null)
                    {
                        if (activity.Labels != null)
                            _unitOfWork.LabelRepository.DeleteRange(activity.Labels);

                        activity.Category = null;
                        _unitOfWork.FixedActivityRepository.Delete(activity);
                        _unitOfWork.Save();
                    }
                }

                var viewModelContainer = new ViewModelManagerContainer() { FixedActivityManageViewModel = viewModel };
                CreateNewActivityFromContainer(viewModelContainer);
            }
            else
            {
                viewModel.LabelSourceCollection = AddLabelsToProcess(UserId);
                viewModel.CategorySourceCollection = AddCategoriesToProcess(UserId);
                viewModel.RepeatTypeSourceCollection = AddRepeatTypeToProcess();
                viewModel.ColorSourceCollection = _colors;

                return View(viewModel);
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUnfixedActivity(UnfixedActivityManageViewModel viewModel)
        {
            var changedFromEditor = viewModel.EnableRepeatChange;
            var activityId = int.Parse(Request.Form["id"]);

            if (ModelState.IsValid)
            {
                if (changedFromEditor)
                {
                    using (var container = new ActivityContainer())
                    {
                        container.Reset = false;

                        if (viewModel.IsOptional == true)
                        {
                            var editedActivity = container.ActivitySelectedByUserForOptional
                                                                .FirstOrDefault(a => a.Id == activityId &&
                                                                                        a.ActivityType == ActivityType.UnfixedActivity) as UnfixedActivity;
                            editedActivity += CreateNewUnfixedActivityMethod(viewModel, false);
                        }
                        else
                        {
                            var editedActivity = container.ActivitySelectedByUserForSure
                                                                .FirstOrDefault(a => a.Id == activityId &&
                                                                                        a.ActivityType == ActivityType.UnfixedActivity) as UnfixedActivity;
                            editedActivity += CreateNewUnfixedActivityMethod(viewModel, false);
                        }
                    }

                    return RedirectToAction(ActionNames.Editor.ToString(), ControllerNames.CalendarEditor.ToString());
                }
                else
                {
                    var activity = _unitOfWork.UnfixedActivityRepository.Get(a => a.Id == activityId, null, a => a.Category, a => a.Labels).FirstOrDefault();

                    if (activity != null)
                    {
                        if (activity.Labels != null)
                            _unitOfWork.LabelRepository.DeleteRange(activity.Labels);

                        activity.Category = null;
                        _unitOfWork.UnfixedActivityRepository.Delete(activity);
                        _unitOfWork.Save();
                    }
                }

                var viewModelContainer = new ViewModelManagerContainer() { UnfixedActivityManageViewModel = viewModel };
                CreateNewActivityFromContainer(viewModelContainer);
            }
            else
            {
                viewModel.LabelSourceCollection = AddLabelsToProcess(UserId);
                viewModel.CategorySourceCollection = AddCategoriesToProcess(UserId);
                viewModel.RepeatTypeSourceCollection = AddRepeatTypeToProcess();
                viewModel.ColorSourceCollection = _colors;

                return View(viewModel);
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUndefinedActivity(UndefinedActivityManageViewModel viewModel)
        {
            int activityId = int.Parse(Request.Form["id"]);

            if (ModelState.IsValid)
            {
                if (viewModel.CalledFromEditor)
                {
                    using (var container = new ActivityContainer())
                    {
                        container.Reset = false;
                        var editedActivity = container.ActivitySelectedByUserForOptional
                                                        .FirstOrDefault(a => a.Id == activityId &&
                                                                             a.ActivityType == ActivityType.UndefinedActivity) as UndefinedActivity;

                        editedActivity += CreateNewUndefinedActivityMethod(viewModel, false);
                    }

                    return RedirectToAction(ActionNames.Editor.ToString(), ControllerNames.CalendarEditor.ToString());
                }
                else
                {
                    var activity = _unitOfWork.UndefinedActivityRepository.Get(a => a.Id == activityId, null, a => a.Category, a => a.Labels).FirstOrDefault();

                    if (activity != null)
                    {
                        if (activity.Labels != null)
                            _unitOfWork.LabelRepository.DeleteRange(activity.Labels);

                        activity.Category = null;
                        _unitOfWork.UndefinedActivityRepository.Delete(activity);
                        _unitOfWork.Save();
                    }
                }

                var viewModelContainer = new ViewModelManagerContainer() { UndefinedActivityManageViewModel = viewModel };

                CreateNewActivityFromContainer(viewModelContainer);
            }
            else
            {
                viewModel.LabelSourceCollection = AddLabelsToProcess(UserId);
                viewModel.CategorySourceCollection = AddCategoriesToProcess(UserId);
                viewModel.ColorSourceCollection = _colors;

                return View(viewModel);
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDeadlineActivity(DeadlineActivityManageViewModel viewModel)
        {
            int activityId = int.Parse(Request.Form["id"]);

            if (ModelState.IsValid)
            {
                if (viewModel.CalledFromEditor)
                {
                    using (var container = new ActivityContainer())
                    {
                        container.Reset = false;
                        var editedActivity = container.ActivitySelectedByUserForSure
                                                          .FirstOrDefault(a => a.Id == activityId &&
                                                                               a.ActivityType == ActivityType.DeadlineActivity) as DeadlineActivity;

                        editedActivity += CreateNewDeadlineActivityMethod(viewModel, false);
                    }

                    return RedirectToAction(ActionNames.Editor.ToString(), ControllerNames.CalendarEditor.ToString());
                }
                else
                {
                    var activity = _unitOfWork.DeadlineActivityRepository.Get(a => a.Id == activityId, null, a => a.Labels, a => a.Category, a => a.Milestones).FirstOrDefault();

                    if (activity != null)
                    {
                        if (activity.Labels != null)
                            _unitOfWork.LabelRepository.DeleteRange(activity.Labels);

                        activity.Category = null;

                        if (activity.Milestones != null)
                            _unitOfWork.MilestoneRepository.DeleteRange(activity.Milestones);

                        _unitOfWork.DeadlineActivityRepository.Delete(activity);
                        _unitOfWork.Save();
                        ViewBag.ActivityRemovedSuccessFully = Success.ActivityRemovedSuccessfully;
                    }
                }

                var viewModelContainer = new ViewModelManagerContainer() { DeadlineActivityManageViewModel = viewModel };
                CreateNewActivityFromContainer(viewModelContainer);
            }
            else
            {
                viewModel.ColorSourceCollection = _colors;
                return View(viewModel);
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        private ActionResult CreateNewActivityFromContainer(ViewModelManagerContainer modelManagerContainer)
        {
            if (modelManagerContainer.FixedActivityManageViewModel != null)
            {
                var fixedActivityViewModel = modelManagerContainer.FixedActivityManageViewModel;

                if (ModelState.IsValid)
                {
                    var fixedActivity = CreateNewFixedActivityMethod(fixedActivityViewModel, true);

                    if (fixedActivity.Category != null)
                        _repositorySettings.ChangeCategoryEntryState(fixedActivity.Category, EntityState.Unchanged);

                    _unitOfWork.FixedActivityRepository.Insert(fixedActivity);
                    _unitOfWork.Save();

                    return RedirectToAction(ActionNames.ManageActivities.ToString());
                }
                else
                {
                    fixedActivityViewModel.LabelSourceCollection = AddLabelsToProcess(UserId);
                    fixedActivityViewModel.CategorySourceCollection = AddCategoriesToProcess(UserId);
                    fixedActivityViewModel.ColorSourceCollection = _colors;
                    var viewModelContainer = new ViewModelManagerContainer() { FixedActivityManageViewModel = fixedActivityViewModel };

                    return View(viewModelContainer);
                }
            }

            if (modelManagerContainer.UnfixedActivityManageViewModel != null)
            {
                var unfixedActivityViewModel = modelManagerContainer.UnfixedActivityManageViewModel;

                if (ModelState.IsValid)
                {
                    var unfixedActivity = CreateNewUnfixedActivityMethod(unfixedActivityViewModel, true);

                    if (unfixedActivity.Category != null)
                        _repositorySettings.ChangeCategoryEntryState(unfixedActivity.Category, EntityState.Unchanged);

                    _unitOfWork.UnfixedActivityRepository.Insert(unfixedActivity);
                    _unitOfWork.Save();

                    return RedirectToAction(ActionNames.ManageActivities.ToString());
                }
                else
                {
                    unfixedActivityViewModel.LabelSourceCollection = AddLabelsToProcess(UserId);
                    unfixedActivityViewModel.CategorySourceCollection = AddCategoriesToProcess(UserId);
                    unfixedActivityViewModel.ColorSourceCollection = _colors;
                    var viewModelContainer = new ViewModelManagerContainer() { UnfixedActivityManageViewModel = unfixedActivityViewModel };

                    return View(viewModelContainer);
                }
            }

            if (modelManagerContainer.UndefinedActivityManageViewModel != null)
            {
                var undefinedActivityViewModel = modelManagerContainer.UndefinedActivityManageViewModel;

                if (ModelState.IsValid)
                {
                    var undefinedActivity = CreateNewUndefinedActivityMethod(undefinedActivityViewModel, true);

                    if (undefinedActivity.Category != null)
                        _repositorySettings.ChangeCategoryEntryState(undefinedActivity.Category, EntityState.Unchanged);

                    _unitOfWork.UndefinedActivityRepository.Insert(undefinedActivity);
                    _unitOfWork.Save();

                    return RedirectToAction(ActionNames.ManageActivities.ToString());
                }
                else
                {
                    undefinedActivityViewModel.LabelSourceCollection = AddLabelsToProcess(UserId);
                    undefinedActivityViewModel.CategorySourceCollection = AddCategoriesToProcess(UserId);
                    undefinedActivityViewModel.ColorSourceCollection = _colors;
                    var viewModelContainer = new ViewModelManagerContainer() { UndefinedActivityManageViewModel = undefinedActivityViewModel };

                    return View(viewModelContainer);
                }
            }

            if (modelManagerContainer.DeadlineActivityManageViewModel != null)
            {
                var deadlineActivityViewModel = modelManagerContainer.DeadlineActivityManageViewModel;

                if (ModelState.IsValid)
                {
                    var deadlineActivity = CreateNewDeadlineActivityMethod(deadlineActivityViewModel, true);
                    _unitOfWork.DeadlineActivityRepository.Insert(deadlineActivity);
                    _unitOfWork.Save();

                    return RedirectToAction(ActionNames.ManageActivities.ToString());
                }
                else
                {
                    deadlineActivityViewModel.ColorSourceCollection = _colors;
                    var viewModelContainer = new ViewModelManagerContainer() { DeadlineActivityManageViewModel = deadlineActivityViewModel };

                    return View(viewModelContainer);
                }
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        private FixedActivity CreateNewFixedActivityMethod(FixedActivityManageViewModel viewModel, bool isBaseActivity)
        {
            Repeat repeat = null;
            int finalPriority;
            var start = DateTime.Today + viewModel.StartTime.TimeOfDay;
            var end = DateTime.Today + viewModel.EndTime.TimeOfDay;
            var category = _unitOfWork.CategoryRepository.Get(c => c.Name == viewModel.Category).FirstOrDefault();
            var labels = viewModel.Labels?.Select(l => new Label(l, UserId)).ToList() ?? new List<Label>();

            if (viewModel.Priority == 0)
                finalPriority = category == null ? 1 : category.Priority;
            else
                finalPriority = viewModel.Priority;

            if (viewModel.RepeatType != null && viewModel.RepeatEndDate != null)
                repeat = new Repeat(viewModel.StartTime, viewModel.RepeatEndDate, (RepeatPeriod)Enum.Parse(typeof(RepeatPeriod), viewModel.RepeatType));

            var builder = new FixedActivityBuilder();

            FixedActivity result = builder.CreateActivity(viewModel.Name)
                                            .WithDescription(viewModel.Description)
                                            .WithColor((Color)Enum.Parse(typeof(Color), viewModel.Color))
                                            .WithCreationType(CreationType.ManuallyCreated)
                                            .WithLabels(labels)
                                            .WithCategory(category)
                                            .WithUserId(UserId)
                                            .WithPriority(finalPriority)
                                            .WithStart(start)
                                            .WithEnd(end)
                                            .IsBaseActivity(isBaseActivity);
            result.Repeat = repeat;
            return result;
        }

        private UnfixedActivity CreateNewUnfixedActivityMethod(UnfixedActivityManageViewModel viewModel, bool isBaseActivity)
        {
            int finalPriority;
            Repeat repeat = null;

            var category = _unitOfWork.CategoryRepository.Get(c => c.Name == viewModel.Category).FirstOrDefault();
            var labels = viewModel.Labels?.Select(l => new Label(l, UserId)) ?? new List<Label>();

            if (viewModel.Priority == 0)
                finalPriority = category == null ? 1 : category.Priority;
            else
                finalPriority = viewModel.Priority;

            if (viewModel.RepeatType != null && viewModel.RepeatEndDate != null)
                repeat = new Repeat((RepeatPeriod)Enum.Parse(typeof(RepeatPeriod), viewModel.RepeatType), viewModel.RepeatEndDate);

            var builder = new UnfixedActivityBuilder();
            UnfixedActivity result = builder.CreateActivity(viewModel.Name)
                                     .WithDescription(viewModel.Description)
                                     .WithColor((Color)Enum.Parse(typeof(Color), viewModel.Color))
                                     .WithCreationType(CreationType.ManuallyCreated)
                                     .WithCategory(category)
                                     .WithLabels(labels)
                                     .WithUserId(UserId)
                                     .WithPriority(finalPriority)
                                     .WithTimeSpan(viewModel.Timespan)
                                     .IsBaseActivity(isBaseActivity);
            result.Repeat = repeat;
            return result;
        }

        private UndefinedActivity CreateNewUndefinedActivityMethod(UndefinedActivityManageViewModel viewModel, bool isBaseActivity)
        {
            var category = _unitOfWork.CategoryRepository.Get(c => c.Name == viewModel.Category).FirstOrDefault();
            var labels = viewModel.Labels?.Select(l => new Label(l, UserId)) ?? new List<Label>();

            var builder = new UndefinedActivityBuilder();
            var result = builder.CreateActivity(viewModel.Name)
                                         .WithDescription(viewModel.Description)
                                         .WithColor((Color)Enum.Parse(typeof(Color), viewModel.Color))
                                         .WithCreationType(CreationType.ManuallyCreated)
                                         .WithCategory(category)
                                         .WithLabels(labels)
                                         .WithUserId(UserId)
                                         .WithMinTime(viewModel.MinimumTime)
                                         .WithMaxTime(viewModel.MaximumTime)
                                         .IsBaseActivity(isBaseActivity);
            return result;
        }

        private DeadlineActivity CreateNewDeadlineActivityMethod(DeadlineActivityManageViewModel viewModel, bool isBaseActivity)
        {
            var milestones = new List<Milestone>();

            if (viewModel.Milestones != null)
            {
                var stringCollection = viewModel.Milestones.Split('|').ToList();
                milestones = stringCollection
                                .Where(m => m != "")
                                .Select(m => new Milestone(
                                                    m.Substring(0, m.IndexOf(';')), 
                                                    DateTime.Parse(m.Substring(m.IndexOf(';') + 1)))).ToList();
            }

            var builder = new DeadlineActivityBuilder();
            var result = builder.CreateActivity(viewModel.Name)
                                         .WithDescription(viewModel.Description)
                                         .WithCreationType(CreationType.ManuallyCreated)
                                         .WithUserId(UserId)
                                         .WithStart(viewModel.StartDate + viewModel.StartTime.TimeOfDay)
                                         .WithEnd(viewModel.EndDate + viewModel.EndTime.TimeOfDay)
                                         .WithMilestones(milestones)
                                         .IsBaseActivity(isBaseActivity);
            return result;
        }
        #endregion
    }
}