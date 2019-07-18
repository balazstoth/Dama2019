using Dama.Data.Enums;
using Dama.Data.Interfaces;
using Dama.Data.Models;
using Dama.Data.Sql.Models;
using Dama.Data.Sql.Repositories;
using Dama.Data.Sql.SQL;
using Dama.Organizer.Enums;
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
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Dama.Web.Manager;
using ActionNames = Dama.Organizer.Enums.ActionNames;
using ControllerNames = Dama.Organizer.Enums.ControllerNames;
using ViewNames = Dama.Organizer.Enums.ViewNames;
using Repeat = Dama.Data.Models.Repeat;

namespace Dama.Web.Controllers
{
    [Authorize]
    [DisableUser]
    public class CalendarController : Controller
    {
        private UserManager<User> _userManager;
        private List<SelectListItem> _colors;
        private readonly string[] _availableColors;
        private readonly IContentRepository _repositories;
        private readonly RepositoryManager _repositoryManager;
        private readonly CalendarControllerManager _calendarControllerManager;

        public CalendarController(IContentRepository contentRepository, RepositoryManager repositoryManager)
        {
            _repositories = contentRepository;
            _repositoryManager = repositoryManager;
            _userManager = new UserManager<User>(new UserStore<User>(new DamaContext(new SqlConfiguration())));
            _colors = new List<SelectListItem>();
            _availableColors = Enum.GetValues(typeof(Color)).Cast<string>().ToArray();
            _colors = _availableColors.Select(c => new SelectListItem() { Text = c.ToString() }).ToList();
            _calendarControllerManager = new CalendarControllerManager(_repositories);
        }

        public string UserId => User.Identity.GetUserId();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetActivities() //TODO: implement correctly
        {
            var fixedActivities = _repositories.FixedActivitySqlRepository.FindByPredicate(x => x.UserId == UserId).ToList();
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
            var categories = _repositories.CategorySqlRepository.FindByPredicate(x => x.UserId == UserId);
            return View(categories);
        }

        public async Task<ActionResult> DeleteCategory(string categoryId)
        {
            var selectedCategory = await _repositories.CategorySqlRepository.FindAsync(categoryId);

            if (selectedCategory == null)
            {
                ViewBag.CategoryNotFoundError = Error.CategoryNotFound;
            }
            else
            {
                RemoveCategoryFromTables(selectedCategory.Id.ToString());
                await _repositories.CategorySqlRepository.RemoveAsync(selectedCategory);
                ViewBag.CategoryRemovedSuccessFully = Success.CategoryRemovedSuccessfully;
            }

            return RedirectToAction(ActionNames.ManageCategories.ToString());
        }

        private void RemoveCategoryFromTables(string categoryId)
        {
            Action<DbSet<FixedActivity>> fixedActivityAction = (activity) =>
            {
                foreach (var record in activity.Include(r => r.Category))
                    if (record.Category != null && record.Category.Id.Equals(categoryId))
                        record.Category = null;
            };
            Action<DbSet<UnfixedActivity>> unfixedActivityAction = (activity) =>
            {
                foreach (var record in activity.Include(r => r.Category))
                    if (record.Category != null && record.Category.Id.Equals(categoryId))
                        record.Category = null;
            };
            Action<DbSet<UndefinedActivity>> undefinedActivityAction = (activity) =>
            {
                foreach (var record in activity.Include(r => r.Category))
                    if (record.Category != null && record.Category.Id.Equals(categoryId))
                        record.Category = null;
            };
            Action<DbSet<DeadlineActivity>> deadlineActivityAction = (activity) =>
            {
                foreach (var record in activity.Include(r => r.Category))
                    if (record.Category != null && record.Category.Id.Equals(categoryId))
                        record.Category = null;
            };

            var dbSetActions = new DbSetAction()
            {
                FixedActivityAction = fixedActivityAction,
                UnfixedActivityAction = unfixedActivityAction,
                UndefinedActivityAction = undefinedActivityAction,
                DeadlineActivityAction = deadlineActivityAction
            };

            _repositoryManager.RemoveCategoryFromDataTables(dbSetActions, ActivityType.FixedActivity);
            _repositoryManager.RemoveCategoryFromDataTables(dbSetActions, ActivityType.UnfixedActivity);
            _repositoryManager.RemoveCategoryFromDataTables(dbSetActions, ActivityType.UndefinedActivity);
            _repositoryManager.RemoveCategoryFromDataTables(dbSetActions, ActivityType.DeadlineActivity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewCategory(AddNewCategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var newCategory = new Category()
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    Color = (Color) Enum.Parse(typeof(Color), viewModel.SelectedColor),
                    Priority = viewModel.Priority,
                    UserId = UserId
                };

                var categoryAlreadyExists = _repositories.CategorySqlRepository
                                                         .FindByPredicate(c => c.UserId == newCategory.UserId && c.Name == newCategory.Name)
                                                         .Any();
                if (categoryAlreadyExists)
                {
                    ViewBag.CategoryAlreadyExists = Error.CategoryAlreadyExists;
                }
                else
                {
                    await _repositories.CategorySqlRepository.AddAsync(newCategory);
                    ViewBag.CategoryCreatedSuccessFully = Success.CategoryCreatedSuccessfully;
                }
            }

            viewModel.Color = _colors;
            return View(viewModel);
        }

