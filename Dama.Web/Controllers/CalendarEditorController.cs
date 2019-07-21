using Dama.Data;
using Dama.Data.Enums;
using Dama.Data.Models;
using Dama.Organizer.Models;
using Dama.Web.Attributes;
using Dama.Web.Models.ViewModels.Activity.Display;
using Dama.Web.Models.ViewModels.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ActionNames = Dama.Organizer.Enums.ActionNames;
using ControllerNames = Dama.Organizer.Enums.ControllerNames;
using ViewNames = Dama.Organizer.Enums.ViewNames;

namespace Dama.Web.Controllers
{
    [Authorize]
    [DisableUser]
    public class CalendarEditorController : Controller
    {
        private const string _fixedActivityName = "FixedActivity";
        private const string _unfixedActivityName = "UnfixedActivity";
        private const string _undefinedActivityName = "UndefinedActivity";
        private const string _deadlineActivityName = "DeadlineActivity";

        /// <summary>
        /// Gives basic data for the Editor view
        /// </summary>
        public ActionResult Editor() 
        {
            var viewModel = GetValidViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// Fill first listBox to display the activities ordered by types
        /// </summary>
        public PartialViewResult GetActivityData(string activityTypeName)
        {
            CalendarEditorViewModel viewModel = null;

            using (var container = new ActivityContainer())
            {
                container.CalendarEditorViewModel.SelectedType = activityTypeName;
                container.CalendarEditorViewModel.ActivityCollectionForActivityTypes.Clear();

                IEnumerable<SelectListItem> selectListItemCollection = null;

                switch (activityTypeName)
                {
                    case _fixedActivityName:
                        selectListItemCollection = container.FixedActivities
                                                            .Select(a => new SelectListItem() { Text = a.Name, Value = a.Id.ToString() });
                        break;

                    case _unfixedActivityName:
                        selectListItemCollection = container.UnfixedActivities
                                                            .Select(a => new SelectListItem() { Text = a.Name, Value = a.Id.ToString() });
                        break;

                    case _undefinedActivityName:
                        selectListItemCollection = container.UndefinedActivities
                                                            .Select(a => new SelectListItem() { Text = a.Name, Value = a.Id.ToString() });
                        break;

                    case _deadlineActivityName:
                        selectListItemCollection = container.DeadlineActivities
                                                            .Select(a => new SelectListItem() { Text = a.Name, Value = a.Id.ToString() });
                        break;
                }

                container.CalendarEditorViewModel
                                .ActivityCollectionForActivityTypes
                                        .AddRange(selectListItemCollection);

                viewModel = container.CalendarEditorViewModel;
            }

            return PartialView(ViewNames.GetActivityData.ToString(), viewModel);
        } 

        public PartialViewResult GetActivityDetails(string activityTypeName, string activityId)
        {
            //For the selected listboxes
            if (activityTypeName == "null")
            {
                //In this case the activityid contains two parameters
                var parameters = activityId.Split('|');
                activityTypeName = parameters[1];
                activityId = parameters[0];
            }

            //For the first lisbox
            var viewModel = new DisplayDetailsViewModel();

            using (var container = new ActivityContainer())
            switch (activityTypeName)
            {
                case _fixedActivityName:
                {
                    var fixedActivity = container.FixedActivities
                                                        .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.FixedActivity)
                                                        .FirstOrDefault();
                    if (fixedActivity == null)
                        fixedActivity = container.ActivitySelectedByUserForSure
                                                        .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.FixedActivity)
                                                        .FirstOrDefault() as FixedActivity;

                    if (fixedActivity == null)
                        fixedActivity = container.ActivitySelectedByUserForOptional
                                                        .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.FixedActivity)
                                                        .FirstOrDefault() as FixedActivity;

                    viewModel.FixedActivity = fixedActivity;
                    break;
                }

                case _unfixedActivityName:
                {
                    var unfixedActivity = container.UnfixedActivities
                                                        .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.UnfixedActivity)
                                                        .FirstOrDefault();

