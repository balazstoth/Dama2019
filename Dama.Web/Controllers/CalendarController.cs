using Dama.Data.Enums;
using Dama.Data.Interfaces;
using Dama.Data.Models;
using Dama.Data.Sql.Models;
using Dama.Data.Sql.Repositories;
using Dama.Data.Sql.SQL;
using Dama.Organizer.Enums;
using Dama.Organizer.Resources;
using Dama.Web.Attributes;
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
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using ActionNames = Dama.Organizer.Enums.ActionNames;
using ControllerNames = Dama.Organizer.Enums.ControllerNames;
using ViewNames = Dama.Organizer.Enums.ViewNames;

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

        public CalendarController(IContentRepository contentRepository, RepositoryManager repositoryManager)
        {
            _repositories = contentRepository;
            _repositoryManager = repositoryManager;
            _userManager = new UserManager<User>(new UserStore<User>(new DamaContext(new SqlConfiguration())));
            _colors = new List<SelectListItem>();
            _availableColors = Enum.GetValues(typeof(Color)).Cast<string>().ToArray();
            _colors = _availableColors.Select(c => new SelectListItem() { Text = c.ToString() }).ToList();
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
                    if (category.Name == viewModel.Name && category.Id != viewModel.Id)
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
            var categories = await GetAllCategoriesToAddProcessAsyc(UserId);
            var labels = GetAllLabelsToAddProcess(UserId);
            var repeatTypeList = GetRepeatTypeToAddProcess();

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
                                          .Where(l => l.UserId == UserId)
                                          .Distinct()
                                          .ToListAsync());

            var labelSelectItems = labels.Select(l => new SelectListItem() { Text = l.Name }).ToList();
            return labelSelectItems;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewActivity(ViewModelManagerContainer container)
        {
            return await CreateNewActivityFromTuple(container);
        }

        public async Task<ActionResult> EditActivity(string id, ActivityType? type, bool calledFromEditor = false, bool optional = false)
        {
            int IDParam = int.Parse(id);
            string currentUserID = User.Identity.GetUserId();
            List<SelectListItem> ColorList = new List<SelectListItem>(this._colors);
            List<SelectListItem> CategoryList = new List<SelectListItem>();
            //CategoryList.Add(new SelectListItem() { Text = "No category", Value = null });
            CategoryList.AddRange(await GetAllCategoriesToAddProcessAsyc(currentUserID));
            List<SelectListItem> RepeatTypeList = GetRepeatTypeToAddProcess();
            bool? isOptional = null;
            List<SelectListItem> LabelList = GetAllLabelsToAddProcess(currentUserID);

            using (DamaDB db = new DamaDB())
            {
                switch (type)
                {
                    case ActivityType.Fixed:
                        FixedActivity fixedActivity;
                        if (calledFromEditor)
                        {
                            if (optional)
                            {
                                isOptional = true;
                                fixedActivity = Cont.ActivitySelectedByUserForOptional.FirstOrDefault(x => x.ID == IDParam && x.OwnType == ActivityType.Fixed) as FixedActivity;
                            }
                            else
                            {
                                isOptional = false;
                                fixedActivity = Cont.ActivitySelectedByUserForSure.FirstOrDefault(x => x.ID == IDParam && x.OwnType == ActivityType.Fixed) as FixedActivity;
                            }
                        }
                        else
                        {
                            fixedActivity = await db.FixedActivities.Include(i => i.Category).Include(i => i.Labels).FirstOrDefaultAsync(i => i.ID == IDParam && i.Base == true);
                        }
                        FixedActivityAddOrEditViewModel vmFixed = new FixedActivityAddOrEditViewModel()
                        {
                            Id = id,
                            Category = fixedActivity.Category != null ? fixedActivity.Category.ToString() : null,
                            CategoryList = CategoryList,
                            Color = fixedActivity.Color,
                            ColorList = ColorList,
                            Description = fixedActivity.Description,
                            EndTime = fixedActivity.End,
                            LabelList = LabelList,
                            Labels = fixedActivity.Labels.Select(x => x.Name).ToList(),
                            Name = fixedActivity.Name,
                            Priority = fixedActivity.Priority,
                            StartTime = fixedActivity.Start,
                            RepeatTypeList = RepeatTypeList,
                            enableRepeatChange = calledFromEditor,
                            RepeatType = fixedActivity.Repeat == null ? null : fixedActivity.Repeat.RepeatPeriod.ToString(),
                            RepeatEndDate = fixedActivity.Repeat == null ? DateTime.Today.AddDays(7) : fixedActivity.Repeat.EndDate,
                            IsOptional = isOptional
                        };

                        if (calledFromEditor)
                            return PartialView("../Calendar/EditFixedActivity", vmFixed);
                        return View("EditFixedActivity", vmFixed);


                    case ActivityType.Undefined:
                        UndefinedActivity undefinedActivity;
                        if (calledFromEditor)
                        {
                            undefinedActivity = Cont.ActivitySelectedByUserForOptional.FirstOrDefault(x => x.ID == IDParam && x.OwnType == ActivityType.Undefined) as UndefinedActivity;
                        }
                        else
                        {
                            undefinedActivity = await db.UndefinedActivities.Include(i => i.Category).Include(i => i.Labels).FirstOrDefaultAsync(i => i.ID == IDParam && i.Base == true);
                        }
                        UndefinedActivityAddOrEditViewModel vm = new UndefinedActivityAddOrEditViewModel()
                        {
                            Id = id,
                            Category = undefinedActivity.Category != null ? undefinedActivity.Category.ToString() : null,
                            CategoryList = CategoryList,
                            Color = undefinedActivity.Color,
                            ColorList = ColorList,
                            Description = undefinedActivity.Description,
                            LabelList = LabelList,
                            Labels = undefinedActivity.Labels.Select(x => x.Name).ToList(),
                            Name = undefinedActivity.Name,
                            CalledFromEditor = calledFromEditor,
                            MaximumTime = undefinedActivity.MaximumTime,
                            MinimumTime = undefinedActivity.MinimumTime
                        };
                        if (calledFromEditor)
                            return PartialView("../Calendar/EditUndefinedActivity", vm);
                        return View("EditUndefinedActivity", vm);

                    case ActivityType.Unfixed:
                        UnfixedActivity unfixedActivity;
                        if (calledFromEditor)
                        {
                            if (optional)
                            {
                                isOptional = true;
                                unfixedActivity = Cont.ActivitySelectedByUserForOptional.FirstOrDefault(x => x.ID == IDParam && x.OwnType == ActivityType.Unfixed) as UnfixedActivity;
                            }
                            else
                            {
                                isOptional = false;
                                unfixedActivity = Cont.ActivitySelectedByUserForSure.FirstOrDefault(x => x.ID == IDParam && x.OwnType == ActivityType.Unfixed) as UnfixedActivity;
                            }
                        }
                        else
                        {
                            unfixedActivity = await db.UnFixedActivities.Include(i => i.Category).Include(i => i.Labels).FirstOrDefaultAsync(i => i.ID == IDParam && i.Base == true);
                        }
                        UnfixedActivityAddOrEditViewModel vmUnfixed = new UnfixedActivityAddOrEditViewModel()
                        {
                            Id = id,
                            Category = unfixedActivity.Category != null ? unfixedActivity.Category.ToString() : null,
                            CategoryList = CategoryList,
                            Color = unfixedActivity.Color,
                            ColorList = ColorList,
                            Description = unfixedActivity.Description,
                            LabelList = LabelList,
                            Labels = unfixedActivity.Labels.Select(x => x.Name).ToList(),
                            Name = unfixedActivity.Name,
                            Priority = unfixedActivity.Priority,
                            Timespan = unfixedActivity.TimeSpan,
                            RepeatTypeList = RepeatTypeList,
                            enableRepeatChange = calledFromEditor,
                            RepeatType = unfixedActivity.Repeat == null ? null : unfixedActivity.Repeat.RepeatPeriod.ToString(),
                            RepeatEndDate = unfixedActivity.Repeat == null ? DateTime.Today.AddDays(7) : unfixedActivity.Repeat.EndDate,
                            IsOptional = isOptional
                        };

                        if (calledFromEditor)
                            return PartialView("../Calendar/EditUnfixedActivity", vmUnfixed);
                        return View("EditUnfixedActivity", vmUnfixed);


                    case ActivityType.Deadline:
                        DeadlineActivity deadlineActivity;
                        if (calledFromEditor)
                        {
                            deadlineActivity = Cont.ActivitySelectedByUserForSure.FirstOrDefault(x => x.ID == IDParam && x.OwnType == ActivityType.Deadline) as DeadlineActivity;
                        }
                        else
                        {
                            deadlineActivity = await db.DeadLineActivities.Include(i => i.MileStones).FirstOrDefaultAsync(i => i.ID == IDParam && i.Base == true);
                        }

                        string ms = "";
                        foreach (MileStone i in deadlineActivity.MileStones)
                            ms += i.Name + ";" + i.Time + "|";

                        DeadlineActivityAddOrEditViewModel vmDeadline = new DeadlineActivityAddOrEditViewModel()
                        {
                            Id = id,
                            Description = deadlineActivity.Description,
                            Name = deadlineActivity.Name,
                            EndDate = deadlineActivity.End.Date,
                            StartDate = deadlineActivity.Start.Date,
                            EndTime = deadlineActivity.End,
                            StartTime = deadlineActivity.Start,
                            Milestones = ms,
                            CalledFromEditor = calledFromEditor
                        };

                        if (calledFromEditor)
                            return View("../Calendar/EditDeadlineActivity", vmDeadline);

                        return View("EditDeadlineActivity", vmDeadline);
                }
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditFixedActivity(FixedActivityAddOrEditViewModel vm)
        {
            bool changedFromEditor = vm.enableRepeatChange;
            string userid = User.Identity.GetUserId();
            int activityID = int.Parse(Request.Form["id"]); //Get the ID from "view"
            if (ModelState.IsValid)
            {
                using (DamaDB db = new DamaDB())
                {
                    if (changedFromEditor) //If it's true, no need to change the database
                    {
                        Cont.Reset = false;
                        if (vm.IsOptional == true)
                        {
                            FixedActivity edited = Cont.ActivitySelectedByUserForOptional.FirstOrDefault(x => x.ID == activityID && x.OwnType == ActivityType.Fixed) as FixedActivity;
                            edited = edited + CreateNewFixedActivityMethod(vm, userid, false);
                        }
                        else
                        {
                            FixedActivity edited = Cont.ActivitySelectedByUserForSure.FirstOrDefault(x => x.ID == activityID && x.OwnType == ActivityType.Fixed) as FixedActivity;
                            edited = edited + CreateNewFixedActivityMethod(vm, userid, false);
                        }
                        return RedirectToAction("Editor", "CalendarEditor");
                    }
                    else
                    {
                        FixedActivity cActivity = await db.FixedActivities.Where(x => x.ID == activityID).Include(x => x.Labels).Include(act => act.Category).FirstOrDefaultAsync();
                        if (cActivity != null)
                        {
                            if (cActivity.Labels != null)
                                db.Labels.RemoveRange(cActivity.Labels);
                            //if (a.Category != null)
                            //db.Categories.Remove(a.Category);
                            cActivity.Category = null;
                            db.FixedActivities.Remove(cActivity);
                            db.SaveChanges();
                        }
                    }
                }
                await CreateNewActivityFromTuple(new MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel>() { Item1 = vm });
            }
            else
            {
                vm.LabelList = GetAllLabelsToAddProcess(userid);
                vm.ColorList = _colors;
                vm.CategoryList = await GetAllCategoriesToAddProcessAsyc(userid);
                vm.RepeatTypeList = GetRepeatTypeToAddProcess();
                return View(vm);
            }

            return RedirectToAction("ManageActivities");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUnfixedActivity(UnfixedActivityAddOrEditViewModel vm)
        {
            bool changedFromEditor = vm.enableRepeatChange;
            string userid = User.Identity.GetUserId();
            int activityID = int.Parse(Request.Form["id"]); //Get the ID from "view"
            if (ModelState.IsValid)
            {
                using (DamaDB db = new DamaDB())
                {
                    if (changedFromEditor) //If it's true, no need to change the database
                    {
                        Cont.Reset = false;
                        if (vm.IsOptional == true)
                        {
                            UnfixedActivity edited = Cont.ActivitySelectedByUserForOptional.FirstOrDefault(x => x.ID == activityID && x.OwnType == ActivityType.Unfixed) as UnfixedActivity;
                            edited = edited + CreateNewUnfixedActivityMethod(vm, userid, false);
                        }
                        else
                        {
                            UnfixedActivity edited = Cont.ActivitySelectedByUserForSure.FirstOrDefault(x => x.ID == activityID && x.OwnType == ActivityType.Unfixed) as UnfixedActivity;
                            edited = edited + CreateNewUnfixedActivityMethod(vm, userid, false);
                        }
                        return RedirectToAction("Editor", "CalendarEditor");
                    }
                    else
                    {
                        UnfixedActivity cActivity = await db.UnFixedActivities.Where(x => x.ID == activityID).Include(x => x.Labels).Include(act => act.Category).FirstOrDefaultAsync();
                        if (cActivity != null)
                        {
                            if (cActivity.Labels != null)
                                db.Labels.RemoveRange(cActivity.Labels);
                            cActivity.Category = null;
                            db.UnFixedActivities.Remove(cActivity);
                            db.SaveChanges();
                        }
                    }
                }
                await CreateNewActivityFromTuple(new MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel>() { Item2 = vm });
            }
            else
            {
                vm.LabelList = GetAllLabelsToAddProcess(userid);
                vm.ColorList = _colors;
                vm.CategoryList = await GetAllCategoriesToAddProcessAsyc(userid);
                vm.RepeatTypeList = GetRepeatTypeToAddProcess();
                return View(vm);
            }

            return RedirectToAction("ManageActivities");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUndefinedActivity(UndefinedActivityAddOrEditViewModel vm)
        {
            string userid = User.Identity.GetUserId();
            int activityID = int.Parse(Request.Form["id"]); //Get the ID from "view"
            if (ModelState.IsValid)
            {
                using (DamaDB db = new DamaDB())
                {

                    if (vm.CalledFromEditor)
                    {
                        Cont.Reset = false;
                        UndefinedActivity edited = Cont.ActivitySelectedByUserForOptional.FirstOrDefault(x => x.ID == activityID && x.OwnType == ActivityType.Undefined) as UndefinedActivity;
                        edited = edited + CreateNewUndefinedActivityMethod(vm, userid, false);
                        return RedirectToAction("Editor", "CalendarEditor");
                    }
                    else
                    {
                        UndefinedActivity cActivity = await db.UndefinedActivities.Where(x => x.ID == activityID).Include(x => x.Labels).Include(act => act.Category).FirstOrDefaultAsync();
                        if (cActivity != null)
                        {
                            if (cActivity.Labels != null)
                                db.Labels.RemoveRange(cActivity.Labels);
                            cActivity.Category = null;
                            db.UndefinedActivities.Remove(cActivity);
                            db.SaveChanges();
                        }
                    }
                }
                await CreateNewActivityFromTuple(new MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel>() { Item3 = vm });
            }
            else
            {
                vm.LabelList = GetAllLabelsToAddProcess(userid);
                vm.ColorList = _colors;
                vm.CategoryList = await GetAllCategoriesToAddProcessAsyc(userid);
                return View(vm);
            }

            return RedirectToAction("ManageActivities");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDeadlineActivity(DeadlineActivityAddOrEditViewModel vm)
        {
            string userid = User.Identity.GetUserId();
            int activityID = int.Parse(Request.Form["id"]); //Get the ID from "view"
            if (ModelState.IsValid)
            {
                using (DamaDB db = new DamaDB())
                {
                    if (vm.CalledFromEditor)
                    {
                        Cont.Reset = false;
                        DeadlineActivity edited = Cont.ActivitySelectedByUserForSure.FirstOrDefault(x => x.ID == activityID && x.OwnType == ActivityType.Deadline) as DeadlineActivity;
                        edited = edited + CreateNewDeadlineActivityMethod(vm, userid, false);
                        return RedirectToAction("Editor", "CalendarEditor");
                    }
                    else
                    {
                        DeadlineActivity cActivity = await db.DeadLineActivities.Where(x => x.ID == activityID).Include(x => x.Labels).Include(act => act.Category).Include(act => act.MileStones).FirstOrDefaultAsync();
                        if (cActivity != null)
                        {
                            if (cActivity.Labels != null)
                                db.Labels.RemoveRange(cActivity.Labels);
                            cActivity.Category = null;
                            if (cActivity.MileStones != null)
                                db.Milestones.RemoveRange(cActivity.MileStones);
                            db.DeadLineActivities.Remove(cActivity);
                            db.SaveChanges();
                            ViewBag.ActivityRemovedSuccessFully = Messages.ActivityRemovedSuccessFully;
                        }
                    }
                }
                await CreateNewActivityFromTuple(new MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel>() { Item4 = vm });
            }
            else
            {
                vm.ColorList = _colors;
                return View(vm);
            }

            return RedirectToAction("ManageActivities");
        }

        private async Task<ActionResult> CreateNewActivityFromTuple(MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel> tupleObject)
        {
            bool Base = true;
            string userid = User.Identity.GetUserId();

            //Fixed
            if (tupleObject.Item1 != null)
            {
                FixedActivityAddOrEditViewModel fixedActivityAddOrEditViewModel = tupleObject.Item1;
                if (ModelState.IsValid)
                {
                    using (DamaDB db = new DamaDB())
                    {
                        FixedActivity fa = CreateNewFixedActivityMethod(fixedActivityAddOrEditViewModel, userid, Base);
                        if (fa.Category != null)
                            db.Entry(fa.Category).State = EntityState.Unchanged;
                        db.FixedActivities.Add(fa);
                        db.SaveChanges();
                    }
                    return RedirectToAction("ManageActivities");
                }
                else
                {
                    fixedActivityAddOrEditViewModel.LabelList = GetAllLabelsToAddProcess(userid);
                    fixedActivityAddOrEditViewModel.ColorList = _colors;
                    fixedActivityAddOrEditViewModel.CategoryList = await GetAllCategoriesToAddProcessAsyc(userid);
                    return View(GetActivityTupleObject(false, fixedActivityAddOrEditViewModel, null, null, null));
                }
            }
            //Unfixed
            if (tupleObject.Item2 != null)
            {
                UnfixedActivityAddOrEditViewModel unfixedActivityAddOrEditViewModel = tupleObject.Item2;
                if (ModelState.IsValid)
                {
                    using (DamaDB db = new DamaDB())
                    {
                        UnfixedActivity ufa = CreateNewUnfixedActivityMethod(unfixedActivityAddOrEditViewModel, userid, Base);
                        if (ufa.Category != null)
                            db.Entry(ufa.Category).State = EntityState.Unchanged;
                        db.UnFixedActivities.Add(ufa);
                        db.SaveChanges();
                    }
                    return RedirectToAction("ManageActivities");
                }
                else
                {
                    unfixedActivityAddOrEditViewModel.LabelList = GetAllLabelsToAddProcess(userid);
                    unfixedActivityAddOrEditViewModel.ColorList = _colors;
                    unfixedActivityAddOrEditViewModel.CategoryList = await GetAllCategoriesToAddProcessAsyc(userid);
                    return View(GetActivityTupleObject(false, null, unfixedActivityAddOrEditViewModel, null, null));
                }
            }
            //Undefined
            if (tupleObject.Item3 != null)
            {
                UndefinedActivityAddOrEditViewModel undefinedActivityAddOrEditViewModel = tupleObject.Item3;
                if (ModelState.IsValid)
                {
                    using (DamaDB db = new DamaDB())
                    {
                        UndefinedActivity uda = CreateNewUndefinedActivityMethod(undefinedActivityAddOrEditViewModel, userid, Base);
                        if (uda.Category != null)
                            db.Entry(uda.Category).State = EntityState.Unchanged;
                        db.UndefinedActivities.Add(uda);
                        db.SaveChanges();
                    }
                    return RedirectToAction("ManageActivities");
                }
                else
                {
                    undefinedActivityAddOrEditViewModel.LabelList = GetAllLabelsToAddProcess(userid);
                    undefinedActivityAddOrEditViewModel.ColorList = _colors;
                    undefinedActivityAddOrEditViewModel.CategoryList = await GetAllCategoriesToAddProcessAsyc(userid);
                    return View(GetActivityTupleObject(false, null, null, undefinedActivityAddOrEditViewModel, null));
                }
            }
            //Deadline
            if (tupleObject.Item4 != null)
            {
                DeadlineActivityAddOrEditViewModel deadlineActivityAddOrEditViewModel = tupleObject.Item4;
                if (ModelState.IsValid)
                {
                    using (DamaDB db = new DamaDB())
                    {
                        db.DeadLineActivities.Add(CreateNewDeadlineActivityMethod(deadlineActivityAddOrEditViewModel, userid, Base));
                        db.SaveChanges();
                    }
                    return RedirectToAction("ManageActivities");
                }
                else
                {
                    deadlineActivityAddOrEditViewModel.ColorList = _colors;
                    return View(GetActivityTupleObject(false, null, null, null, deadlineActivityAddOrEditViewModel));
                }
            }
            return RedirectToAction("ManageActivities");
        }

        private FixedActivity CreateNewFixedActivityMethod(FixedActivityAddOrEditViewModel vm, string userid, bool Base)
        {
            Category category;
            List<Label> labels = new List<Label>();
            DateTime start = DateTime.Today + vm.StartTime.TimeOfDay;
            DateTime end = DateTime.Today + vm.EndTime.TimeOfDay;
            int finalPriority;

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

                if (vm.Priority == 0)
                {
                    if (category == null)
                        finalPriority = 1;
                    else
                        finalPriority = category.Priority;
                }
                else
                    finalPriority = vm.Priority;
            }

            Repeat repeat = null;
            if (vm.RepeatType != null && vm.RepeatEndDate != null)
            {
                repeat = new Repeat(vm.StartTime, (RepeatPeriod)Enum.Parse(typeof(RepeatPeriod), vm.RepeatType), vm.RepeatEndDate);
            }

            return new FixedActivity(vm.Name,
                                    vm.Description,
                                    vm.Color,
                                    false,
                                    Base,
                                    labels,
                                    category,
                                    userid,
                                    finalPriority,
                                    start,
                                    end,
                                    repeat);
        }

        private UnfixedActivity CreateNewUnfixedActivityMethod(UnfixedActivityAddOrEditViewModel vm, string userid, bool Base)
        {
            Category category;
            List<Label> labels = new List<Label>();
            TimeSpan time = vm.Timespan;
            int finalPriority;

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
            }

            if (vm.Priority == 0)
            {
                if (category == null)
                    finalPriority = 1;
                else
                    finalPriority = category.Priority;
            }
            else
                finalPriority = vm.Priority;

            Repeat repeat = null;
            if (vm.RepeatType != null && vm.RepeatEndDate != null)
            {
                repeat = new Repeat((RepeatPeriod)Enum.Parse(typeof(RepeatPeriod), vm.RepeatType), vm.RepeatEndDate);
            }

            return new UnfixedActivity(
                                        vm.Name,
                                        vm.Description,
                                        vm.Color,
                                        false,
                                        Base,
                                        labels,
                                        category,
                                        userid,
                                        finalPriority,
                                        time,
                                        repeat);

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