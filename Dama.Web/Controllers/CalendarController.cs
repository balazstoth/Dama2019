using Dama.Data.Enums;
using Dama.Data.Interfaces;
using Dama.Data.Models;
using Dama.Data.Sql.Repositories;
using Dama.Data.Sql.SQL;
using Dama.Organizer.Enums;
using Dama.Organizer.Resources;
using Dama.Web.Attributes;
using Dama.Web.Models.ViewModels.Category;
using Dama.Web.Models.ViewModels.Label;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Dama.Web.Controllers
{
    [Authorize]
    [DisableUser]
    public class CalendarController : Controller
    {
        private UserManager<User> _userManager;
        private List<SelectListItem> _colors;
        private readonly string[] _availableColors;
        private readonly IRepositoryInjection _repositories;

        public CalendarController(IRepositoryInjection repositoryInjection)
        {
            _repositories = repositoryInjection;
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
                RemoveCategoryFromDataTables(selectedCategory.Id);
                await _repositories.CategorySqlRepository.RemoveAsync(selectedCategory);
                ViewBag.CategoryRemovedSuccessFully = Success.CategoryRemovedSuccessfully;
            }

            return RedirectToAction(ActionNames.ManageCategories.ToString());
        }

        private void RemoveCategoryFromDataTables(string categoryId)
        {
            RemoveCategoryFromTable(_repositories.FixedActivitySqlRepository, categoryId);
            RemoveCategoryFromTable(db.UnFixedActivities, categoryId);
            RemoveCategoryFromTable(db.UndefinedActivities, categoryId);
            RemoveCategoryFromTable(db.DeadLineActivities, categoryId);
        }

        private void RemoveCategoryFromTable(DbSet<Activity> table, string categoryId)
        {
            foreach (var record in table.Include(r => r.Category).ToList())
                if (record.Category != null && record.Category.Id.Equals(categoryId))
                    record.Category = null;
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
                    Id = category.Id,
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

        //==================================================================================================
        
        #region Activity
        public async Task<ActionResult> ManageActivities(bool sortedByCategory = false, string categoryID = "")
        {
            string CurrentUserID = User.Identity.GetUserId();
            List<FixedActivity> fixedActivities = new List<FixedActivity>();
            List<UnfixedActivity> unfixedActivities = new List<UnfixedActivity>();
            List<UndefinedActivity> undefinedActivities = new List<UndefinedActivity>();
            List<DeadlineActivity> deadlineActivities = new List<DeadlineActivity>();

            using (DamaDB db = new DamaDB())
            {
                if (sortedByCategory)
                {
                    int category = int.Parse(categoryID);
                    fixedActivities = await db.FixedActivities.Where(x => x.UserID == CurrentUserID && x.Base == true && x.Category.ID == category).Include(act => act.Labels).Include(act => act.Category).OrderBy(x => x.Name).ToListAsync();
                    unfixedActivities = await db.UnFixedActivities.Where(x => x.UserID == CurrentUserID && x.Base == true && x.Category.ID == category).Include(act => act.Labels).Include(act => act.Category).OrderBy(x => x.Name).ToListAsync();
                    undefinedActivities = await db.UndefinedActivities.Where(x => x.UserID == CurrentUserID && x.Base == true && x.Category.ID == category).Include(act => act.Labels).Include(act => act.Category).OrderBy(x => x.Name).ToListAsync();
                    deadlineActivities = await db.DeadLineActivities.Where(x => x.UserID == CurrentUserID && x.Base == true && x.Category.ID == category).Include(act => act.Labels).Include(act => act.Category).Include(act => act.MileStones).OrderBy(x => x.Name).ToListAsync();
                }
                else
                {
                    fixedActivities = await db.FixedActivities.Where(x => x.UserID == CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).OrderBy(x => x.Name).ToListAsync();
                    unfixedActivities = await db.UnFixedActivities.Where(x => x.UserID == CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).OrderBy(x => x.Name).ToListAsync();
                    undefinedActivities = await db.UndefinedActivities.Where(x => x.UserID == CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).OrderBy(x => x.Name).ToListAsync();
                    deadlineActivities = await db.DeadLineActivities.Where(x => x.UserID == CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).OrderBy(x => x.Name).Include(act => act.MileStones).ToListAsync();
                }
            }

            var tuple = new Tuple<FixedActivityViewModel, UnFixedActivityViewModel, UndefinedActivityViewModel, DeadlineActivityViewModel>(
                new FixedActivityViewModel() { fixedActivities = fixedActivities },
                new UnFixedActivityViewModel() { unfixedActivities = unfixedActivities },
                new UndefinedActivityViewModel() { undefinedActivity = undefinedActivities },
                new DeadlineActivityViewModel() { deadlineActivities = deadlineActivities });

            if (sortedByCategory)
                return View("ListSortedByCategoryActivities", tuple);
            else
                return View("ManageActivities", tuple);
        }
        public async Task<ActionResult> ActivityDetails(string id, ActivityType type)
        {
            int paramID = int.Parse(id);
            using (DamaDB db = new DamaDB())
            {
                switch (type)
                {
                    case ActivityType.Fixed:
                        List<FixedActivity> a = await db.FixedActivities.Where(x => x.ID == paramID).Include(x => x.Labels).Include(act => act.Category).ToListAsync();
                        FixedActivityViewModel avm = new FixedActivityViewModel() { fixedActivities = a };
                        return View("FixedActivityDetails", avm);

                    case ActivityType.Undefined:
                        List<UndefinedActivity> b = await db.UndefinedActivities.Where(x => x.ID == paramID).Include(x => x.Labels).Include(act => act.Category).ToListAsync();
                        UndefinedActivityViewModel bvm = new UndefinedActivityViewModel() { undefinedActivity = b };
                        return View("UndefinedActivityDetails", bvm);

                    case ActivityType.Unfixed:
                        List<UnfixedActivity> c = await db.UnFixedActivities.Where(x => x.ID == paramID).Include(x => x.Labels).Include(act => act.Category).ToListAsync();
                        UnFixedActivityViewModel cvm = new UnFixedActivityViewModel() { unfixedActivities = c };
                        return View("UnfixedActivityDetails", cvm);

                    case ActivityType.Deadline:
                        List<DeadlineActivity> d = await db.DeadLineActivities.Where(x => x.ID == paramID).Include(x => x.Labels).Include(act => act.Category).Include(act => act.MileStones).ToListAsync();
                        DeadlineActivityViewModel dvm = new DeadlineActivityViewModel() { deadlineActivities = d };
                        return View("DeadlineActivityDetails", dvm);

                    default:
                        return RedirectToAction("ManageActivities");
                }
            }
        }
        public async Task<ActionResult> DeleteActivity(string id, ActivityType type)
        {
            int paramID = int.Parse(id);
            using (DamaDB db = new DamaDB())
            {
                switch (type)
                {
                    case ActivityType.Fixed:
                        FixedActivity a = await db.FixedActivities.Where(x => x.ID == paramID).Include(x => x.Labels).Include(act => act.Category).FirstOrDefaultAsync();
                        if (a != null)
                        {
                            if (a.Labels != null)
                                db.Labels.RemoveRange(a.Labels);
                            //if (a.Category != null)
                            //    db.Categories.Remove(a.Category);
                            a.Category = null;
                            db.FixedActivities.Remove(a);
                            db.SaveChanges();
                        }
                        break;

                    case ActivityType.Undefined:
                        UndefinedActivity b = await db.UndefinedActivities.Where(x => x.ID == paramID).Include(x => x.Labels).Include(act => act.Category).FirstOrDefaultAsync();
                        if (b != null)
                        {
                            if (b.Labels != null)
                                db.Labels.RemoveRange(b.Labels);
                            b.Category = null;
                            db.UndefinedActivities.Remove(b);
                            db.SaveChanges();
                            ViewBag.ActivityRemovedSuccessFully = Messages.ActivityRemovedSuccessFully;
                        }
                        break;

                    case ActivityType.Unfixed:
                        UnfixedActivity c = await db.UnFixedActivities.Where(x => x.ID == paramID).Include(x => x.Labels).Include(act => act.Category).FirstOrDefaultAsync();
                        if (c != null)
                        {
                            if (c.Labels != null)
                                db.Labels.RemoveRange(c.Labels);
                            c.Category = null;
                            db.UnFixedActivities.Remove(c);
                            db.SaveChanges();
                            ViewBag.ActivityRemovedSuccessFully = Messages.ActivityRemovedSuccessFully;
                        }
                        break;

                    case ActivityType.Deadline:
                        DeadlineActivity d = await db.DeadLineActivities.Where(x => x.ID == paramID).Include(x => x.Labels).Include(act => act.Category).Include(act => act.MileStones).FirstOrDefaultAsync();
                        if (d != null)
                        {
                            if (d.Labels != null)
                                db.Labels.RemoveRange(d.Labels);
                            d.Category = null;
                            if (d.MileStones != null)
                                db.Milestones.RemoveRange(d.MileStones);
                            db.DeadLineActivities.Remove(d);
                            db.SaveChanges();
                            ViewBag.ActivityRemovedSuccessFully = Messages.ActivityRemovedSuccessFully;
                        }
                        break;
                }
                return RedirectToAction("ManageActivities");
            }
        }
        public async Task<ActionResult> AddNewActivity()
        {
            string currentUserID = User.Identity.GetUserId();
            List<SelectListItem> ColorList = new List<SelectListItem>(this._colors);
            List<SelectListItem> CategoryList = await GetAllCategoriesToAddProcessAsyc(currentUserID);
            List<SelectListItem> LabelList = GetAllLabelsToAddProcess(currentUserID);
            List<SelectListItem> RepeatTypeList = GetRepeatTypeToAddProcess();
            FixedActivityAddOrEditViewModel fixedActivityViewModel = new FixedActivityAddOrEditViewModel() { LabelList = LabelList, CategoryList = CategoryList, ColorList = ColorList, RepeatTypeList = RepeatTypeList };
            UnfixedActivityAddOrEditViewModel unFixedActivityViewModel = new UnfixedActivityAddOrEditViewModel() { LabelList = LabelList, CategoryList = CategoryList, ColorList = ColorList, RepeatTypeList = RepeatTypeList };
            UndefinedActivityAddOrEditViewModel undefinedActivityViewModel = new UndefinedActivityAddOrEditViewModel() { LabelList = LabelList, CategoryList = CategoryList, ColorList = ColorList };
            DeadlineActivityAddOrEditViewModel deadlineActivityViewModel = new DeadlineActivityAddOrEditViewModel() { ColorList = ColorList };

            return View(GetActivityTupleObject(true, fixedActivityViewModel, unFixedActivityViewModel, undefinedActivityViewModel, deadlineActivityViewModel));
        }
        public List<SelectListItem> GetRepeatTypeToAddProcess()
        {
            var enums = Enum.GetValues(typeof(RepeatPeriod)).Cast<RepeatPeriod>();
            return enums.Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString() }).ToList();
        }
        public async Task<List<SelectListItem>> GetAllCategoriesToAddProcessAsyc(string userID)
        {
            List<Category> AllCategories;
            List<SelectListItem> CategoryList = new List<SelectListItem>();

            using (DamaDB db = new DamaDB())
            {
                AllCategories = await db.Categories.Where(x => x.UserID == userID).ToListAsync();
            }

            CategoryList.Add(new SelectListItem() { Text = "Uncategorized", Value = null });
            foreach (Category i in AllCategories)
            {
                SelectListItem sli = new SelectListItem() { Text = i.Name };
                CategoryList.Add(sli);
            }
            return CategoryList;
        }
        public List<SelectListItem> GetAllLabelsToAddProcess(string userID)
        {
            List<string> AllLabels = new List<string>();
            List<SelectListItem> LabelList = new List<SelectListItem>();
            using (DamaDB db = new DamaDB())
            {
                var tmp = (db.Labels.Where(x => x.UserID == userID).Select(x => x.Name)).Distinct();
                foreach (string i in tmp)
                {
                    SelectListItem sli = new SelectListItem() { Text = i };
                    LabelList.Add(sli);
                }
            }

            return LabelList;
        }
        public MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel> GetActivityTupleObject(
            bool getMethod,
            FixedActivityAddOrEditViewModel fixedActivityAddOrEditViewModel,
            UnfixedActivityAddOrEditViewModel unfixedActivityAddOrEditViewModel,
            UndefinedActivityAddOrEditViewModel undefinedActivityAddOrEditViewModel,
            DeadlineActivityAddOrEditViewModel deadlineActivityAddOrEditViewModel)
        {
            if (getMethod)
            {
                return new MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel>(fixedActivityAddOrEditViewModel, unfixedActivityAddOrEditViewModel, undefinedActivityAddOrEditViewModel, deadlineActivityAddOrEditViewModel);
            }

            if (fixedActivityAddOrEditViewModel != null)
            {
                return new MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel>(fixedActivityAddOrEditViewModel, null, null, null);
            }
            if (unfixedActivityAddOrEditViewModel != null)
            {
                return new MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel>(null, unfixedActivityAddOrEditViewModel, null, null);
            }
            if (undefinedActivityAddOrEditViewModel != null)
            {
                return new MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel>(null, null, undefinedActivityAddOrEditViewModel, null);
            }
            if (deadlineActivityAddOrEditViewModel != null)
            {
                return new MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel>(null, null, null, deadlineActivityAddOrEditViewModel);
            }
            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewActivity(MyTuple<FixedActivityAddOrEditViewModel, UnfixedActivityAddOrEditViewModel, UndefinedActivityAddOrEditViewModel, DeadlineActivityAddOrEditViewModel> tupleObject)
        {
            return await CreateNewActivityFromTuple(tupleObject);
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