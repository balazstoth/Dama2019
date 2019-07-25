﻿using Dama.Data;
using Dama.Data.Enums;
using Dama.Data.Models;
using Dama.Organizer.Models;
using Dama.Web.Attributes;
using Dama.Web.Models.ViewModels.Activity.Display;
using Dama.Web.Models.ViewModels.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Dama.Web.Models.ViewModels;
using Dama.Generate;
using Dama.Organizer.Enums;
using Microsoft.AspNet.Identity;
using Filter = Dama.Web.Models.Filter;
using Repeat = Dama.Data.Models.Repeat;
using Error = Dama.Organizer.Resources.Error;
using ActionNames = Dama.Organizer.Enums.ActionNames;
using ControllerNames = Dama.Organizer.Enums.ControllerNames;
using ViewNames = Dama.Organizer.Enums.ViewNames;
using Dama.Data.Interfaces;
using System.Data.Entity;

namespace Dama.Web.Controllers
{
    [Authorize]
    [DisableUser]
    public class CalendarEditorController : BaseController
    {
        private const string _fixedActivityName = "FixedActivity";
        private const string _unfixedActivityName = "UnfixedActivity";
        private const string _undefinedActivityName = "UndefinedActivity";
        private const string _deadlineActivityName = "DeadlineActivity";

        private readonly IContentRepository _repositories;

        public CalendarEditorController(IContentRepository contentRepository)
        {
            _repositories = contentRepository;
        }

