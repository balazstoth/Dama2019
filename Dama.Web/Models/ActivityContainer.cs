using Dama.Data.Models;
using Dama.Organizer.Enums;
using Dama.Organizer.Others;
using Dama.Web.Models.ViewModels.Editor;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Filter = Dama.Web.Models.Filter;

namespace Dama.Organizer.Models
{
    public class ActivityContainer : IDisposable
    {
        private readonly string _propertyName;

        #region Properties
        public OrderedLinkedList<FixedActivity> FixedActivities { get; set; }

        public OrderedLinkedList<UnfixedActivity> UnfixedActivities { get; set; }

        public OrderedLinkedList<UndefinedActivity> UndefinedActivities { get; set; }

        public OrderedLinkedList<DeadlineActivity> DeadlineActivities { get; set; }

        public OrderedLinkedList<Activity> ActivitySelectedByUserForSure { get; set; }

        public OrderedLinkedList<Activity> ActivitySelectedByUserForOptional { get; set; }

        public List<Category> Categories { get; set; }

        public List<Label> Labels { get; set; }

        public List<SelectListItem> FixedActivitiesSLI { get; set; }

        public List<SelectListItem> UnfixedActivitiesSLI { get; set; }

        public List<SelectListItem> UndefinedActivitiesSLI { get; set; }

        public List<SelectListItem> DeadlineActivitiesSLI { get; set; }

        public string UserId { get; set; }

        public CalendarEditorViewModel CalendarEditorViewModel { get; set; }

        public bool Reset { get; set; }

        public bool IsAsc { get; set; }

        public DateTime? SelectedDate { get; set; }

        public Filter Filter { get; set; }
        #endregion

        public ActivityContainer(string propertyName = nameof(ActivityContainer))
        {
            _propertyName = propertyName;
            var sessionContainer = HttpContext.Current.Session[propertyName];

            if(sessionContainer == null)
            {
                FixedActivities = new OrderedLinkedList<FixedActivity>(ComparableSequence.Asc, ComparableProperties.Name);
                UnfixedActivities = new OrderedLinkedList<UnfixedActivity>(ComparableSequence.Asc, ComparableProperties.Name);
                UndefinedActivities = new OrderedLinkedList<UndefinedActivity>(ComparableSequence.Asc, ComparableProperties.Name);
                DeadlineActivities = new OrderedLinkedList<DeadlineActivity>(ComparableSequence.Asc, ComparableProperties.Name);
                ActivitySelectedByUserForOptional = new OrderedLinkedList<Activity>(ComparableSequence.Asc, ComparableProperties.Name);
                ActivitySelectedByUserForSure = new OrderedLinkedList<Activity>(ComparableSequence.Asc, ComparableProperties.Name);

                FixedActivitiesSLI = new List<SelectListItem>();
                UnfixedActivitiesSLI = new List<SelectListItem>();
                UndefinedActivitiesSLI = new List<SelectListItem>();
                DeadlineActivitiesSLI = new List<SelectListItem>();

                CalendarEditorViewModel = new CalendarEditorViewModel();
                Reset = true;
                SelectedDate = null;
                IsAsc = true;
                Filter = new Filter();
            }
            else
            {
                Initialize(sessionContainer);
            }
        }

        public void Dispose()
        {
            HttpContext.Current.Session[_propertyName] = this;
        }

        private void Initialize(object session)
        {
            var container = session as ActivityContainer;

            FixedActivities = container.FixedActivities ?? new OrderedLinkedList<FixedActivity>(ComparableSequence.Asc, ComparableProperties.Name);
            UnfixedActivities = container.UnfixedActivities ?? new OrderedLinkedList<UnfixedActivity>(ComparableSequence.Asc, ComparableProperties.Name);
            UndefinedActivities = container.UndefinedActivities ?? new OrderedLinkedList<UndefinedActivity>(ComparableSequence.Asc, ComparableProperties.Name);
            DeadlineActivities = container.DeadlineActivities ?? new OrderedLinkedList<DeadlineActivity>(ComparableSequence.Asc, ComparableProperties.Name);

            ActivitySelectedByUserForOptional = container.ActivitySelectedByUserForOptional ?? new OrderedLinkedList<Activity>(ComparableSequence.Asc, ComparableProperties.Name);
            ActivitySelectedByUserForSure = container.ActivitySelectedByUserForSure ?? new OrderedLinkedList<Activity>(ComparableSequence.Asc, ComparableProperties.Name);

            FixedActivitiesSLI = container.FixedActivitiesSLI ?? new List<SelectListItem>();
            UnfixedActivitiesSLI = container.UnfixedActivitiesSLI ?? new List<SelectListItem>();
            UndefinedActivitiesSLI = container.UndefinedActivitiesSLI ?? new List<SelectListItem>();
            DeadlineActivitiesSLI = container.DeadlineActivitiesSLI ?? new List<SelectListItem>();

            CalendarEditorViewModel = container.CalendarEditorViewModel ?? new CalendarEditorViewModel();
            Reset = container.Reset;
            IsAsc = container.IsAsc;
            SelectedDate = container.SelectedDate;
            Filter = container.Filter ?? new Filter();
        }
    }
}