using Dama.Data.Enums;
using Dama.Organizer.Models;
using Dama.Web.Attributes;
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

        public PartialViewResult GetActivityDetails(string activityTypeName, string activityid)
        {
            //For the selected listboxes
            if (activityTypeName == "null")
            {
                //In this case the activityid contains two parameters
                string[] p = activityid.Split('|');
                activityTypeName = p[1];
                activityid = p[0];
            }

            //For the first lisbox
            DisplayDetailsViewModel vm = new DisplayDetailsViewModel();
            switch (activityTypeName)
            {
                case "Fixed":
                    FixedActivity fixedA = Cont.FixedActivities.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Fixed).FirstOrDefault();
                    if (fixedA == null)
                        fixedA = Cont.ActivitySelectedByUserForSure.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Fixed).FirstOrDefault() as FixedActivity;
                    if (fixedA == null)
                        fixedA = Cont.ActivitySelectedByUserForOptional.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Fixed).FirstOrDefault() as FixedActivity;
                    vm.fixedActivity = fixedA;
                    break;

                case "Unfixed":
                    UnfixedActivity unfixedA = Cont.UnfixedActivities.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Unfixed).FirstOrDefault();
                    if (unfixedA == null)
                        unfixedA = Cont.ActivitySelectedByUserForSure.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Unfixed).FirstOrDefault() as UnfixedActivity;
                    if (unfixedA == null)
                        unfixedA = Cont.ActivitySelectedByUserForOptional.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Unfixed).FirstOrDefault() as UnfixedActivity;
                    vm.unfixedActivity = unfixedA;
                    break;

                case "Undefined":
                    UndefinedActivity undefinedA = Cont.UndefinedActivities.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Undefined).FirstOrDefault();
                    if (undefinedA == null)
                        undefinedA = Cont.ActivitySelectedByUserForSure.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Undefined).FirstOrDefault() as UndefinedActivity;
                    if (undefinedA == null)
                        undefinedA = Cont.ActivitySelectedByUserForOptional.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Undefined).FirstOrDefault() as UndefinedActivity;
                    vm.undefinedActivity = undefinedA;
                    break;

                case "Deadline":
                    DeadlineActivity deadlineA = Cont.DeadlineActivities.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Deadline).FirstOrDefault();
                    if (deadlineA == null)
                        deadlineA = Cont.ActivitySelectedByUserForSure.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Deadline).FirstOrDefault() as DeadlineActivity;
                    if (deadlineA == null)
                        deadlineA = Cont.ActivitySelectedByUserForOptional.Where(x => x.ID.ToString() == activityid && x.OwnType == ActivityType.Deadline).FirstOrDefault() as DeadlineActivity;
                    vm.deadlineActivity = deadlineA;
                    break;

                default:
                    //DropDownList onchange has never run yet, the default is fixedactivity
                    FixedActivity fixedAct = Cont.FixedActivities.Where(x => x.ID.ToString() == activityid).FirstOrDefault();
                    vm.fixedActivity = fixedAct;
                    break;
            }
            return PartialView("GetActivityDetails", vm);
        }

        public PartialViewResult GetAvailableFilters(string activityTypeName)
        {
            switch (activityTypeName)
            {
                case "Fixed":
                case "Unfixed":
                    Cont.VM.CategoryFilter = new List<SelectListItem>(Cont.Categories.Select(x => new SelectListItem() { Text = x.Name, Value = x.Name }));
                    Cont.VM.CategoryFilter.Add(new SelectListItem() { Text = "Not active", Value = String.Empty, Selected = true });

                    Cont.VM.LabelFilter = (from x in Cont.Labels group x by x.Name into grpdLabel select new SelectListItem() { Text = grpdLabel.Key, Value = grpdLabel.Select(x => x.Name).First() }).ToList();
                    Cont.VM.LabelFilter.Add(new SelectListItem() { Text = "Not active", Value = String.Empty, Selected = true });
                    return PartialView("GetAllFilters", Cont.VM);

                case "Undefined":
                    Cont.VM.CategoryFilter = new List<SelectListItem>(Cont.Categories.Select(x => new SelectListItem() { Text = x.Name, Value = x.Name }));
                    Cont.VM.CategoryFilter.Add(new SelectListItem() { Text = "Not active", Value = String.Empty, Selected = true });

                    Cont.VM.LabelFilter = (from x in Cont.Labels group x by x.Name into grpdLabel select new SelectListItem() { Text = grpdLabel.Key, Value = grpdLabel.Select(x => x.Name).First() }).ToList();
                    Cont.VM.LabelFilter.Add(new SelectListItem() { Text = "Not active", Value = String.Empty, Selected = true });
                    return PartialView("GetReducedFilters", Cont.VM);

                case "Deadline":
                    return null;
            }
            return null;
        }

        public PartialViewResult GetFilteredActivities(string type, string name, string category, string label, string priority, string order)
        {
            Cont.VM.SelectedType = type;
            int parsedPriority = int.MinValue;
            Cont.VM.ActivitySelectedList.Clear();
            switch (type)
            {
                case "Fixed":
                    List<FixedActivity> filteredFixed = new List<FixedActivity>();
                    filteredFixed = Cont.FixedActivities.ToList();

                    if (!name.Equals(String.Empty))
                        filteredFixed = Cont.FixedActivities.Where(x => x.Name.ToLower().Contains(name.ToLower())).ToList();

                    if (!category.Equals(String.Empty))
                        filteredFixed = filteredFixed.Where(x => x.Category != null && x.Category.Name.Equals(category)).ToList();

                    if (!label.Equals(String.Empty))
                        filteredFixed = filteredFixed.Where(x => x.ContainsLabel(label)).ToList();

                    if (!priority.Equals(String.Empty) && int.TryParse(priority, out parsedPriority))
                        filteredFixed = filteredFixed.Where(x => x.Priority.Equals(parsedPriority)).ToList();

                    Cont.VM.ActivitySelectedList.AddRange(filteredFixed.Select(x => new SelectListItem() { Text = x.Name, Value = x.ID.ToString() }));

                    return PartialView("GetActivityData", Cont.VM);

                case "Unfixed":
                    List<UnfixedActivity> filteredUnfixed = Cont.UnfixedActivities.ToList();
                    if (!name.Equals(String.Empty))
                        filteredUnfixed = filteredUnfixed.Where(x => x.Name.ToLower().Contains(name.ToLower())).ToList();

                    if (!category.Equals(String.Empty))
                        filteredUnfixed = filteredUnfixed.Where(x => x.Category != null && x.Category.Name.Equals(category)).ToList();

                    if (!label.Equals(String.Empty))
                        filteredUnfixed = filteredUnfixed.Where(x => x.ContainsLabel(label)).ToList();

                    if (!priority.Equals(String.Empty) && int.TryParse(priority, out parsedPriority))
                        filteredUnfixed = filteredUnfixed.Where(x => x.Priority.Equals(parsedPriority)).ToList();

                    Cont.VM.ActivitySelectedList.AddRange(filteredUnfixed.Select(x => new SelectListItem() { Text = x.Name, Value = x.ID.ToString() }));

                    return PartialView("GetActivityData", Cont.VM);

                case "Undefined":
                    List<UndefinedActivity> filteredUndefined = Cont.UndefinedActivities.ToList();
                    if (!name.Equals(String.Empty))
                        filteredUndefined = filteredUndefined.Where(x => x.Name.ToLower().Contains(name.ToLower())).ToList();

                    if (!category.Equals(String.Empty))
                        filteredUndefined = filteredUndefined.Where(x => x.Category != null && x.Category.Name.Equals(category)).ToList();

                    if (!label.Equals(String.Empty))
                        filteredUndefined = filteredUndefined.Where(x => x.ContainsLabel(label)).ToList();

                    Cont.VM.ActivitySelectedList.AddRange(filteredUndefined.Select(x => new SelectListItem() { Text = x.Name, Value = x.ID.ToString() }));

                    return PartialView("GetActivityData", Cont.VM);
            }

            return null;
        }

        public PartialViewResult GetSelectedActivities(string type, string idList, string forOptional)
        {
            if (!IsValidForFixedOrOpt(type, forOptional))
            {
                ViewBag.AddActIsNotValid = "Cannot add this activity to this list!";
                if (forOptional == "true")
                    return PartialView("GetSelectedOptionalActivities", Cont.VM);
                else
                    return PartialView("GetSelectedActivities", Cont.VM);
            }

            List<int> selectedIDs = SeparateidList(idList);
            if (selectedIDs == null)
                if (forOptional == "true")
                    return PartialView("GetSelectedOptionalActivities", Cont.VM);
                else
                    return PartialView("GetSelectedActivities", Cont.VM);

            Activity tmp = null;

            foreach (int i in selectedIDs)
            {
                switch (type)
                {
                    case "Fixed":
                        tmp = Cont.FixedActivities.Where(x => x.ID == Convert.ToInt32(i)).FirstOrDefault();
                        Cont.FixedActivities.Remove(tmp as FixedActivity);
                        break;

                    case "Unfixed":
                        tmp = Cont.UnfixedActivities.Where(x => x.ID == Convert.ToInt32(i)).FirstOrDefault();
                        Cont.UnfixedActivities.Remove(tmp as UnfixedActivity);
                        break;

                    case "Undefined":
                        tmp = Cont.UndefinedActivities.Where(x => x.ID == Convert.ToInt32(i)).FirstOrDefault();
                        Cont.UndefinedActivities.Remove(tmp as UndefinedActivity);
                        break;

                    case "Deadline":
                        tmp = Cont.DeadlineActivities.Where(x => x.ID == Convert.ToInt32(i)).FirstOrDefault();
                        Cont.DeadlineActivities.Remove(tmp as DeadlineActivity);
                        break;
                }
                if (forOptional == "true")
                {
                    Cont.VM.OptionalActivitiesSelectedByUser.Add(new SelectListItem() { Text = tmp.Name, Value = tmp.ID.ToString() + "|" + type });
                    Cont.VM.OptionalActivitiesSelectedByUser = Cont.VM.OptionalActivitiesSelectedByUser.OrderBy(x => x.Text).ToList();
                    Cont.ActivitySelectedByUserForOptional.AddSorted(tmp);
                    Cont.VM.ActivitySelectedList.RemoveAll(x => x.Value.Split('|')[0] == i.ToString());
                }
                else
                {
                    Cont.VM.ActivitiesSelectedByUser.Add(new SelectListItem() { Text = tmp.Name, Value = tmp.ID.ToString() + "|" + type });
                    Cont.VM.ActivitiesSelectedByUser = Cont.VM.ActivitiesSelectedByUser.OrderBy(x => x.Text).ToList();
                    Cont.ActivitySelectedByUserForSure.AddSorted(tmp);
                    Cont.VM.ActivitySelectedList.RemoveAll(x => x.Value.Split('|')[0] == i.ToString());

                    if (type == "Unfixed")
                    {
                        break;
                    }
                }
            }

            if (forOptional == "true")
                return PartialView("GetSelectedOptionalActivities", Cont.VM);
            else
                return PartialView("GetSelectedActivities", Cont.VM);
        }

        public PartialViewResult MoveBack(string id_type_List, string fromOptional)
        {
            string[] splittedValues = id_type_List.Split(',');
            SelectListItem sli = null;
            Activity tmp = null;

            if (splittedValues == null || id_type_List == "null") //There's nothing selected
                if (fromOptional == "true")
                    return PartialView("GetSelectedOptionalActivities", Cont.VM);
                else
                    return PartialView("GetSelectedActivities", Cont.VM);

            if (fromOptional == "true")
            {
                foreach (string value in splittedValues)
                {
                    foreach (SelectListItem item in Cont.VM.OptionalActivitiesSelectedByUser)
                    {
                        if (item.Value == value)
                        {
                            sli = item;
                            break;
                        }
                    }

                    string[] idNtype = value.Split('|');
                    Cont.VM.OptionalActivitiesSelectedByUser.Remove(sli); //Remove from view list
                    foreach (var i in Cont.ActivitySelectedByUserForOptional)
                    {
                        switch (idNtype[1])
                        {
                            case "Fixed":
                                if (i.ID == int.Parse(idNtype[0]) && (i as FixedActivity) != null)
                                {
                                    Cont.FixedActivities.AddSorted(i as FixedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Unfixed":
                                if (i.ID == int.Parse(idNtype[0]) && (i as UnfixedActivity) != null)
                                {
                                    Cont.UnfixedActivities.AddSorted(i as UnfixedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Undefined":
                                if (i.ID == int.Parse(idNtype[0]) && (i as UndefinedActivity) != null)
                                {
                                    Cont.UndefinedActivities.AddSorted(i as UndefinedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Deadline":
                                if (i.ID == int.Parse(idNtype[0]) && (i as DeadlineActivity) != null)
                                {
                                    Cont.DeadlineActivities.AddSorted(i as DeadlineActivity);
                                    tmp = i;
                                }
                                break;
                        }
                    }
                    //Cont.VM.ActivitySelectedList.Add(new SelectListItem() { Text = tmp.Name, Value = tmp.ID.ToString() });

                    Cont.ActivitySelectedByUserForOptional.Remove(tmp);
                }
            }
            else
            {
                foreach (string value in splittedValues)
                {
                    foreach (SelectListItem item in Cont.VM.ActivitiesSelectedByUser)
                    {
                        if (item.Value == value)
                        {
                            sli = item;
                            break;
                        }
                    }

                    string[] idNtype = value.Split('|');
                    Cont.VM.ActivitiesSelectedByUser.Remove(sli); //Remove from view list
                    foreach (var i in Cont.ActivitySelectedByUserForSure)
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
                                        Cont.UnfixedActivities.AddSorted(ua);
                                    }
                                    else
                                    {
                                        Cont.FixedActivities.AddSorted(current);
                                    }
                                    tmp = i;
                                }
                                break;
                            case "Unfixed":
                                if (i.ID == int.Parse(idNtype[0]) && (i as UnfixedActivity) != null)
                                {
                                    Cont.UnfixedActivities.AddSorted(i as UnfixedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Undefined":
                                if (i.ID == int.Parse(idNtype[0]) && (i as UndefinedActivity) != null)
                                {
                                    Cont.UndefinedActivities.AddSorted(i as UndefinedActivity);
                                    tmp = i;
                                }
                                break;
                            case "Deadline":
                                if (i.ID == int.Parse(idNtype[0]) && (i as DeadlineActivity) != null)
                                {
                                    Cont.DeadlineActivities.AddSorted(i as DeadlineActivity);
                                    tmp = i;
                                }
                                break;
                        }
                    }
                    Cont.ActivitySelectedByUserForSure.Remove(tmp);
                }
            }
            if (fromOptional == "true")
                return PartialView("GetSelectedOptionalActivities", Cont.VM);
            else
                return PartialView("GetSelectedActivities", Cont.VM);
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
            Cont.Reset = false;

            if (ModelState.IsValid)
            {
                UnfixedActivity ua = Cont.ActivitySelectedByUserForSure.FirstOrDefault(i => i.ID == vm.ActivityID && i.OwnType == ActivityType.Unfixed) as UnfixedActivity;
                FixedActivity fa = new FixedActivity(ua, vm.StartTime) { IsUnfixedOriginally = true };
                Cont.ActivitySelectedByUserForSure.Remove(ua);
                Cont.ActivitySelectedByUserForSure.AddSorted(fa);
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
                List<FixedActivity> l = Cont.ActivitySelectedByUserForSure.Where(x => x.OwnType == ActivityType.Fixed).Select(x => x as FixedActivity).ToList();
                AutoFill fill = new AutoFill(l,
                                                Cont.ActivitySelectedByUserForOptional,
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
                Cont.FixedActivities.AddSortedRange(db.FixedActivities.Where(x => x.UserID == Cont.CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).ToList());
                Cont.UnfixedActivities.AddSortedRange(db.UnFixedActivities.Where(x => x.UserID == Cont.CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).ToList());
                Cont.UndefinedActivities.AddSortedRange(db.UndefinedActivities.Where(x => x.UserID == Cont.CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).ToList());
                Cont.DeadlineActivities.AddSortedRange(db.DeadLineActivities.Where(x => x.UserID == Cont.CurrentUserID && x.Base == true).Include(act => act.Labels).Include(act => act.Category).Include(act => act.MileStones).ToList());
                Cont.Categories = db.Categories.Where(x => x.UserID == Cont.CurrentUserID).ToList();
                Cont.Labels = db.Labels.Where(x => x.UserID == Cont.CurrentUserID).ToList();
            }
        }

        private List<int> SeparateidList(string idList)
        {
            return idList == "null" || idList == null ? null : idList.Split(',').Select(x => Convert.ToInt32(x)).ToList();
        }

        public PartialViewResult RefreshActivityListbox()
        {
            return PartialView("GetActivityData", Cont.VM);
        }

        public PartialViewResult RefreshFixList()
        {
            Cont.VM.ActivitiesSelectedByUser.Clear();
            foreach (Activity i in Cont.ActivitySelectedByUserForSure)
            {
                Cont.VM.ActivitiesSelectedByUser.Add(new SelectListItem() { Text = i.Name, Value = i.ID.ToString() + "|" + i.OwnType });
            }
            Cont.VM.ActivitiesSelectedByUser = Cont.VM.ActivitiesSelectedByUser.OrderBy(x => x.Text).ToList();
            return PartialView("GetSelectedActivities", Cont.VM);
        }

        public PartialViewResult RefreshOptionalList()
        {
            Cont.VM.OptionalActivitiesSelectedByUser.Clear();
            foreach (Activity i in Cont.ActivitySelectedByUserForOptional)
            {
                Cont.VM.OptionalActivitiesSelectedByUser.Add(new SelectListItem() { Text = i.Name, Value = i.ID.ToString() + "|" + i.OwnType });
            }
            Cont.VM.OptionalActivitiesSelectedByUser = Cont.VM.OptionalActivitiesSelectedByUser.OrderBy(x => x.Text).ToList();
            return PartialView("GetSelectedOptionalActivities", Cont.VM);
        }

        public PartialViewResult ReOrderLists(string checkBoxValue, string orderBy, string type)
        {
            if (orderBy == "name")
            {
                if (checkBoxValue == "true")
                {
                    //Asc
                    Cont.FixedActivities.SetComparatorProperties(ComparableMethods.Asc, ComparableProperties.Name);
                    Cont.FixedActivities.ReSetElements(Cont.FixedActivities.OrderBy(x => x.Name).ToList());

                    Cont.UnfixedActivities.SetComparatorProperties(ComparableMethods.Asc, ComparableProperties.Name);
                    Cont.UnfixedActivities.ReSetElements(Cont.UnfixedActivities.OrderBy(x => x.Name).ToList());

                    Cont.UndefinedActivities.SetComparatorProperties(ComparableMethods.Asc, ComparableProperties.Name);
                    Cont.UndefinedActivities.ReSetElements(Cont.UndefinedActivities.OrderBy(x => x.Name).ToList());

                    Cont.DeadlineActivities.SetComparatorProperties(ComparableMethods.Asc, ComparableProperties.Name);
                    Cont.DeadlineActivities.ReSetElements(Cont.DeadlineActivities.OrderBy(x => x.Name).ToList());
                }
                else
                {
                    //Desc
                    Cont.FixedActivities.SetComparatorProperties(ComparableMethods.Desc, ComparableProperties.Name);
                    Cont.FixedActivities.ReSetElements(Cont.FixedActivities.OrderByDescending(x => x.Name).ToList());

                    Cont.UnfixedActivities.SetComparatorProperties(ComparableMethods.Desc, ComparableProperties.Name);
                    Cont.UnfixedActivities.ReSetElements(Cont.UnfixedActivities.OrderByDescending(x => x.Name).ToList());

                    Cont.UndefinedActivities.SetComparatorProperties(ComparableMethods.Desc, ComparableProperties.Name);
                    Cont.UndefinedActivities.ReSetElements(Cont.UndefinedActivities.OrderByDescending(x => x.Name).ToList());

                    Cont.DeadlineActivities.SetComparatorProperties(ComparableMethods.Desc, ComparableProperties.Name);
                    Cont.DeadlineActivities.ReSetElements(Cont.DeadlineActivities.OrderByDescending(x => x.Name).ToList());
                }
            }
            Debug.WriteLine("Ordered elements:");
            foreach (var i in Cont.FixedActivities)
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
            Cont.FixedActivitiesSLI.Clear();
            Cont.UnfixedActivitiesSLI.Clear();
            Cont.UndefinedActivitiesSLI.Clear();
            Cont.DeadlineActivitiesSLI.Clear();

            Cont.ActivitySelectedByUserForOptional.Clear();
            Cont.ActivitySelectedByUserForSure.Clear();

            Cont.FixedActivities.Clear();
            Cont.UndefinedActivities.Clear();
            Cont.UnfixedActivities.Clear();
            Cont.DeadlineActivities.Clear();
            Cont.VM = new CalendarEditorViewModel();
            Cont.CurrentUserID = User.Identity.GetUserId();
            Cont.SelectedDate = null;
            Cont.IsAsc = true;
            Cont.Filters.Reset();
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
            Cont.IsAsc = Boolean.Parse(isAsc);
            if (date != "")
                Cont.SelectedDate = DateTime.Parse(date);
            else
                Cont.SelectedDate = null;

            Cont.Filters = new SaveFilters(name, category, priority, label);
        }

        private void SaveCurrentDayToDB(CalendarEditorViewModel vm)
        {
            List<Activity> fix = Cont.ActivitySelectedByUserForSure.ToList();
            List<Activity> optional = Cont.ActivitySelectedByUserForOptional.ToList();

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