        /// <summary>
        /// Gives basic data for the Editor view
        /// </summary>
        public ActionResult Editor() 
        {
            var viewModel = GetValidViewModelAsync();
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

        public PartialViewResult GetSelectedActivities(string activityType, string idCollection, string forOptional)
        {
            CalendarEditorViewModel viewModel = null;
            var container = new ActivityContainer();

            if (!IsValidForFixedOrOptional(activityType, forOptional))
            {
                ViewBag.AddActIsNotValid = Error.AddActivityToListDenied;

                if (forOptional == "true")
                    return PartialView(ViewNames.GetSelectedOptionalActivities.ToString(), container.CalendarEditorViewModel);

                return PartialView(ViewNames.GetSelectedActivities.ToString(), container.CalendarEditorViewModel);
            }

            var selectedIds = SeparateIdCollection(idCollection);

            if (selectedIds == null)
            {
                if (forOptional == "true")
                    return PartialView(ViewNames.GetSelectedOptionalActivities.ToString(), container.CalendarEditorViewModel);
                
                return PartialView(ViewNames.GetSelectedActivities.ToString(), container.CalendarEditorViewModel);
            }

            Activity tmpActivity = null;

            foreach (int id in selectedIds)
            {
                switch (activityType)
                {
                    case _fixedActivityName:
                        tmpActivity = container.FixedActivities.Where(x => x.Id == Convert.ToInt32(id)).FirstOrDefault();
                        container.FixedActivities.Remove(tmpActivity as FixedActivity);
                        break;

                    case _unfixedActivityName:
                        tmpActivity = container.UnfixedActivities.Where(x => x.Id == Convert.ToInt32(id)).FirstOrDefault();
                        container.UnfixedActivities.Remove(tmpActivity as UnfixedActivity);
                        break;

                    case _undefinedActivityName:
                        tmpActivity = container.UndefinedActivities.Where(x => x.Id == Convert.ToInt32(id)).FirstOrDefault();
                        container.UndefinedActivities.Remove(tmpActivity as UndefinedActivity);
                        break;

                    case _deadlineActivityName:
                        tmpActivity = container.DeadlineActivities.Where(x => x.Id == Convert.ToInt32(id)).FirstOrDefault();
                        container.DeadlineActivities.Remove(tmpActivity as DeadlineActivity);
                        break;
                }

                if (forOptional == "true")
                {
                    container.CalendarEditorViewModel
                             .OptionalActivitiesSelectedByUser
                                .Add(new SelectListItem()
                                {
                                    Text = tmpActivity.Name,
                                    Value = tmpActivity.Id.ToString() + "|" + activityType
                                });

                    container.CalendarEditorViewModel
                             .OptionalActivitiesSelectedByUser = container.CalendarEditorViewModel
                                                                          .OptionalActivitiesSelectedByUser
                                                                             .OrderBy(a => a.Text).ToList();

                    container.ActivitySelectedByUserForOptional.AddSorted(tmpActivity);
                    container.CalendarEditorViewModel
                             .ActivityCollectionForActivityTypes
                                .RemoveAll(a => a.Value.Split('|')[0] == id.ToString());
                }
                else
                {
                    container.CalendarEditorViewModel
                             .MandatoryActivitiesSelectedByUser
                                .Add(new SelectListItem() { Text = tmpActivity.Name, Value = tmpActivity.Id.ToString() + "|" + activityType });

                    container.CalendarEditorViewModel
                             .MandatoryActivitiesSelectedByUser = container.CalendarEditorViewModel
                                                                           .MandatoryActivitiesSelectedByUser
                                                                               .OrderBy(a => a.Text).ToList();

                    container.ActivitySelectedByUserForSure.AddSorted(tmpActivity);
                    container.CalendarEditorViewModel
                             .ActivityCollectionForActivityTypes
                                 .RemoveAll(a => a.Value.Split('|')[0] == id.ToString());

                    if (activityType == _unfixedActivityName)
                        break;
                }
            }

            viewModel = container.CalendarEditorViewModel;
            container.Dispose();

            if (forOptional == "true")
                return PartialView(ViewNames.GetSelectedOptionalActivities.ToString(), viewModel);

            return PartialView(ViewNames.GetSelectedActivities.ToString(), viewModel);
        }

        public PartialViewResult MoveBack(string idAndTypeCollection, string fromOptional)
        {
            var splittedValues = idAndTypeCollection.Split(',');
            SelectListItem sli = null;
            Activity tmp = null;
            var container = new ActivityContainer();
            CalendarEditorViewModel viewModel;

            //There's nothing selected
            if (splittedValues == null || idAndTypeCollection == "null") 
            {
                if (fromOptional == "true")
                    return PartialView(ViewNames.GetSelectedOptionalActivities.ToString(), container.CalendarEditorViewModel);

                return PartialView(ViewNames.GetSelectedActivities.ToString(), container.CalendarEditorViewModel);
            }
                
            if (fromOptional == "true")
            {
                foreach (var value in splittedValues)
                {
                    foreach (var item in container.CalendarEditorViewModel.OptionalActivitiesSelectedByUser)
                    {
                        if (item.Value == value)
                        {
                            sli = item;
                            break;
                        }
                    }

                    var idAndType = value.Split('|');
                    container.CalendarEditorViewModel.OptionalActivitiesSelectedByUser.Remove(sli); //Remove from view list

                    foreach (var activity in container.ActivitySelectedByUserForOptional)
                    {
                        switch (idAndType[1])
                        {
                            case _fixedActivityName:
                                if (activity.Id == int.Parse(idAndType[0]) && (activity as FixedActivity) != null)
                                {
                                    container.FixedActivities.AddSorted(activity as FixedActivity);
                                    tmp = activity;
                                }

                                break;

                            case _unfixedActivityName:
                                if (activity.Id == int.Parse(idAndType[0]) && (activity as UnfixedActivity) != null)
                                {
                                    container.UnfixedActivities.AddSorted(activity as UnfixedActivity);
                                    tmp = activity;
                                }

                                break;

                            case _undefinedActivityName:
                                if (activity.Id == int.Parse(idAndType[0]) && (activity as UndefinedActivity) != null)
                                {
                                    container.UndefinedActivities.AddSorted(activity as UndefinedActivity);
                                    tmp = activity;
                                }

                                break;

                            case _deadlineActivityName:
                                if (activity.Id == int.Parse(idAndType[0]) && (activity as DeadlineActivity) != null)
                                {
                                    container.DeadlineActivities.AddSorted(activity as DeadlineActivity);
                                    tmp = activity;
                                }

                                break;
                        }
                    }

                    container.ActivitySelectedByUserForOptional.Remove(tmp);
                }
            }
            else
            {
                foreach (var value in splittedValues)
                {
                    foreach (var activity in container.CalendarEditorViewModel.MandatoryActivitiesSelectedByUser)
                    {
                        if (activity.Value == value)
                        {
                            sli = activity;
                            break;
                        }
                    }

                    var idAndType = value.Split('|');
                    container.CalendarEditorViewModel.MandatoryActivitiesSelectedByUser.Remove(sli); //Remove from view list

                    foreach (var activity in container.ActivitySelectedByUserForSure)
                    {
                        switch (idAndType[1])
                        {
                            case _fixedActivityName:
                                if (activity.Id == int.Parse(idAndType[0]) && (activity as FixedActivity) != null)
                                {
                                    var currentActivity = activity as FixedActivity;
                                    if (currentActivity.IsUnfixedOriginally)
                                    {
                                        var unfixedActivity = new UnfixedActivity(currentActivity);
                                        container.UnfixedActivities.AddSorted(unfixedActivity);
                                    }
                                    else
                                    {
                                        container.FixedActivities.AddSorted(currentActivity);
                                    }

                                    tmp = activity;
                                }

                                break;

                            case _unfixedActivityName:
                                if (activity.Id == int.Parse(idAndType[0]) && (activity as UnfixedActivity) != null)
                                {
                                    container.UnfixedActivities.AddSorted(activity as UnfixedActivity);
                                    tmp = activity;
                                }

                                break;

                            case _undefinedActivityName:
                                if (activity.Id == int.Parse(idAndType[0]) && (activity as UndefinedActivity) != null)
                                {
                                    container.UndefinedActivities.AddSorted(activity as UndefinedActivity);
                                    tmp = activity;
                                }

                                break;

                            case _deadlineActivityName:
                                if (activity.Id == int.Parse(idAndType[0]) && (activity as DeadlineActivity) != null)
                                {
                                    container.DeadlineActivities.AddSorted(activity as DeadlineActivity);
                                    tmp = activity;
                                }

                                break;
                        }
                    }

                    container.ActivitySelectedByUserForSure.Remove(tmp);
                }
            }

            viewModel = container.CalendarEditorViewModel;
            container.Dispose();

            if (fromOptional == "true")
                return PartialView(ViewNames.GetSelectedOptionalActivities.ToString(), viewModel);

            return PartialView(ViewNames.GetSelectedActivities.ToString(), viewModel);
        }

        public async Task<ActionResult> GetDataForChange(string idAndType, string isAsc, string selectedDate, string nameFilter, string priorityFilter, string labelFilter, string categoryFilter)
        {
            if (idAndType == "null") //There's nothing selected
                return null;

            SaveValues(isAsc, selectedDate, nameFilter, priorityFilter, categoryFilter, labelFilter);

            string[] firstId;

            if (idAndType.Contains(','))
                firstId = idAndType.Substring(0, idAndType.IndexOf(',')).Split('|');
            else
                firstId = idAndType.Split('|');

            var id = firstId[0];
            var type = firstId[1];
            var optional = firstId[2] == "true";

            var controller = DependencyResolver.Current.GetService<CalendarController>();
            controller.ControllerContext = new ControllerContext(Request.RequestContext, controller);
            return await controller.EditActivity(id, GetOriginalTypeFromString(type), true, optional);
        }

        public ActionResult RequestStartTime(string value)
        {
            if (value == "null")
                return null;

            string firstId;

            if (value.Contains(','))
                firstId = value.Substring(0, value.IndexOf(','));
            else
                firstId = value;

            var viewModel = new RequestTimeViewModel(int.Parse(firstId));
            return PartialView(ViewNames.RequestStartTime.ToString(), viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestStartTime(RequestTimeViewModel viewModel)
        {
            using (var container = new ActivityContainer())
            {
                container.Reset = false;

                if (ModelState.IsValid)
                {
                    var unfixedActivity = container.ActivitySelectedByUserForSure
                                                        .FirstOrDefault(a => a.Id == viewModel.ActivityId && a.ActivityType == ActivityType.UnfixedActivity) as UnfixedActivity;

                    var fixedActivity = new FixedActivity(viewModel.StartTime, unfixedActivity) { IsUnfixedOriginally = true };
                    container.ActivitySelectedByUserForSure.Remove(unfixedActivity);
                    container.ActivitySelectedByUserForSure.AddSorted(fixedActivity);
                }
                else
                {
                    return View();
                }
            }

            return RedirectToAction(ActionNames.Editor.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(CalendarEditorViewModel viewModel)
        {
            if (ModelState.Values.First().Errors.Count == 0)
            {
                SaveCurrentDayToDatabase(viewModel);
                using (var container = new ActivityContainer())
                {
                    var fixedActivityCollection = container.ActivitySelectedByUserForSure.Where(a => a.ActivityType == ActivityType.FixedActivity).Select(x => x as FixedActivity).ToList();
                    var autoFillFeature = new AutoFill(fixedActivityCollection,
                                                        container.ActivitySelectedByUserForOptional,
                                                        new DateTime(DateTime.Now.Year, DateTime.Today.Month, DateTime.Today.Day, 8, 0, 0),
                                                        new DateTime(DateTime.Now.Year, DateTime.Today.Month, DateTime.Today.Day, 20, 0, 0),
                                                        new TimeSpan(0, 5, 0));
                }
            }
            else
            {
                return View(viewModel);
            }

            return RedirectToAction(ActionNames.Index.ToString(), ControllerNames.Home.ToString());
        }

        private async Task FillCollectionsFromDatabase()
        {
            using (var container = new ActivityContainer())
            {
                container.FixedActivities.AddSortedRange(
                    await (_repositories.FixedActivitySqlRepository
                                    .FindByExpressionAsync(t => t
                                        .Where(a => a.UserId == container.UserId && a.BaseActivity)
                                        .Include(a => a.LabelCollection)
                                        .Include(a => a.Category).ToListAsync())));

                container.UnfixedActivities.AddSortedRange(
                    await (_repositories.UnfixedActivitySqlRepository
                                    .FindByExpressionAsync(t => t
                                        .Where(a => a.UserId == container.UserId && a.BaseActivity)
                                        .Include(a => a.LabelCollection)
                                        .Include(a => a.Category).ToListAsync())));



                container.UndefinedActivities.AddSortedRange(
                    await (_repositories.UndefinedActivitySqlRepository
                                    .FindByExpressionAsync(t => t
                                        .Where(a => a.UserId == container.UserId && a.BaseActivity)
                                        .Include(a => a.LabelCollection)
                                        .Include(a => a.Category).ToListAsync())));


                container.DeadlineActivities.AddSortedRange(
                    await (_repositories.DeadlineActivitySqlRepository
                                    .FindByExpressionAsync(t => t
                                        .Where(a => a.UserId == container.UserId && a.BaseActivity)
                                        .Include(a => a.LabelCollection)
                                        .Include(a => a.Category)
                                        .Include(a => a.Milestones).ToListAsync())));

                container.Categories = await (_repositories.CategorySqlRepository
                                                                .FindByExpressionAsync(t => t
                                                                    .Where(a => a.UserId == container.UserId).ToListAsync()));

                container.Labels = await (_repositories.LabelSqlRepository
                                                            .FindByExpressionAsync(t => t
                                                                .Where(a => a.UserId == container.UserId).ToListAsync()));
            }
        }

        public PartialViewResult RefreshActivityListbox()
        {
            var container = new ActivityContainer();
            return PartialView(ViewNames.GetActivityData.ToString(), container.CalendarEditorViewModel);
        }

        public PartialViewResult RefreshFixList()
        {
            using (var container = new ActivityContainer())
            {
                container.CalendarEditorViewModel.MandatoryActivitiesSelectedByUser.Clear();

                foreach (var activity in container.ActivitySelectedByUserForSure)
                    container.CalendarEditorViewModel
                             .MandatoryActivitiesSelectedByUser
                                .Add(new SelectListItem()
                                {
                                    Text = activity.Name,
                                    Value = activity.Id.ToString() + "|" + activity.ActivityType
                                });

                container.CalendarEditorViewModel
                         .MandatoryActivitiesSelectedByUser = container.CalendarEditorViewModel
                                                                       .MandatoryActivitiesSelectedByUser
                                                                       .OrderBy(a => a.Text).ToList();

                return PartialView(ViewNames.GetSelectedActivities.ToString(), container.CalendarEditorViewModel);
            }
        }

        public PartialViewResult RefreshOptionalList()
        {
            using (var container = new ActivityContainer())
            {
                container.CalendarEditorViewModel.OptionalActivitiesSelectedByUser.Clear();

                foreach (var activity in container.ActivitySelectedByUserForOptional)
                    container.CalendarEditorViewModel
                             .OptionalActivitiesSelectedByUser
                                .Add(new SelectListItem()
                                {
                                    Text = activity.Name,
                                    Value = activity.Id.ToString() + "|" + activity.ActivityType
                                });

                container.CalendarEditorViewModel
                         .OptionalActivitiesSelectedByUser = container.CalendarEditorViewModel
                                                                      .OptionalActivitiesSelectedByUser
                                                                      .OrderBy(a => a.Text).ToList();

                return PartialView(ViewNames.GetSelectedOptionalActivities.ToString(), container.CalendarEditorViewModel);
            }
        }

        public PartialViewResult ReOrderLists(string checkBoxValue, string orderBy, string activityType)
        {
            using (var container = new ActivityContainer())
            {
                if (orderBy == "name")
                {
                    if (checkBoxValue == "true")
                    {
                        //Asc
                        container.FixedActivities.SetComparatorProperties(ComparableSequence.Asc, ComparableProperties.Name);
                        container.FixedActivities.ResetElements(container.FixedActivities.OrderBy(x => x.Name).ToList());

                        container.UnfixedActivities.SetComparatorProperties(ComparableSequence.Asc, ComparableProperties.Name);
                        container.UnfixedActivities.ResetElements(container.UnfixedActivities.OrderBy(x => x.Name).ToList());

                        container.UndefinedActivities.SetComparatorProperties(ComparableSequence.Asc, ComparableProperties.Name);
                        container.UndefinedActivities.ResetElements(container.UndefinedActivities.OrderBy(x => x.Name).ToList());

                        container.DeadlineActivities.SetComparatorProperties(ComparableSequence.Asc, ComparableProperties.Name);
                        container.DeadlineActivities.ResetElements(container.DeadlineActivities.OrderBy(x => x.Name).ToList());
                    }
                    else
                    {
                        //Desc
                        container.FixedActivities.SetComparatorProperties(ComparableSequence.Desc, ComparableProperties.Name);
                        container.FixedActivities.ResetElements(container.FixedActivities.OrderByDescending(x => x.Name).ToList());

                        container.UnfixedActivities.SetComparatorProperties(ComparableSequence.Desc, ComparableProperties.Name);
                        container.UnfixedActivities.ResetElements(container.UnfixedActivities.OrderByDescending(x => x.Name).ToList());

                        container.UndefinedActivities.SetComparatorProperties(ComparableSequence.Desc, ComparableProperties.Name);
                        container.UndefinedActivities.ResetElements(container.UndefinedActivities.OrderByDescending(x => x.Name).ToList());

                        container.DeadlineActivities.SetComparatorProperties(ComparableSequence.Desc, ComparableProperties.Name);
                        container.DeadlineActivities.ResetElements(container.DeadlineActivities.OrderByDescending(x => x.Name).ToList());
                    }
                }
            }

            return GetActivityData(activityType);
        }

        private bool IsValidForFixedOrOptional(string activityType, string optional)
        {
            if (optional == "true")
                if (activityType == _deadlineActivityName)
                    return false;
            else
                if (activityType == _undefinedActivityName)
                    return false;

            return true;
        }

        private void ResetViewModel()
        {
            using (var container = new ActivityContainer())
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
                container.CalendarEditorViewModel = new CalendarEditorViewModel();
                container.UserId = User.Identity.GetUserId();
                container.SelectedDate = null;
                container.IsAsc = true;
                container.Filter.ResetValues();
            }
        }

        private ActivityType? GetOriginalTypeFromString(string activityType)
        {
            switch (activityType)
            {
                case _fixedActivityName:
                    return ActivityType.FixedActivity;

                case _unfixedActivityName:
                    return ActivityType.UnfixedActivity;

                case _undefinedActivityName:
                    return ActivityType.UndefinedActivity;

                case _deadlineActivityName:
                    return ActivityType.DeadlineActivity;

                default:
                    return null;
            }
        }

        private List<int> SeparateIdCollection(string idCollection)
        {
            return (idCollection == "null" ||
                    idCollection == null) ?
                null : idCollection.Split(',').Select(id => Convert.ToInt32(id)).ToList();
        }

        private void SaveValues(string isAsc, string date, string name, string priority, string category, string label)
        {
            using (var container = new ActivityContainer())
            {
                container.IsAsc = Boolean.Parse(isAsc);

                if (date == "")
                    container.SelectedDate = null;
                else
                    container.SelectedDate = DateTime.Parse(date);

                container.Filter = new Filter()
                {
                    Name = name,
                    Category = category,
                    Priority = priority,
                    Label = label
                };
            }
        }

        private void SaveCurrentDayToDatabase(CalendarEditorViewModel viewModel)
        {
            var container = new ActivityContainer();
            var fixCollection = container.ActivitySelectedByUserForSure.ToList();
            var optionalCollection = container.ActivitySelectedByUserForOptional.ToList();
            var repeat = new Repeat(viewModel.SelectedDate, viewModel.SelectedDate, RepeatPeriod.Single);

            foreach (var activity in fixCollection)
            {
                switch (activity.ActivityType)
                {
                    case ActivityType.FixedActivity:
                        var fixedActivity = activity as FixedActivity;

                        if (fixedActivity.Repeat == null)
                            fixedActivity.Repeat = repeat;

                        break;

                    case ActivityType.UnfixedActivity:
                        var unfixedActivity = activity as UnfixedActivity;

                        if (unfixedActivity.Repeat == null)
                            unfixedActivity.Repeat = repeat;

                        break;
                }
            }
            
            foreach (var activity in fixCollection)
            {
                switch (activity.ActivityType)
                {
                    case ActivityType.FixedActivity:
                        var fixedActivity = activity as FixedActivity;

                        if (fixedActivity.Category != null)
                           _repositories.RepositorySettings.ChangeCategoryEntryState(fixedActivity.Category, EntityState.Unchanged);

                        fixedActivity.BaseActivity = false;
                         _repositories.FixedActivitySqlRepository.AddAsync(fixedActivity);
                        break;

                    case ActivityType.UnfixedActivity:
                        var unfixedActivity = activity as UnfixedActivity;

                        if (unfixedActivity.Category != null)
                            _repositories.RepositorySettings.ChangeCategoryEntryState(unfixedActivity.Category, EntityState.Unchanged);

                        unfixedActivity.BaseActivity = false;
                        _repositories.UnfixedActivitySqlRepository.AddAsync(unfixedActivity);
                        break;

                    case ActivityType.DeadlineActivity:
                        var deadlineActivity = activity as DeadlineActivity;
                        deadlineActivity.BaseActivity = false;
                        _repositories.DeadlineActivitySqlRepository.AddAsync(deadlineActivity);
                        break;
                }
            }

            //foreach (var activity in fixCollection)
            //    db.Calendar.Add(new CalendarSystem(viewModel.SelectedDate, activity));

        }

        private async Task<CalendarEditorViewModel> GetValidViewModelAsync()
        {
            CalendarEditorViewModel viewModel;

            using (var container = new ActivityContainer())
            {
                var tmpDate = container.SelectedDate == null ? DateTime.Today : container.SelectedDate.GetValueOrDefault();
                var tmpIsAsc = container.IsAsc;
                var filter = container.Filter;

                if (container.Reset)
                {
                    ResetViewModel();
                    await FillCollectionsFromDatabase();
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