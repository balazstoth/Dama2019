using System;
using System.Collections.Generic;
using Dama.Data.Interfaces;

namespace Dama.Data.Models
{
    public class FixedActivity : Activity, IDefinedActivity
    {
        #region Properties
        public int Priority { get; set; }
        public DateTime? Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public ActivityType OriginalType { get; set; }
        public Repeat Repeat { get; set; } 
        #endregion

        #region Constructor
        public FixedActivity()
        {
        }

        public FixedActivity(string name,
                             string description,
                             Color color,
                             CreationType creationType,
                             IEnumerable<Label> labels,
                             Category category,
                             string userId,
                             int priority,
                             DateTime start,
                             DateTime end) : base(name,
                                                  description,
                                                  color,
                                                  creationType,
                                                  labels,
                                                  category,
                                                  userId,
                                                  ActivityType.FixedActivity)
        {
            Start = start;
            End = end;
            Priority = priority;
            TimeSpan = CalculateTimeSpan(Start.GetValueOrDefault(), End);
            OriginalType = ActivityType.FixedActivity;
            Repeat = null;
        }

        public FixedActivity(DateTime start, UnfixedActivity unfixedActivity)
        {
            Category = unfixedActivity.Category;
            Color = unfixedActivity.Color;
            Description = unfixedActivity.Description;
            End = start + unfixedActivity.TimeSpan;
            OriginalType = ActivityType.UnfixedActivity;
            Id = unfixedActivity.Id;
            LabelCollection = unfixedActivity.LabelCollection;
            Name = unfixedActivity.Name;
            ActivityType = ActivityType.FixedActivity;
            Priority = unfixedActivity.Priority;
            Repeat = unfixedActivity.Repeat;
            Start = start;
            CreationType = CreationType.ManuallyCreated;
            TimeSpan = unfixedActivity.TimeSpan;
            UserId = unfixedActivity.UserId;
        } 
        #endregion

        public override string ToString()
        {
            return String.Format($"Name: {Name}, Start: {Start}, End: {End}, Priority: {Priority}");
        }

        public static FixedActivity operator +(FixedActivity first, FixedActivity second)
        {
            first.Category = second.Category;
            first.Color = second.Color;
            first.Description = second.Description;
            first.End = second.End;
            first.CreationType = second.CreationType;
            first.LabelCollection = second.LabelCollection;
            first.OriginalType = second.OriginalType;
            first.Name = second.Name;
            first.ActivityType = second.ActivityType;
            first.Priority = second.Priority;
            first.Repeat = second.Repeat;
            first.Start = second.Start;
            first.TimeSpan = second.TimeSpan;

            return first;
        }

        public TimeSpan CalculateTimeSpan(DateTime start, DateTime end)
        {
            return end.TimeOfDay - start.TimeOfDay;
        }
    }
}
