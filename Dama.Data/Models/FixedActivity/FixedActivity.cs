using System;
using System.Collections.Generic;
using Dama.Data.Enums;
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
        public bool IsUnfixedOriginally { get; set; }
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
                             DateTime end,
                             bool baseActivity) : base(name,
                                                  description,
                                                  color,
                                                  creationType,
                                                  labels,
                                                  category,
                                                  userId,
                                                  ActivityType.FixedActivity,
                                                  baseActivity)
        {
            if (start >= end)
                throw new ArgumentException("Start, End");

            if (priority < 0)
                throw new ArgumentOutOfRangeException("priority");

            Start = start;
            End = end;
            Priority = priority;
            TimeSpan = CalculateTimeSpan(Start.GetValueOrDefault(), End);
            OriginalType = ActivityType.FixedActivity;
            Repeat = null;
            IsUnfixedOriginally = false;
        }

        public FixedActivity(DateTime start, UnfixedActivity unfixedActivity)
        {
            Category = unfixedActivity.Category;
            Color = unfixedActivity.Color;
            Description = unfixedActivity.Description;
            End = start + unfixedActivity.TimeSpan;
            OriginalType = ActivityType.UnfixedActivity;
            Id = unfixedActivity.Id;
            Labels = unfixedActivity.Labels;
            Name = unfixedActivity.Name;
            ActivityType = ActivityType.FixedActivity;
            Priority = unfixedActivity.Priority;
            Repeat = unfixedActivity.Repeat;
            Start = start;
            CreationType = CreationType.ManuallyCreated;
            TimeSpan = unfixedActivity.TimeSpan;
            UserId = unfixedActivity.UserId;
            IsUnfixedOriginally = true;
        } 
        #endregion

        public override string ToString()
        {
            return string.Format($"Name: {Name}, Start: {Start}, End: {End}, Priority: {Priority}");
        }

        public static FixedActivity operator +(FixedActivity first, FixedActivity second)
        {
            first.Category = second.Category;
            first.Color = second.Color;
            first.Description = second.Description;
            first.End = second.End;
            first.CreationType = second.CreationType;
            first.Labels = second.Labels;
            first.OriginalType = second.OriginalType;
            first.Name = second.Name;
            first.ActivityType = second.ActivityType;
            first.Priority = second.Priority;
            first.Repeat = second.Repeat;
            first.Start = second.Start;
            first.TimeSpan = second.TimeSpan;
            first.IsUnfixedOriginally = second.IsUnfixedOriginally;

            return first;
        }

        public TimeSpan CalculateTimeSpan(DateTime start, DateTime end)
        {
            return end.TimeOfDay - start.TimeOfDay;
        }
    }
}
