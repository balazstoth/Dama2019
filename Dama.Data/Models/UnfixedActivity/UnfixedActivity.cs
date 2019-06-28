using System;
using System.Collections.Generic;
using Dama.Data.Interfaces;

namespace Dama.Data.Models
{
    public class UnfixedActivity : Activity, IDefinedActivity
    {
        public int Priority { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateTime? Start { get; set; }
        public Repeat Repeat { get; set; }

        #region Constructors
        public UnfixedActivity()
        {
        }

        public UnfixedActivity(
            string name, 
            string description,
            Color color, 
            CreationType creationType, 
            IEnumerable<Label> labels, 
            Category category, 
            string userId, 
            int priority, 
            TimeSpan timeSpan) : base(name, 
                                      description, 
                                      color, 
                                      creationType, 
                                      labels, 
                                      category, 
                                      userId, 
                                      ActivityType.UnfixedActivity)
        {
            Priority = priority;
            TimeSpan = timeSpan;
            Repeat = null;
            Start = null;
        }

        public UnfixedActivity(FixedActivity fixedActivity)
        {
            Id = fixedActivity.Id;
            Name = fixedActivity.Name;
            Description = fixedActivity.Description;
            Color = fixedActivity.Color;
            Category = fixedActivity.Category;
            LabelCollection = fixedActivity.LabelCollection;
            Priority = fixedActivity.Priority;
            ActivityType = ActivityType.UnfixedActivity;
            CreationType = CreationType.ManuallyCreated;
            UserId = fixedActivity.UserId;
            Start = fixedActivity.Start;
            Repeat = fixedActivity.Repeat;
            TimeSpan = fixedActivity.TimeSpan;
        }
        #endregion

        public override string ToString()
        {
            return String.Format($"Name: {Name}, Timespan: {TimeSpan}, Priority: {Priority}");
        }

        public static UnfixedActivity operator +(UnfixedActivity first, UnfixedActivity second)
        {
            first.Category = second.Category;
            first.Color = second.Color;
            first.Description = second.Description;
            first.CreationType = second.CreationType;
            first.LabelCollection = second.LabelCollection;
            first.Name = second.Name;
            first.ActivityType = second.ActivityType;
            first.Priority = second.Priority;
            first.Repeat = second.Repeat;
            first.TimeSpan = second.TimeSpan;
            first.Start = second.Start;

            return first;
        }
    }
}