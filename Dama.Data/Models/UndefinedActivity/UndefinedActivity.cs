using Dama.Data.Enums;
using System;
using System.Collections.Generic;

namespace Dama.Data.Models
{
    public class UndefinedActivity : Activity
    {
        public int MinimumTime { get; set; }
        public int MaximumTime { get; set; }

        public UndefinedActivity()
        {
        }

        public UndefinedActivity(string name,
                             string description,
                             Color color,
                             CreationType creationType,
                             IEnumerable<Label> labels,
                             Category category,
                             string userId,
                             int minTime,
                             int maxTime,
                             bool baseActivity)
                                           : base(name,
                                                  description,
                                                  color,
                                                  creationType,
                                                  labels,
                                                  category,
                                                  userId,
                                                  ActivityType.UndefinedActivity,
                                                  baseActivity)
        {
            if (MinimumTime < 0)
                throw new ArgumentOutOfRangeException("minimumTime");

            if (MaximumTime < 0)
                throw new ArgumentOutOfRangeException("MaximumTime");

            if (MinimumTime >= MaximumTime)
                throw new ArgumentException("MinimumTime, MaximumTime");

            MinimumTime = minTime;
            MaximumTime = maxTime;
        }

        public override string ToString()
        {
            return String.Format($"Name: {Name}, MinTime: {MinimumTime}, MaxTime: {MaximumTime}");
        }

        public static UndefinedActivity operator +(UndefinedActivity first, UndefinedActivity second)
        {
            first.Category = second.Category;
            first.Color = second.Color;
            first.Description = second.Description;
            first.CreationType = second.CreationType;
            first.Labels = second.Labels;
            first.Name = second.Name;
            first.ActivityType = second.ActivityType;
            first.MaximumTime = second.MaximumTime;
            first.MinimumTime = second.MinimumTime;

            return first;
        }
    }
}
