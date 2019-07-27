using Dama.Data.Enums;
using Dama.Data.Models;
using Dama.Organizer.Models;
using Dama.Web.Models;
using Dama.Web.Models.ViewModels.Activity.Manage;
using System;
using System.Linq;
using System.Text;
using Dama.Data.Sql.Interfaces;

namespace Dama.Web.Manager
{
    public class CalendarControllerManager
    {
        private readonly IUnitOfWork _unitOfWork;

        public CalendarControllerManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public FixedActivityManageViewModel AssembleFixedActivityManageViewModel(EditDetails details)
        {
            bool? optional = null;
            FixedActivity fixedActivity;

            using (var container = new ActivityContainer())
            {
                if (details.CalledFromEditor)
                {
                    optional = details.IsOptional;

                    if (details.IsOptional)
                        fixedActivity = container.ActivitySelectedByUserForOptional
                                                            .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                                 a.ActivityType == ActivityType.FixedActivity) as FixedActivity;
                    else
                        fixedActivity = container.ActivitySelectedByUserForSure
                                                            .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                                 a.ActivityType == ActivityType.FixedActivity) as FixedActivity;
                }
                else
                {
                    fixedActivity = _unitOfWork.FixedActivityRepository.Get(includeProperties: "Category,Labels")
                                                                       .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                                            a.CreationType == CreationType.ManuallyCreated);
                }
            }

            var fixedActivityViewModel = new FixedActivityManageViewModel()
            {
                Id = details.ActivityId.ToString(),
                Category = fixedActivity.Category?.ToString(),
                CategorySourceCollection = details.Categories,
                Color = fixedActivity.Color.ToString(),
                ColorSourceCollection = details.Colors,
                Description = fixedActivity.Description,
                EndTime = fixedActivity.End,
                LabelSourceCollection = details.Labels,
                Labels = fixedActivity.Labels.Select(l => l.Name),
                Name = fixedActivity.Name,
                Priority = fixedActivity.Priority,
                StartTime = fixedActivity.Start.GetValueOrDefault(),
                RepeatTypeSourceCollection = details.RepeatTypes,
                EnableRepeatChange = details.CalledFromEditor,
                RepeatType = fixedActivity.Repeat?.RepeatPeriod.ToString(),
                RepeatEndDate = fixedActivity.Repeat?.EndDate ?? DateTime.Today.AddDays(7),
                IsOptional = optional
            };

            return fixedActivityViewModel;
        }

        public UnfixedActivityManageViewModel AssembleUnfixedActivityViewModel(EditDetails details)
        {
            UnfixedActivity unfixedActivity;
            bool? optional = null;

            using (var container = new ActivityContainer())
            {
                if (details.CalledFromEditor)
                {
                    optional = details.IsOptional;

                    if (details.IsOptional)
                        unfixedActivity = container.ActivitySelectedByUserForOptional
                                                        .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                             a.ActivityType == ActivityType.UnfixedActivity) as UnfixedActivity;
                    else
                        unfixedActivity = container.ActivitySelectedByUserForSure
                                                        .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                             a.ActivityType == ActivityType.UnfixedActivity) as UnfixedActivity;
                }
                else
                {
                    unfixedActivity = _unitOfWork.UnfixedActivityRepository.Get(includeProperties: "Category,Labels")
                                                                           .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                                                a.CreationType == CreationType.ManuallyCreated);
                }
            }

            var unfixedActivityViewModel = new UnfixedActivityManageViewModel()
            {
                Id = details.ActivityId.ToString(),
                Category = unfixedActivity.Category?.ToString(),
                CategorySourceCollection = details.Categories,
                Color = unfixedActivity.Color.ToString(),
                ColorSourceCollection = details.Colors,
                Description = unfixedActivity.Description,
                LabelSourceCollection = details.Labels,
                Labels = unfixedActivity.Labels.Select(x => x.Name),
                Name = unfixedActivity.Name,
                Priority = unfixedActivity.Priority,
                Timespan = unfixedActivity.TimeSpan,
                RepeatTypeSourceCollection = details.RepeatTypes,
                EnableRepeatChange = details.CalledFromEditor,
                RepeatType = unfixedActivity.Repeat?.RepeatPeriod.ToString(),
                RepeatEndDate = unfixedActivity.Repeat?.EndDate ?? DateTime.Today.AddDays(7),
                IsOptional = optional
            };

            return unfixedActivityViewModel;
        }

        public UndefinedActivityManageViewModel AssembleUndefinedActivityViewModel(EditDetails details)
        {
            UndefinedActivity undefinedActivity;

            using (var container = new ActivityContainer())
            {
                if (details.CalledFromEditor)
                    undefinedActivity = container.ActivitySelectedByUserForOptional
                                                        .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                             a.ActivityType == ActivityType.UndefinedActivity) as UndefinedActivity;
                else
                    undefinedActivity = _unitOfWork.UndefinedActivityRepository.Get(includeProperties: "Category,Labels")
                                                                               .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                                                    a.CreationType == CreationType.ManuallyCreated);
            }

            var undefinedActivityViewModel = new UndefinedActivityManageViewModel()
            {
                Id = details.ActivityId.ToString(),
                Category = undefinedActivity.Category?.ToString(),
                CategorySourceCollection = details.Categories,
                Color = undefinedActivity.Color.ToString(),
                ColorSourceCollection = details.Colors,
                Description = undefinedActivity.Description,
                LabelSourceCollection = details.Labels,
                Labels = undefinedActivity.Labels.Select(x => x.Name),
                Name = undefinedActivity.Name,
                CalledFromEditor = details.CalledFromEditor,
                MaximumTime = undefinedActivity.MaximumTime,
                MinimumTime = undefinedActivity.MinimumTime
            };

            return undefinedActivityViewModel;
        }

        public DeadlineActivityManageViewModel AssembleDeadlineActivityViewModel(EditDetails details)
        {
            DeadlineActivity deadlineActivity;
            var milestoneStringBuilder = new StringBuilder();

            using (var container = new ActivityContainer())
            {
                if (details.CalledFromEditor)
                    deadlineActivity = container.ActivitySelectedByUserForSure
                                                    .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                         a.ActivityType == ActivityType.DeadlineActivity) as DeadlineActivity;
                else
                    deadlineActivity = _unitOfWork.DeadlineActivityRepository.Get(includeProperties: "Milestones")
                                                                             .FirstOrDefault(a => a.Id == details.ActivityId &&
                                                                                                  a.CreationType == CreationType.ManuallyCreated);

                foreach (var milestone in deadlineActivity.Milestones)
                    milestoneStringBuilder.Append($"{milestone.Name};{milestone.Time}|");
            }

            var deadlineActivityViewModel = new DeadlineActivityManageViewModel()
            {
                Id = details.ActivityId.ToString(),
                Description = deadlineActivity.Description,
                Name = deadlineActivity.Name,
                EndDate = deadlineActivity.End.Date,
                StartDate = deadlineActivity.Start.Date,
                EndTime = deadlineActivity.End,
                StartTime = deadlineActivity.Start,
                Milestones = milestoneStringBuilder.ToString(),
                CalledFromEditor = details.CalledFromEditor
            };

            return deadlineActivityViewModel;
        }
    }
}