        public async Task<ActionResult> EditCategory(string categoryId)
        {
            Category category;
            EditCategoryViewModel viewModel = null;

            category = await _repositories.CategorySqlRepository.FindAsync(categoryId);

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
        public async Task<ActionResult> EditCategory(EditCategoryViewModel viewModel)
        {
            var currentId = viewModel.Id;

            if (ModelState.IsValid)
            {
                var categories = _repositories.CategorySqlRepository.FindByPredicate(c => c.UserId == UserId).ToList();

                foreach (var category in categories)
                {
                    if (category.Name == viewModel.Name && category.Id.ToString() != viewModel.Id)
                    {
                        ViewBag.CategoryAlreadyExists = Error.CategoryAlreadyExists;
                        viewModel.Color = _colors;
                        return View(viewModel);
                    }
                }

                var currentCategory = await _repositories.CategorySqlRepository.FindAsync(currentId);
                currentCategory.Name = viewModel.Name;
                currentCategory.Description = viewModel.Description;
                currentCategory.Priority = viewModel.Priority;
                currentCategory.Color = (Color)Enum.Parse(typeof(Color), viewModel.SelectedColor);
                ViewBag.CategoryChangesSuccessfully = Success.CategoryChangesSuccessfully;
            }

            viewModel.Color = _colors;
            return View(viewModel);
        }
        #endregion

        #region Label
        public ActionResult AddNewLabel()
        {
            return View();
        }

        public ActionResult ManageLabels()
        {
            var labels = _repositories.LabelSqlRepository
                        .FindByPredicate(l => l.UserId == UserId)
                        .GroupBy(l => l.Name)
                        .Select(l => l.First());

            return View(labels);
        }

        public async Task<ActionResult> DeleteLabel(string labelId)
        {
            var selectedLabel = await _repositories.LabelSqlRepository.FindAsync(labelId);

            if (selectedLabel == null)
            {
                ViewBag.LabelNotFoundError = Error.LabelNotFound;
            }
            else
            {
                var itemsToRemove = _repositories.LabelSqlRepository
                                                  .FindByPredicate(l => l.Name == selectedLabel.Name)
                                                  .ToList();

                await _repositories.LabelSqlRepository.RemoveRangeAsync(itemsToRemove);
            }

            ViewBag.LabelRemovedSuccessFully = Success.LabelRemovedSuccessfully;
            return RedirectToAction(ActionNames.ManageLabels.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewLabel(AddNewLabelViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var newLabel = new Label(viewModel.Name, UserId);
               
                var labelAlreadyExists = _repositories.LabelSqlRepository
                                                    .FindByPredicate(l => l.UserId == newLabel.UserId && l.Name == newLabel.Name)
                                                    .Any();

                if(labelAlreadyExists)
                {
                    ViewBag.LabelAlreadyExists = Error.LabelAlreadyExists;
                    return View();
                }

                await _repositories.LabelSqlRepository.AddAsync(newLabel);

                ViewBag.LabelCreatedSuccessFully = Success.LabelCreatedSuccessfully;
            }

            return View();
        }
        #endregion

        #region Activity
        /// <param name="categoryId"> If the Id is not 0, it is managed as sorted by category</param>
        public async Task<ActionResult> ManageActivities(int categoryId = -1)

        {
            Predicate<Activity> predicate;
            var sortedByCategory = categoryId != -1;

            if (sortedByCategory)
            {
                predicate = a => a.UserId == UserId &&
                                 a.CreationType == CreationType.ManuallyCreated &&
                                 a.Category.Id == categoryId;
            }
            else
            {
                predicate = a => a.UserId == UserId &&
                                 a.CreationType == CreationType.ManuallyCreated;
            }

            var fixedActivities = await _repositories.FixedActivitySqlRepository
                                                        .FindByExpressionAsync(t => t
                                                            .Where(a => predicate(a))
                                                            .Include(a => a.LabelCollection)
                                                            .Include(a => a.Category)
                                                            .OrderBy(a => a.Name).ToListAsync());

            var unfixedActivities = await _repositories.UnfixedActivitySqlRepository
                                                        .FindByExpressionAsync(t => t
                                                            .Where(a => predicate(a))
                                                            .Include(a => a.LabelCollection)
                                                            .Include(a => a.Category)
                                                            .OrderBy(a => a.Name).ToListAsync());

            var undefinedActivities = await _repositories.UndefinedActivitySqlRepository
                                                        .FindByExpressionAsync(t => t
                                                            .Where(a => predicate(a))
                                                            .Include(a => a.LabelCollection)
                                                            .Include(a => a.Category)
                                                            .OrderBy(a => a.Name).ToListAsync());

            var deadlineActivities = await _repositories.DeadlineActivitySqlRepository
                                                        .FindByExpressionAsync(t => t
                                                            .Where(a => predicate(a))
                                                            .Include(a => a.LabelCollection)
                                                            .Include(a => a.Category)
                                                            .Include(a => a.Milestones)
                                                            .OrderBy(a => a.Name).ToListAsync());

            var container = new ViewModelContainer()
            {
                FixedActivityViewModel = new FixedActivityViewModel() { FixedActivityCollection = fixedActivities },
                UnfixedActivityViewModel = new UnfixedActivityViewModel() { UnfixedActivityCollection = unfixedActivities },
                UndefinedActivityViewModel = new UndefinedActivityViewModel() { UndefinedActivityCollection = undefinedActivities },
                DeadlineActivityViewModel = new DeadlineActivityViewModel() { DeadlineActivityCollection = deadlineActivities }
            };
            
            if (sortedByCategory)
                return View(ViewNames.ListSortedByCategoryActivities.ToString(), container);
            else
                return View(ViewNames.ManageActivities.ToString(), container);
        }

        public async Task<ActionResult> ActivityDetails(string activityId, ActivityType activityType)
        {
            var id = int.Parse(activityId);
            
            switch (activityType)
            {
                case ActivityType.FixedActivity:
                    var fixedActivities = await _repositories.FixedActivitySqlRepository.FindByExpressionAsync(
                                            t => t
                                                .Where(a => a.Id == id)
                                                .Include(a => a.LabelCollection)
                                                .Include(a => a.Category)
                                                .ToListAsync());

                    var fixedActivityModel = new FixedActivityViewModel() { FixedActivityCollection = fixedActivities };
                    return View(ViewNames.FixedActivityDetails.ToString(), fixedActivityModel);

                case ActivityType.UnfixedActivity:
                    var unfixedActivities = await _repositories.UnfixedActivitySqlRepository.FindByExpressionAsync(
                                              t => t
                                                .Where(a => a.Id == id)
                                                .Include(a => a.LabelCollection)
                                                .Include(a => a.Category)
                                                .ToListAsync());

                    var unfixedActivityModel = new UnfixedActivityViewModel() { UnfixedActivityCollection = unfixedActivities };
                    return View(ViewNames.UnfixedActivityDetails.ToString(), unfixedActivityModel);

                case ActivityType.UndefinedActivity:
                    var undefinedActivities = await _repositories.UndefinedActivitySqlRepository.FindByExpressionAsync(
                                               t => t
                                                .Where(a => a.Id == id)
                                                .Include(a => a.LabelCollection)
                                                .Include(a => a.Category)
                                                .ToListAsync());

                    var undefinedActivityModel = new UndefinedActivityViewModel() { UndefinedActivityCollection = undefinedActivities };
                    return View(ViewNames.UndefinedActivityDetails.ToString(), undefinedActivityModel);

                case ActivityType.DeadlineActivity:
                    var deadlineActivities = await _repositories.DeadlineActivitySqlRepository.FindByExpressionAsync(
                                               t => t
                                                .Where(a => a.Id == id)
                                                .Include(a => a.LabelCollection)
                                                .Include(a => a.Category)
                                                .Include(a => a.Milestones)
                                                .ToListAsync());

                    var deadlineActivityModel = new DeadlineActivityViewModel() { DeadlineActivityCollection = deadlineActivities };
                    return View(ViewNames.DeadlineActivityDetails.ToString(), deadlineActivityModel);

                default:
                    return RedirectToAction(ActionNames.ManageActivities.ToString());
            }
        }

        public async Task<ActionResult> DeleteActivity(string activityId, ActivityType activityType)
        {
            int id = int.Parse(activityId);
            switch (activityType)
            {
                case ActivityType.FixedActivity:
                    var fixedActivity = (await _repositories.FixedActivitySqlRepository.FindByExpressionAsync(
                                                    t => t
                                                        .Where(a => a.Id == id)
                                                        .Include(a => a.LabelCollection)
                                                        .Include(a => a.Category)
                                                        .ToListAsync())).FirstOrDefault();

                    if (fixedActivity != null)
                    {
                        if (fixedActivity.LabelCollection != null)
                            await _repositories.LabelSqlRepository.RemoveRangeAsync(fixedActivity.LabelCollection);

                        //if (a.Category != null)
                        //    db.Categories.Remove(a.Category);

                        fixedActivity.Category = null;
                        await _repositories.FixedActivitySqlRepository.RemoveAsync(fixedActivity);
                    }
                    break;

                case ActivityType.UnfixedActivity:
                    var unfixedActivity = (await _repositories.UnfixedActivitySqlRepository.FindByExpressionAsync(
                                                    t => t
                                                        .Where(a => a.Id == id)
                                                        .Include(a => a.LabelCollection)
                                                        .Include(a => a.Category)
                                                        .ToListAsync())).FirstOrDefault();
                    if (unfixedActivity != null)
                    {
                        if (unfixedActivity.LabelCollection != null)
                           await  _repositories.LabelSqlRepository.RemoveRangeAsync(unfixedActivity.LabelCollection);

                        unfixedActivity.Category = null;
                        await _repositories.UnfixedActivitySqlRepository.RemoveAsync(unfixedActivity);
                        ViewBag.ActivityRemovedSuccessfully = Success.ActivityRemovedSuccessfully;
                    }
                    break;

                case ActivityType.UndefinedActivity:
                    var undefinedActivity = (await _repositories.UndefinedActivitySqlRepository.FindByExpressionAsync(
                                                    t => t
                                                        .Where(a => a.Id == id)
                                                        .Include(a => a.LabelCollection)
                                                        .Include(a => a.Category)
                                                        .ToListAsync())).FirstOrDefault();
                    if (undefinedActivity != null)
                    {
                        if (undefinedActivity.LabelCollection != null)
                            await _repositories.LabelSqlRepository.RemoveRangeAsync(undefinedActivity.LabelCollection);

                        undefinedActivity.Category = null;
                        await _repositories.UndefinedActivitySqlRepository.RemoveAsync(undefinedActivity);
                        ViewBag.ActivityRemovedSuccessfully = Success.ActivityRemovedSuccessfully;
                    }
                    break;

                case ActivityType.DeadlineActivity:
                    var deadlineActivity = (await _repositories.DeadlineActivitySqlRepository.FindByExpressionAsync(
                                                    t => t
                                                        .Where(a => a.Id == id)
                                                        .Include(a => a.LabelCollection)
                                                        .Include(a => a.Category)
                                                        .Include(a => a.Milestones)
                                                        .ToListAsync())).FirstOrDefault();
                    if (deadlineActivity != null)
                    {
                        if (deadlineActivity.LabelCollection != null)
                            await _repositories.LabelSqlRepository.RemoveRangeAsync(deadlineActivity.LabelCollection);

                        deadlineActivity.Category = null;

                        if (deadlineActivity.Milestones != null)
                            await _repositories.MilestoneSqlRepository.RemoveRangeAsync(deadlineActivity.Milestones);

                        await _repositories.DeadlineActivitySqlRepository.RemoveAsync(deadlineActivity);
                        ViewBag.ActivityRemovedSuccessFully = Success.ActivityRemovedSuccessfully;
                    }
                    break;
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        public async Task<ActionResult> AddNewActivity()
        {
            var colors = new List<SelectListItem>(_colors);
            var categories = await AddCategoriesToProcessAsyc(UserId);
            var labels = await AddLabelsToProcessAsync(UserId);
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

        public async Task<List<SelectListItem>> AddCategoriesToProcessAsyc(string userId)
        {
            var categorySelectItemList = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = "Uncategorized",
                    Value = null
                }
            };
            var categories = await _repositories
                                        .CategorySqlRepository
                                        .FindByExpressionAsync(
                                            t => t.Where(c => c.UserId == userId)
                                                  .ToListAsync());

            categorySelectItemList.AddRange(categories.Select(c => new SelectListItem() { Text = c.Name }));

            return categorySelectItemList;
        }

        public async Task<List<SelectListItem>> AddLabelsToProcessAsync(string userId)
        {
            List<SelectListItem> LabelList = new List<SelectListItem>();

            var labels = await _repositories
                                .LabelSqlRepository
                                .FindByExpressionAsync(
                                    t => t
                                          .Where(l => l.UserId == userId)
                                          .Distinct()
                                          .ToListAsync());

            var labelSelectItems = labels.Select(l => new SelectListItem() { Text = l.Name }).ToList();
            return labelSelectItems;
        }

        public async Task<ActionResult> EditActivity(string id, ActivityType? activityType, bool calledFromEditor = false, bool optional = false)
        {
            var details = new EditDetails()
            {
                ActivityId = int.Parse(id),
                ActivityType = activityType,
                CalledFromEditor = calledFromEditor,
                IsOptional = optional,
                Categories = new List<SelectListItem>(_colors),
                Colors = new List<SelectListItem>(_colors),
                Labels = await AddLabelsToProcessAsync(UserId),
                RepeatTypes = AddRepeatTypeToProcess(),
            };
            var path = "../Calendar";

            switch (activityType)
            {
                case ActivityType.FixedActivity:
                    {
                        var fixedActivityViewModel = await _calendarControllerManager.AssembleFixedActivityManageViewModelAsync(details);

                        if (calledFromEditor)
                            return PartialView(Path.Combine(path, ViewNames.EditFixedActivity.ToString()), fixedActivityViewModel);

                        return View(ViewNames.EditFixedActivity.ToString(), fixedActivityViewModel);
                    }

                case ActivityType.UnfixedActivity:
                    {
                        var unfixedActivityViewModel = await _calendarControllerManager.AssembleUnfixedActivityViewModelAsync(details);

                        if (calledFromEditor)
                            return PartialView(Path.Combine(path, ViewNames.EditUnfixedActivity.ToString()), unfixedActivityViewModel);

                        return View(ViewNames.EditUnfixedActivity.ToString(), unfixedActivityViewModel);
                    }

                case ActivityType.UndefinedActivity:
                    {
                        var undefinedActivityViewModel = await _calendarControllerManager.AssembleUndefinedActivityViewModelAsync(details);

                        if (calledFromEditor)
                            return PartialView(Path.Combine(path, ViewNames.EditUndefinedActivity.ToString()), undefinedActivityViewModel);

                        return View(ViewNames.EditUndefinedActivity.ToString(), undefinedActivityViewModel);
                    }

                case ActivityType.DeadlineActivity:
                    {
                        var deadlineActivityViewModel = await _calendarControllerManager.AssembleDeadlineActivityViewModelAsync(details);

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
        public async Task<ActionResult> AddNewActivity(ViewModelManagerContainer container)
        {
            return await CreateNewActivityFromTuple(container);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditFixedActivity(FixedActivityManageViewModel viewModel)
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
                            editedActivity = editedActivity + CreateNewFixedActivityMethod(viewModel, false);
                        }
                        else
                        {
                            var editedActivity = container.ActivitySelectedByUserForSure
                                                                .FirstOrDefault(a => a.Id == activityId &&
                                                                                     a.ActivityType == ActivityType.FixedActivity) as FixedActivity;
                            editedActivity = editedActivity + CreateNewFixedActivityMethod(viewModel, false);
                        }
                    }

                    return RedirectToAction(ActionNames.Editor.ToString(), ControllerNames.CalendarEditor.ToString());
                }
                else
                {
                    var activity = (await _repositories.FixedActivitySqlRepository
                                                            .FindByExpressionAsync(t => t
                                                                .Where(a => a.Id == activityId)
                                                                .Include(a => a.LabelCollection)
                                                                .Include(a => a.Category).ToListAsync()))
                                                                    .FirstOrDefault();
                    if (activity != null)
                    {
                        if (activity.LabelCollection != null)
                            await _repositories.LabelSqlRepository.RemoveRangeAsync(activity.LabelCollection);

                        //if (a.Category != null)
                        //db.Categories.Remove(a.Category);

                        activity.Category = null;
                        await _repositories.FixedActivitySqlRepository.RemoveAsync(activity);
                    }
                }

                var viewModelContainer = new ViewModelManagerContainer() { FixedActivityManageViewModel = viewModel };
                await CreateNewActivityFromTuple(viewModelContainer);
            }
            else
            {
                viewModel.LabelSourceCollection = await AddLabelsToProcessAsync(UserId);
                viewModel.CategorySourceCollection = await AddCategoriesToProcessAsyc(UserId);
                viewModel.RepeatTypeSourceCollection = AddRepeatTypeToProcess();
                viewModel.ColorSourceCollection = _colors;

                return View(viewModel);
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUnfixedActivity(UnfixedActivityManageViewModel viewModel)
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
                            editedActivity = editedActivity + CreateNewUnfixedActivityMethod(viewModel, false);
                        }
                        else
                        {
                            var editedActivity = container.ActivitySelectedByUserForSure
                                                                .FirstOrDefault(a => a.Id == activityId &&
                                                                                        a.ActivityType == ActivityType.UnfixedActivity) as UnfixedActivity;
                            editedActivity = editedActivity + CreateNewUnfixedActivityMethod(viewModel, false);
                        }
                    }

                    return RedirectToAction(ActionNames.Editor.ToString(), ControllerNames.CalendarEditor.ToString());
                }
                else
                {
                    var activity = (await _repositories.UnfixedActivitySqlRepository
                                                           .FindByExpressionAsync(t => t
                                                               .Where(a => a.Id == activityId)
                                                               .Include(a => a.LabelCollection)
                                                               .Include(a => a.Category).ToListAsync()))
                                                                   .FirstOrDefault();
                    if (activity != null)
                    {
                        if (activity.LabelCollection != null)
                            await _repositories.LabelSqlRepository.RemoveRangeAsync(activity.LabelCollection);

                        activity.Category = null;
                        await _repositories.UnfixedActivitySqlRepository.RemoveAsync(activity);
                    }
                }

                var viewModelContainer = new ViewModelManagerContainer() { UnfixedActivityManageViewModel = viewModel };
                await CreateNewActivityFromTuple(viewModelContainer);
            }
            else
            {
                viewModel.LabelSourceCollection = await AddLabelsToProcessAsync(UserId);
                viewModel.CategorySourceCollection = await AddCategoriesToProcessAsyc(UserId);
                viewModel.RepeatTypeSourceCollection = AddRepeatTypeToProcess();
                viewModel.ColorSourceCollection = _colors;

                return View(viewModel);
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUndefinedActivity(UndefinedActivityManageViewModel viewModel)
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

                        editedActivity = editedActivity + CreateNewUndefinedActivityMethod(viewModel, false);
                    }

