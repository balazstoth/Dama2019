using Dama.Data.Enums;
using Dama.Data.Interfaces;
using Dama.Data.Models;
using Dama.Data.Sql.Interfaces;
using System;
using System.Collections.Generic;

namespace Dama.Organizer
{
    public class ActivityQuery
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _userId;
        private readonly DateTime _today;
        private readonly DateTime _rangeStart;
        private readonly DateTime _rangeEnd;

        public ActivityQuery(IUnitOfWork unitOfWork, string userId)
        {
            _today = DateTime.Today.Date;
            _rangeStart = _today.AddYears(-1);
            _rangeEnd = _today.AddYears(1);
            _unitOfWork = unitOfWork;
            _userId = userId;
        }

        public List<Activity> GetActivities()
        {
            var originalItems = SelectItemsFromDb();
            var finalList = new List<Activity>();

            foreach (var item in originalItems)
            {
                if(!(item is DeadlineActivity))
                    finalList.Add(item);

                switch (item.ActivityType)
                {
                    case ActivityType.FixedActivity:
                    case ActivityType.UnfixedActivity:
                        ManageSharingWithDefinedActivities(item as IDefinedActivity, finalList);
                        break;

                    case ActivityType.DeadlineActivity:
                        ManageSharingWithDeadlineActivities(item as DeadlineActivity, finalList);
                        break;
                }
            }

            return finalList;
        }

        private List<Activity> SelectItemsFromDb()
        {
            var activities = new List<Activity>();

            activities.AddRange(_unitOfWork.FixedActivityRepository.Get(a => a.UserId == _userId && 
                                                                            !a.BaseActivity && 
                                                                            IsActivityInRange(a.Start.Value), null,
                                                                            a => a.Category, a => a.Labels, a => a.Repeat));

            activities.AddRange(_unitOfWork.UnfixedActivityRepository.Get(a => a.UserId == _userId && !a.BaseActivity && IsActivityInRange(a.Start.Value), null,
                                                                               a => a.Category, a => a.Labels, a => a.Repeat));

            activities.AddRange(_unitOfWork.UndefinedActivityRepository.Get(a => a.UserId == _userId && !a.BaseActivity && IsActivityInRange(a.Start.Value)));
            activities.AddRange(_unitOfWork.DeadlineActivityRepository.Get(a => a.UserId == _userId && !a.BaseActivity && !IsDeadLineInRange(a), null, a => a.Milestones));

            return activities;
        }

        private bool IsActivityInRange(DateTime start)
        {
            return _rangeStart <= start && start <= _rangeEnd;
        }

        private bool IsDeadLineInRange(DeadlineActivity deadlineActivity)
        {
            return deadlineActivity.End < _rangeStart || deadlineActivity.Start > _rangeEnd;
        }

        private void ManageSharingWithDefinedActivities(IDefinedActivity activity, List<Activity> activities)
        {
            if (activity.Repeat == null || activity.Repeat.RepeatPeriod == RepeatPeriod.Single)
                return;

            var days = (int)activity.Repeat.RepeatPeriod;
            var currentDay = activity.Start.Value.Date.AddDays(days);

            if(activity is FixedActivity)
            {
                var fixedActivity = activity as FixedActivity;
                
                while (currentDay <= _rangeEnd)
                {
                    var item = new FixedActivity(fixedActivity.Name, fixedActivity.Description, fixedActivity.Color, CreationType.ManuallyCreated, fixedActivity.Labels,
                                                fixedActivity.Category, fixedActivity.UserId, fixedActivity.Priority, currentDay + fixedActivity.Start.Value.TimeOfDay, currentDay + fixedActivity.End.TimeOfDay, false);
                    currentDay = currentDay.AddDays(days);
                    activities.Add(item);
                }
            }
            else
            {
                var unfixedActivity = activity as UnfixedActivity;

                while (currentDay <= _rangeEnd)
                {
                    var item = new FixedActivity(unfixedActivity.Name, unfixedActivity.Description, unfixedActivity.Color, CreationType.ManuallyCreated, unfixedActivity.Labels,
                                                unfixedActivity.Category, unfixedActivity.UserId, unfixedActivity.Priority, currentDay + unfixedActivity.Start.Value.TimeOfDay, currentDay + unfixedActivity.End.Value.TimeOfDay, false);
                    currentDay = currentDay.AddDays(days);
                    activities.Add(item);
                }
            }
        }

        private void ManageSharingWithDeadlineActivities(DeadlineActivity activity, List<Activity> activities)
        {
            var itemStart = new FixedActivity(activity.Name, activity.Description, Color.Red, CreationType.ManuallyCreated, null,
                                                null, activity.UserId, 0, activity.Start, activity.Start.AddHours(1), false);

            var itemEnd = new FixedActivity(activity.Name, activity.Description, Color.Red, CreationType.ManuallyCreated, null,
                                                null, activity.UserId, 0, activity.End, activity.End.AddHours(1), false);

            activities.Add(itemStart);
            activities.Add(itemEnd);

            foreach (var milestone in activity.Milestones)
            {
                activities.Add(new FixedActivity(milestone.Name, "", Color.Red, CreationType.ManuallyCreated, null,
                                                null, activity.UserId, 0, milestone.Time, milestone.Time.AddHours(1), false));
            }
        }
    }
}