                    if (unfixedActivity == null)
                        unfixedActivity = container.ActivitySelectedByUserForSure
                                                        .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.UnfixedActivity)
                                                        .FirstOrDefault() as UnfixedActivity;

                    if (unfixedActivity == null)
                        unfixedActivity = container.ActivitySelectedByUserForOptional
                                                        .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.UnfixedActivity)
                                                        .FirstOrDefault() as UnfixedActivity;

                    viewModel.UnfixedActivity = unfixedActivity;
                    break;
                }

                case _undefinedActivityName:
                {
                    var undefinedActivity = container.UndefinedActivities
                                                .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.UndefinedActivity)
                                                .FirstOrDefault();

                    if (undefinedActivity == null)
                        undefinedActivity = container.ActivitySelectedByUserForSure.Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.UndefinedActivity)
                                                    .FirstOrDefault() as UndefinedActivity;

                    if (undefinedActivity == null)
                        undefinedActivity = container.ActivitySelectedByUserForOptional.Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.UndefinedActivity)
                                                    .FirstOrDefault() as UndefinedActivity;

                    viewModel.UndefinedActivity = undefinedActivity;
                    break;
                }

                case _deadlineActivityName:
                {
                    var deadlineActivity = container.DeadlineActivities
                                                .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.DeadlineActivity)
                                                .FirstOrDefault();

                    if (deadlineActivity == null)
                        deadlineActivity = container.ActivitySelectedByUserForSure
                                                        .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.DeadlineActivity)
                                                        .FirstOrDefault() as DeadlineActivity;
                    if (deadlineActivity == null)
                        deadlineActivity = container.ActivitySelectedByUserForSure
                                                    .Where(a => a.Id.ToString() == activityId && a.ActivityType == ActivityType.DeadlineActivity)
                                                    .FirstOrDefault() as DeadlineActivity;

                    viewModel.DeadlineActivity = deadlineActivity;
                    break;
                }

                default:
                {
                    //DropDownList onchange has never run yet, the default is "FixedaActivity"
                    var fixedAct = container.FixedActivities.Where(a => a.Id.ToString() == activityId).FirstOrDefault();
                    viewModel.FixedActivity = fixedAct;
                    break;
                }
            }
            
            return PartialView(ViewNames.GetActivityDetails.ToString(), viewModel);
        }

        public PartialViewResult GetAvailableFilters(string activityTypeName)
        {
            CalendarEditorViewModel viewModel = null;
            var message = "Not active";

            switch (activityTypeName)
            {
                case _fixedActivityName:
                case _unfixedActivityName:
                {
                    using (var container = new ActivityContainer())
                    {
                        container.CalendarEditorViewModel
                                 .CategoryFilterSourceCollection = new List<SelectListItem>(
                                        container.Categories
                                                    .Select(c => new SelectListItem() { Text = c.Name, Value = c.Name }));

                        container.CalendarEditorViewModel
                                 .CategoryFilterSourceCollection
                                                    .Add(new SelectListItem() { Text = message, Value = string.Empty, Selected = true });

                        container.CalendarEditorViewModel
                                 .LabelFilterSourceCollection = 
                                                    (from l in container.Labels
                                                        group l by l.Name 
                                                        into grpdLabel
                                                        select new SelectListItem()
                                                        {
                                                            Text = grpdLabel.Key,
                                                            Value = grpdLabel
                                                                        .Select(x => x.Name)
                                                                        .First()
                                                        }).ToList();

                        container.CalendarEditorViewModel
                                 .LabelFilterSourceCollection
                                    .Add(new SelectListItem() { Text = message, Value = string.Empty, Selected = true });

                        viewModel = container.CalendarEditorViewModel;
                    }
                           
                    return PartialView(ViewNames.GetAllFilters.ToString(), viewModel);
                }

                case _undefinedActivityName:
                {
                    using (var container = new ActivityContainer())
                    {
                        container.CalendarEditorViewModel
                                 .CategoryFilterSourceCollection = new List<SelectListItem>(
                                     container.Categories
                                                .Select(c => new SelectListItem() { Text = c.Name, Value = c.Name }));

                        container.CalendarEditorViewModel
                                 .CategoryFilterSourceCollection
                                 .Add(new SelectListItem() { Text = message, Value = string.Empty, Selected = true });

                        container.CalendarEditorViewModel
                                 .LabelFilterSourceCollection = 
                                        (from l in container.Labels
                                        group l by l.Name 
                                        into grpdLabel
                                        select new SelectListItem()
                                        {
                                            Text = grpdLabel.Key,
                                            Value = grpdLabel.Select(x => x.Name).First()
                                        }).ToList();

                        container.CalendarEditorViewModel
                                 .LabelFilterSourceCollection
                                 .Add(new SelectListItem() { Text = message, Value = string.Empty, Selected = true });

                        viewModel = container.CalendarEditorViewModel;
                    }
                
                    return PartialView(ViewNames.GetReducedFilters.ToString(), viewModel);
                }

                default:
                    return null;
            }
        }

        public PartialViewResult GetFilteredActivities(string activityType, string activityName, string category, string label, string priority)
        {
            CalendarEditorViewModel viewModel = null;

            using (var container = new ActivityContainer())
            {
                container.CalendarEditorViewModel.SelectedType = activityType;
                var parsedPriority = int.MinValue;
                container.CalendarEditorViewModel.ActivityCollectionForActivityTypes.Clear();

                switch (activityType)
                {
                    case _fixedActivityName:
                        var filteredFixed = container.FixedActivities.ToList();

                        if (!activityName.Equals(string.Empty))
                            filteredFixed = container.FixedActivities.Where(a => a.Name.ToLower().Contains(activityName.ToLower())).ToList();

                        if (!category.Equals(string.Empty))
                            filteredFixed = filteredFixed.Where(a => a.Category != null && a.Category.Name.Equals(category)).ToList();

                        if (!label.Equals(string.Empty))
                            filteredFixed = filteredFixed.Where(a => a.ContainsLabel(label)).ToList();

                        if (!priority.Equals(string.Empty) && int.TryParse(priority, out parsedPriority))
                            filteredFixed = filteredFixed.Where(a => a.Priority.Equals(parsedPriority)).ToList();

                        container.CalendarEditorViewModel
                                 .ActivityCollectionForActivityTypes
                                    .AddRange(filteredFixed.Select(a => new SelectListItem() { Text = a.Name, Value = a.Id.ToString() }));
                        break;

                    case _unfixedActivityName:
                        var filteredUnfixed = container.UnfixedActivities.ToList();

                        if (!activityName.Equals(string.Empty))
                            filteredUnfixed = filteredUnfixed.Where(a => a.Name.ToLower().Contains(activityName.ToLower())).ToList();

                        if (!category.Equals(string.Empty))
                            filteredUnfixed = filteredUnfixed.Where(a => a.Category != null && a.Category.Name.Equals(category)).ToList();

                        if (!label.Equals(string.Empty))
                            filteredUnfixed = filteredUnfixed.Where(a => a.ContainsLabel(label)).ToList();

                        if (!priority.Equals(string.Empty) && int.TryParse(priority, out parsedPriority))
                            filteredUnfixed = filteredUnfixed.Where(a => a.Priority.Equals(parsedPriority)).ToList();

                        container.CalendarEditorViewModel
                                 .ActivityCollectionForActivityTypes
                                    .AddRange(filteredUnfixed.Select(a => new SelectListItem() { Text = a.Name, Value = a.Id.ToString() }));
                        break;

                    case _undefinedActivityName:
                        var filteredUndefined = container.UndefinedActivities.ToList();

                        if (!activityName.Equals(string.Empty))
                            filteredUndefined = filteredUndefined.Where(a => a.Name.ToLower().Contains(activityName.ToLower())).ToList();

                        if (!category.Equals(string.Empty))
                            filteredUndefined = filteredUndefined.Where(a => a.Category != null && a.Category.Name.Equals(category)).ToList();

                        if (!label.Equals(string.Empty))
                            filteredUndefined = filteredUndefined.Where(a => a.ContainsLabel(label)).ToList();

                        container.CalendarEditorViewModel
                                 .ActivityCollectionForActivityTypes
                                    .AddRange(filteredUndefined.Select(a => new SelectListItem() { Text = a.Name, Value = a.Id.ToString() }));
                        break;

                    default:
                        return null;
                }

                viewModel = container.CalendarEditorViewModel;
            }

            return PartialView(ViewNames.GetActivityData.ToString(), viewModel);
        }

        public PartialViewResult GetSelectedActivities(string type, string idList, string forOptional)
        {
            if (!IsValidForFixedOrOpt(type, forOptional))
            {
                ViewBag.AddActIsNotValid = "Cannot add this activity to this list!";
                if (forOptional == "true")
                    return PartialView("GetSelectedOptionalActivities", container.VM);
                else
                    return PartialView("GetSelectedActivities", container.VM);
            }

            List<int> selectedIDs = SeparateidList(idList);
            if (selectedIDs == null)
                if (forOptional == "true")
                    return PartialView("GetSelectedOptionalActivities", container.VM);
                else
                    return PartialView("GetSelectedActivities", container.VM);

            Activity tmp = null;

            foreach (int i in selectedIDs)
            {
                switch (type)
                {
                    case "Fixed":
                        tmp = container.FixedActivities.Where(x => x.ID == Convert.ToInt32(i)).FirstOrDefault();
                        container.FixedActivities.Remove(tmp as FixedActivity);
                        break;

                    case "Unfixed":
                        tmp = container.UnfixedActivities.Where(x => x.ID == Convert.ToInt32(i)).FirstOrDefault();
                        container.UnfixedActivities.Remove(tmp as UnfixedActivity);
                        break;

                    case "Undefined":
                        tmp = container.UndefinedActivities.Where(x => x.ID == Convert.ToInt32(i)).FirstOrDefault();
                        container.UndefinedActivities.Remove(tmp as UndefinedActivity);
                        break;

                    case "Deadline":
                        tmp = container.DeadlineActivities.Where(x => x.ID == Convert.ToInt32(i)).FirstOrDefault();
                        container.DeadlineActivities.Remove(tmp as DeadlineActivity);
                        break;
                }
                if (forOptional == "true")
                {
                    container.VM.OptionalActivitiesSelectedByUser.Add(new SelectListItem() { Text = tmp.Name, Value = tmp.ID.ToString() + "|" + type });
                    container.VM.OptionalActivitiesSelectedByUser = container.VM.OptionalActivitiesSelectedByUser.OrderBy(x => x.Text).ToList();
                    container.ActivitySelectedByUserForOptional.AddSorted(tmp);
                    container.VM.ActivitySelectedList.RemoveAll(x => x.Value.Split('|')[0] == i.ToString());
                }
                else
                {
                    container.VM.ActivitiesSelectedByUser.Add(new SelectListItem() { Text = tmp.Name, Value = tmp.ID.ToString() + "|" + type });
                    container.VM.ActivitiesSelectedByUser = container.VM.ActivitiesSelectedByUser.OrderBy(x => x.Text).ToList();
                    container.ActivitySelectedByUserForSure.AddSorted(tmp);
                    container.VM.ActivitySelectedList.RemoveAll(x => x.Value.Split('|')[0] == i.ToString());

                    if (type == "Unfixed")
                    {
                        break;
                    }
                }
            }

            if (forOptional == "true")
                return PartialView("GetSelectedOptionalActivities", container.VM);
            else
                return PartialView("GetSelectedActivities", container.VM);
        }

        public PartialViewResult MoveBack(string id_type_List, string fromOptional)
        {
            string[] splittedValues = id_type_List.Split(',');
            SelectListItem sli = null;
            Activity tmp = null;

            if (splittedValues == null || id_type_List == "null") //There's nothing selected
                if (fromOptional == "true")
                    return PartialView("GetSelectedOptionalActivities", container.VM);
                else
                    return PartialView("GetSelectedActivities", container.VM);

            if (fromOptional == "true")
            {
                foreach (string value in splittedValues)
                {
                    foreach (SelectListItem item in container.VM.OptionalActivitiesSelectedByUser)
                    {
                        if (item.Value == value)
                        {
                            sli = item;
                            break;
                        }
                    }

                    string[] idNtype = value.Split('|');
                    container.VM.OptionalActivitiesSelectedByUser.Remove(sli); //Remove from view list
                    foreach (var i in container.ActivitySelectedByUserForOptional)
                    {
                        switch (idNtype[1])
                        {
                            case "Fixed":
                                if (i.ID == int.Parse(idNtype[0]) && (i as FixedActivity) != null)
                                {
                                    container.FixedActivities.AddSorted(i as FixedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Unfixed":
                                if (i.ID == int.Parse(idNtype[0]) && (i as UnfixedActivity) != null)
                                {
                                    container.UnfixedActivities.AddSorted(i as UnfixedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Undefined":
                                if (i.ID == int.Parse(idNtype[0]) && (i as UndefinedActivity) != null)
                                {
                                    container.UndefinedActivities.AddSorted(i as UndefinedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Deadline":
                                if (i.ID == int.Parse(idNtype[0]) && (i as DeadlineActivity) != null)
                                {
                                    container.DeadlineActivities.AddSorted(i as DeadlineActivity);
                                    tmp = i;
                                }
                                break;
                        }
                    }
                    //Cont.VM.ActivitySelectedList.Add(new SelectListItem() { Text = tmp.Name, Value = tmp.ID.ToString() });

                    container.ActivitySelectedByUserForOptional.Remove(tmp);
                }
            }
            else
            {
                foreach (string value in splittedValues)
                {
                    foreach (SelectListItem item in container.VM.ActivitiesSelectedByUser)
                    {
                        if (item.Value == value)
                        {
                            sli = item;
                            break;
                        }
                    }

                    string[] idNtype = value.Split('|');
                    container.VM.ActivitiesSelectedByUser.Remove(sli); //Remove from view list
                    foreach (var i in container.ActivitySelectedByUserForSure)
                    {
                        switch (idNtype[1])
                        {
                            case "Fixed":
                                if (i.ID == int.Parse(idNtype[0]) && (i as FixedActivity) != null)
                                {
                                    FixedActivity current = i as FixedActivity;
                                    if (current.IsUnfixedOriginally)
                                    {
                                        UnfixedActivity ua = new UnfixedActivity(current);
                                        container.UnfixedActivities.AddSorted(ua);
                                    }
                                    else
                                    {
                                        container.FixedActivities.AddSorted(current);
                                    }
                                    tmp = i;
                                }
                                break;
                            case "Unfixed":
                                if (i.ID == int.Parse(idNtype[0]) && (i as UnfixedActivity) != null)
                                {
                                    container.UnfixedActivities.AddSorted(i as UnfixedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Undefined":
                                if (i.ID == int.Parse(idNtype[0]) && (i as UndefinedActivity) != null)
                                {
                                    container.UndefinedActivities.AddSorted(i as UndefinedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Deadline":
                                if (i.ID == int.Parse(idNtype[0]) && (i as DeadlineActivity) != null)
                                {
                                    container.DeadlineActivities.AddSorted(i as DeadlineActivity);
                                    tmp = i;
                                }
                                break;
                        }
                    }
                    container.ActivitySelectedByUserForSure.Remove(tmp);
                }
            }
            if (fromOptional == "true")
                return PartialView("GetSelectedOptionalActivities", container.VM);
            else
                return PartialView("GetSelectedActivities", container.VM);
        }

        public async Task<ActionResult> GetDataForChange(string id_type_string, string isAsc, string selectedDate, string nameFilter, string priorityFilter, string labelFilter, string categoryFilter)
        {
            if (id_type_string == "null") //There's nothing selected
                return null;

            SaveValues(isAsc, selectedDate, nameFilter, priorityFilter, categoryFilter, labelFilter);
            string[] firstID;
            if (id_type_string.Contains(','))
                firstID = id_type_string.Substring(0, id_type_string.IndexOf(',')).Split('|');
            else
                firstID = id_type_string.Split('|');

            string id = firstID[0];
            string type = firstID[1];
            bool optional = firstID[2] == "true";

            var controller = DependencyResolver.Current.GetService<CalendarController>();
            controller.ControllerContext = new ControllerContext(this.Request.RequestContext, controller);
            return await controller.EditActivity(id, GetOriginalTypeFromString(type), true, optional);
        }

        public ActionResult RequestStartTime(string value)
        {
            if (value == "null")
                return null;

            string firstID;

            if (value.Contains(','))
                firstID = value.Substring(0, value.IndexOf(','));
            else
                firstID = value;

            RequestTimeViewModel vm = new RequestTimeViewModel(int.Parse(firstID));
            return PartialView("RequestStartTime", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestStartTime(RequestTimeViewModel vm)
        {
            container.Reset = false;

            if (ModelState.IsValid)
            {
                UnfixedActivity ua = container.ActivitySelectedByUserForSure.FirstOrDefault(i => i.ID == vm.ActivityID && i.OwnType == ActivityType.Unfixed) as UnfixedActivity;
                FixedActivity fa = new FixedActivity(ua, vm.StartTime) { IsUnfixedOriginally = true };
                container.ActivitySelectedByUserForSure.Remove(ua);
                container.ActivitySelectedByUserForSure.AddSorted(fa);
            }
            else
            {
                return View();
            }

            return RedirectToAction("Editor");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(CalendarEditorViewModel vm)
        {
            if (ModelState.Values.First().Errors.Count == 0)
            {
                SaveCurrentDayToDB(vm);
                List<FixedActivity> l = container.ActivitySelectedByUserForSure.Where(x => x.OwnType == ActivityType.Fixed).Select(x => x as FixedActivity).ToList();
                AutoFill fill = new AutoFill(l,
                                                container.ActivitySelectedByUserForOptional,
                                                new DateTime(DateTime.Now.Year, DateTime.Today.Month, DateTime.Today.Day, 8, 0, 0),
                                                new DateTime(DateTime.Now.Year, DateTime.Today.Month, DateTime.Today.Day, 20, 0, 0),
                                                new TimeSpan(0, 5, 0));


            }
            else
                return View(vm);

            return RedirectToAction("Index", "Calendar");
        }

        private void FillListsFromDB()
        {
            //Get all the necessary data from DB
            using (DamaDB db = new DamaDB())
            {
                container.FixedActivities.AddSortedRange(db.FixedActivities.Where(x => x.UserID == container.CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).ToList());
                container.UnfixedActivities.AddSortedRange(db.UnFixedActivities.Where(x => x.UserID == container.CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).ToList());
                container.UndefinedActivities.AddSortedRange(db.UndefinedActivities.Where(x => x.UserID == container.CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).ToList());
                container.DeadlineActivities.AddSortedRange(db.DeadLineActivities.Where(x => x.UserID == container.CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).Include(act => act.MileStones).ToList());
                container.Categories = db.Categories.Where(x => x.UserID == container.CurrentUserID).ToList();
                container.Labels = db.Labels.Where(x => x.UserID == container.CurrentUserID).ToList();
            }
        }

        private List<int> SeparateidList(string idList)
        {
            return idList == "null" || idList == null ? null : idList.Split(',').Select(x => Convert.ToInt32(x)).ToList();
        }

        public PartialViewResult RefreshActivityListbox()
        {
            return PartialView("GetActivityData", container.VM);
        }

        public PartialViewResult RefreshFixList()
        {
            container.VM.ActivitiesSelectedByUser.Clear();
            foreach (Activity i in container.ActivitySelectedByUserForSure)
            {
                container.VM.ActivitiesSelectedByUser.Add(new SelectListItem() { Text = i.Name, Value = i.ID.ToString() + "|" + i.OwnType });
            }
            container.VM.ActivitiesSelectedByUser = container.VM.ActivitiesSelectedByUser.OrderBy(x => x.Text).ToList();
            return PartialView("GetSelectedActivities", container.VM);
        }

        public PartialViewResult RefreshOptionalList()
        {
            container.VM.OptionalActivitiesSelectedByUser.Clear();
            foreach (Activity i in container.ActivitySelectedByUserForOptional)
            {
                container.VM.OptionalActivitiesSelectedByUser.Add(new SelectListItem() { Text = i.Name, Value = i.ID.ToString() + "|" + i.OwnType });
            }
            container.VM.OptionalActivitiesSelectedByUser = container.VM.OptionalActivitiesSelectedByUser.OrderBy(x => x.Text).ToList();
            return PartialView("GetSelectedOptionalActivities", container.VM);
        }

        public PartialViewResult ReOrderLists(string checkBoxValue, string orderBy, string type)
        {
            if (orderBy == "name")
            {
                if (checkBoxValue == "true")
                {
                    //Asc
                    container.FixedActivities.SetComparatorProperties(ComparableMethods.Asc, ComparableProperties.Name);
                    container.FixedActivities.ReSetElements(Cont.FixedActivities.OrderBy(x => x.Name).ToList());

                    container.UnfixedActivities.SetComparatorProperties(ComparableMethods.Asc, ComparableProperties.Name);
                    container.UnfixedActivities.ReSetElements(Cont.UnfixedActivities.OrderBy(x => x.Name).ToList());

                    container.UndefinedActivities.SetComparatorProperties(ComparableMethods.Asc, ComparableProperties.Name);
                    container.UndefinedActivities.ReSetElements(Cont.UndefinedActivities.OrderBy(x => x.Name).ToList());

                    container.DeadlineActivities.SetComparatorProperties(ComparableMethods.Asc, ComparableProperties.Name);
                    container.DeadlineActivities.ReSetElements(Cont.DeadlineActivities.OrderBy(x => x.Name).ToList());
                }
                else
                {
                    //Desc
                    container.FixedActivities.SetComparatorProperties(ComparableMethods.Desc, ComparableProperties.Name);
                    container.FixedActivities.ReSetElements(Cont.FixedActivities.OrderByDescending(x => x.Name).ToList());

                    container.UnfixedActivities.SetComparatorProperties(ComparableMethods.Desc, ComparableProperties.Name);
                    container.UnfixedActivities.ReSetElements(Cont.UnfixedActivities.OrderByDescending(x => x.Name).ToList());

                    container.UndefinedActivities.SetComparatorProperties(ComparableMethods.Desc, ComparableProperties.Name);
                    container.UndefinedActivities.ReSetElements(Cont.UndefinedActivities.OrderByDescending(x => x.Name).ToList());

                    container.DeadlineActivities.SetComparatorProperties(ComparableMethods.Desc, ComparableProperties.Name);
                    container.DeadlineActivities.ReSetElements(Cont.DeadlineActivities.OrderByDescending(x => x.Name).ToList());
                }
            }
            Debug.WriteLine("Ordered elements:");
            foreach (var i in container.FixedActivities)
            {
                Debug.WriteLine(i);
            }
            return GetActivityData(type);
        }

        private bool IsValidForFixedOrOpt(string type, string optional)
        {
            if (optional == "true")
            {
                if (type == "Deadline")
                    return false;
            }
            else
            {
                if (type == "Undefined")
                    return false;
            }
            return true;
        }

        private void ResetVM()
        {
            container.FixedActivitiesSLI.Clear();
            container.UnfixedActivitiesSLI.Clear();
            container.UndefinedActivitiesSLI.Clear();
            container.DeadlineActivitiesSLI.Clear();

            container.ActivitySelectedByUserForOptional.Clear();
            container.ActivitySelectedByUserForSure.Clear();

            container.FixedActivities.Clear();
            container.UndefinedActivities.Clear();
            container.UnfixedActivities.Clear();
            container.DeadlineActivities.Clear();
            container.VM = new CalendarEditorViewModel();
            container.CurrentUserID = User.Identity.GetUserId();
            container.SelectedDate = null;
            container.IsAsc = true;
            container.Filters.Reset();
        }

        private ActivityType? GetOriginalTypeFromString(string type)
        {
            switch (type)
            {
                case "Fixed":
                    return ActivityType.Fixed;
                case "Unfixed":
                    return ActivityType.Unfixed;
                case "Undefined":
                    return ActivityType.Undefined;
                case "Deadline":
                    return ActivityType.Deadline;
            }

            return null;
        }

        private void SaveValues(string isAsc, string date, string name, string priority, string category, string label)
        {
            container.IsAsc = Boolean.Parse(isAsc);
            if (date != "")
                container.SelectedDate = DateTime.Parse(date);
            else
                container.SelectedDate = null;

            container.Filters = new SaveFilters(name, category, priority, label);
        }

        private void SaveCurrentDayToDB(CalendarEditorViewModel vm)
        {
            List<Activity> fix = container.ActivitySelectedByUserForSure.ToList();
            List<Activity> optional = container.ActivitySelectedByUserForOptional.ToList();

            foreach (Activity i in fix)
            {
                switch (i.OwnType)
                {
                    case ActivityType.Fixed:
                        FixedActivity f = i as FixedActivity;
                        if (f.Repeat == null)
                            f.Repeat = new Repeat(vm.SelectedDate, RepeatPeriod.Single, vm.SelectedDate);
                        break;
                    case ActivityType.Unfixed:
                        UnfixedActivity uf = i as UnfixedActivity;
                        if (uf.Repeat == null)
                            uf.Repeat = new Repeat(vm.SelectedDate, RepeatPeriod.Single, vm.SelectedDate);
                        break;
                }
            } //Set default repeat values

            using (DamaDB db = new DamaDB())
            {
                foreach (Activity activity in fix)
                {
                    switch (activity.OwnType)
                    {
                        case ActivityType.Fixed:
                            FixedActivity fa = activity as FixedActivity;
                            if (fa.Category != null)
                                db.Entry(fa.Category).State = EntityState.Unchanged;
                            fa.Base = false;
                            db.FixedActivities.Add(fa);
                            break;
                        case ActivityType.Unfixed:
                            UnfixedActivity ufa = activity as UnfixedActivity;
                            if (ufa.Category != null)
                                db.Entry(ufa.Category).State = EntityState.Unchanged;
                            ufa.Base = false;
                            db.UnFixedActivities.Add(ufa);
                            break;
                        case ActivityType.Deadline:
                            DeadlineActivity dla = activity as DeadlineActivity;
                            dla.Base = false;
                            db.DeadLineActivities.Add(dla);
                            break;
                    }
                }
                db.SaveChanges();

                foreach (Activity activity in fix)
                    db.Calendar.Add(new CalendarSystem(vm.SelectedDate, activity));

                db.SaveChanges();
            }
        }

        private CalendarEditorViewModel GetValidViewModel()
        {
            CalendarEditorViewModel viewModel;

            using (var container = new ActivityContainer())
            {
                var tmpDate = container.SelectedDate == null ? DateTime.Today : container.SelectedDate.GetValueOrDefault();
                var tmpIsAsc = container.IsAsc;
                var filter = container.Filter;

                if (container.Reset)
                {
                    ResetVM();
                    FillListsFromDB();
                    container.CalendarEditorViewModel.ActivityCollectionForActivityTypes
                                                            .AddRange(container.FixedActivities
                                                                                .OrderBy(a => a.Name)
                                                                                .Select(a => new SelectListItem() { Text = a.Name, Value = a.Id.ToString() }));
                }

                container.CalendarEditorViewModel.SelectedDate = tmpDate;
                container.CalendarEditorViewModel.IsAscendant = tmpIsAsc;
                container.CalendarEditorViewModel.SelectedCategory = filter.Category;
                container.CalendarEditorViewModel.SelectedLabel = filter.Label;
                container.CalendarEditorViewModel.SelectedPriorityFilter = filter.Priority;
                container.CalendarEditorViewModel.Name = filter.Name;
                container.Reset = true;
                viewModel = container.CalendarEditorViewModel;
            }

            return viewModel;
        }
    }
}