                    return RedirectToAction(ActionNames.Editor.ToString(), ControllerNames.CalendarEditor.ToString());
                }
                else
                {
                    //var activity = await db.UndefinedActivities.Where(x => x.ID == activityId).Include(x => x.Labels).Include(act => act.Category).FirstOrDefaultAsync();

                    var activity = (await _repositories.UndefinedActivitySqlRepository
                                                           .FindByExpressionAsync(t => t
                                                                .Where(a => a.Id == activityId)
                                                                .Include(a => a.LabelCollection)
                                                                .Include(a => a.Category).ToListAsync()))
                                                                    .FirstOrDefault();
                    if (activity != null)
                    {
                        if (activity.LabelCollection != null)
                            await _repositories.LabelSqlRepository.RemoveRangeAsync(activity.LabelCollection);

                        activity.Category = null;
                        await _repositories.UndefinedActivitySqlRepository.RemoveAsync(activity);
                    }
                }

                var viewModelContainer = new ViewModelManagerContainer() { UndefinedActivityManageViewModel = viewModel };

                await CreateNewActivityFromTuple(viewModelContainer);
            }
            else
            {
                viewModel.LabelSourceCollection = await AddLabelsToProcessAsync(UserId);
                viewModel.CategorySourceCollection = await AddCategoriesToProcessAsyc(UserId);
                viewModel.ColorSourceCollection = _colors;

                return View(viewModel);
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDeadlineActivity(DeadlineActivityManageViewModel viewModel)
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

                        editedActivity = editedActivity + CreateNewDeadlineActivityMethod(viewModel, false);
                    }

                    return RedirectToAction(ActionNames.Editor.ToString(), ControllerNames.CalendarEditor.ToString());
                }
                else
                {
                    var activity = (await _repositories.DeadlineActivitySqlRepository
                                                            .FindByExpressionAsync(t => t
                                                                .Where(a => a.Id == activityId)
                                                                .Include(a => a.LabelCollection)
                                                                .Include(a => a.Category)
                                                                .Include(a => a.Milestones).ToListAsync()))
                                                                    .FirstOrDefault();
                    if (activity != null)
                    {
                        if (activity.LabelCollection != null)
                            await _repositories.LabelSqlRepository.RemoveRangeAsync(activity.LabelCollection);

                        activity.Category = null;

                        if (activity.Milestones != null)
                            await _repositories.MilestoneSqlRepository.RemoveRangeAsync(activity.Milestones);

                        await _repositories.DeadlineActivitySqlRepository.RemoveAsync(activity);
                        ViewBag.ActivityRemovedSuccessFully = Success.ActivityRemovedSuccessfully;
                    }
                }

                var viewModelContainer = new ViewModelManagerContainer() { DeadlineActivityManageViewModel = viewModel };
                await CreateNewActivityFromTuple(viewModelContainer);
            }
            else
            {
                viewModel.ColorSourceCollection = _colors;
                return View(viewModel);
            }

            return RedirectToAction(ActionNames.ManageActivities.ToString());
        } 

        private async Task<ActionResult> CreateNewActivityFromTuple(ViewModelManagerContainer modelManagerContainer)
        {
            if (modelManagerContainer.FixedActivityManageViewModel != null)
            {
                var fixedActivityViewModel = modelManagerContainer.FixedActivityManageViewModel;

                if (ModelState.IsValid)
                {
                    var fixedActivity = CreateNewFixedActivityMethod(fixedActivityViewModel, CreationType.ManuallyCreated);

                    if (fixedActivity.Category != null)
                        _repositories.RepositorySettings.ChangeCategoryEntryState(fixedActivity.Category, EntityState.Unchanged);

                    await _repositories.FixedActivitySqlRepository.AddAsync(fixedActivity);

                    return RedirectToAction(ActionNames.ManageActivities.ToString());
                }
                else
                {
                    fixedActivityViewModel.LabelSourceCollection = await AddLabelsToProcessAsync(UserId);
                    fixedActivityViewModel.CategorySourceCollection = await AddCategoriesToProcessAsyc(UserId);
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
                    var unfixedActivity = CreateNewUnfixedActivityMethod(unfixedActivityViewModel, CreationType.ManuallyCreated);

                    if (unfixedActivity.Category != null)
                        _repositories.RepositorySettings.ChangeCategoryEntryState(unfixedActivity.Category, EntityState.Unchanged);

                    await _repositories.UnfixedActivitySqlRepository.AddAsync(unfixedActivity);

                    return RedirectToAction(ActionNames.ManageActivities.ToString());
                }
                else
                {
                    unfixedActivityViewModel.LabelSourceCollection = await AddLabelsToProcessAsync(UserId);
                    unfixedActivityViewModel.CategorySourceCollection = await AddCategoriesToProcessAsyc(UserId);
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
                    var undefinedActivity = CreateNewUndefinedActivityMethod(undefinedActivityViewModel, CreationType.ManuallyCreated);

                    if (undefinedActivity.Category != null)
                        _repositories.RepositorySettings.ChangeCategoryEntryState(undefinedActivity.Category, EntityState.Unchanged);

                    await _repositories.UndefinedActivitySqlRepository.AddAsync(undefinedActivity);
                    return RedirectToAction(ActionNames.ManageActivities.ToString());
                }
                else
                {
                    undefinedActivityViewModel.LabelSourceCollection = await AddLabelsToProcessAsync(UserId);
                    undefinedActivityViewModel.CategorySourceCollection = await AddCategoriesToProcessAsyc(UserId);
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
                    var deadlineActivity = CreateNewDeadlineActivityMethod(deadlineActivityViewModel, CreationType.ManuallyCreated);
                    await _repositories.DeadlineActivitySqlRepository.AddAsync(deadlineActivity);
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

        private FixedActivity CreateNewFixedActivityMethod(FixedActivityManageViewModel viewModel, CreationType creationType)
        {
            Repeat repeat = null;
            int finalPriority;
            var start = DateTime.Today + viewModel.StartTime.TimeOfDay;
            var end = DateTime.Today + viewModel.EndTime.TimeOfDay;
            var category = _repositories.CategorySqlRepository.FindByPredicate(c => c.Name == viewModel.Category).FirstOrDefault();
            var labels = viewModel.Labels?.Select(l => new Label(l, UserId)).ToList();

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
                                            .WithEnd(end);
            return result;
        }

        private UnfixedActivity CreateNewUnfixedActivityMethod(UnfixedActivityManageViewModel viewModel, CreationType creationType)
        {
            int finalPriority;
            Repeat repeat = null;

            var category = _repositories.CategorySqlRepository.FindByPredicate(c => c.Name == viewModel.Category).FirstOrDefault();
            var labels = viewModel.Labels?.Select(l => new Label(l, UserId));

            if (viewModel.Priority == 0)
                finalPriority = category == null ? 1 : category.Priority;
            else
                finalPriority = viewModel.Priority;

            if (viewModel.RepeatType != null && viewModel.RepeatEndDate != null)
                repeat = new Repeat((RepeatPeriod)Enum.Parse(typeof(RepeatPeriod), viewModel.RepeatType), viewModel.RepeatEndDate);

            var builder = new UnfixedActivityBuilder();
            var result = builder.CreateActivity(viewModel.Name)
                                 .WithDescription(viewModel.Description)
                                 .WithColor((Color)Enum.Parse(typeof(Color), viewModel.Color))
                                 .WithCreationType(CreationType.ManuallyCreated)
                                 .WithCategory(category)
                                 .WithUserId(UserId)
                                 .WithPriority(finalPriority)
                                 .WithTimeSpan(viewModel.Timespan);

            return result;
        }

        private UndefinedActivity CreateNewUndefinedActivityMethod(UndefinedActivityAddOrEditViewModel vm, string userid, bool Base)
        {
            Category category;
            List<Label> labels = new List<Label>();
            using (DamaDB db = new DamaDB())
            {
                category = db.Categories.Where(x => x.Name == vm.Category).FirstOrDefault();
                if (vm.Labels != null)
                {
                    foreach (string i in vm.Labels)
                    {
                        labels.Add(new Label(i, userid));
                    }
                }

                return new UndefinedActivity(
                                            vm.Name,
                                            vm.Description,
                                            vm.Color,
                                            false,
                                            Base,
                                            labels,
                                            category,
                                            userid,
                                            vm.MinimumTime,
                                            vm.MaximumTime);
            }
        }

        private DeadlineActivity CreateNewDeadlineActivityMethod(DeadlineActivityAddOrEditViewModel vm, string userid, bool Base)
        {
            List<MileStone> mileStoneList = new List<MileStone>();

            if (vm.Milestones != null)
            {
                List<string> mileStoneStringList = vm.Milestones.Split('|').ToList();
                foreach (string i in mileStoneStringList)
                {
                    if (i != "")
                        mileStoneList.Add(new MileStone(i.Substring(0, i.IndexOf(';')), DateTime.Parse(i.Substring(i.IndexOf(';') + 1))));
                }
            }

            return new DeadlineActivity(
                                        vm.Name,
                                        vm.Description,
                                        vm.Color,
                                        false,
                                        Base,
                                        userid,
                                        vm.StartDate + vm.StartTime.TimeOfDay,
                                        vm.EndDate + vm.EndTime.TimeOfDay,
                                        mileStoneList);
        }
        #endregion
    }
}