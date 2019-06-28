﻿using System;
using System.Collections.Generic;

namespace Dama.Data.Models
{
    class DeadlineActivity : Activity
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<Milestone> Milestones { get; set; }

        public DeadlineActivity(string name,
                                 string description,
                                 Color color,
                                 CreationType creationType,
                                 IEnumerable<Label> labels,
                                 Category category,
                                 string userId,
                                 DateTime start,
                                 DateTime end,
                                 List<Milestone> milestones)
                                           : base(name,
                                                  description,
                                                  color,
                                                  creationType,
                                                  labels,
                                                  category,
                                                  userId,
                                                  ActivityType.DeadlineActivity)
        {
            this.Start = start;
            this.End = end;
            this.Milestones = milestones;
        }

        public DeadlineActivity()
        {
        }

        public override string ToString()
        {
            return String.Format($"Name: {Name}, Start: {Start}, End: {End}", Name, Start, End);
        }

        public static DeadlineActivity operator +(DeadlineActivity first, DeadlineActivity second)
        {
            first.Category = second.Category;
            first.Color = second.Color;
            first.Description = second.Description;
            first.End = second.End;
            first.CreationType = second.CreationType;
            first.LabelCollection = second.LabelCollection;
            first.Name = second.Name;
            first.ActivityType = second.ActivityType;
            first.Start = second.Start;
            first.End = second.End;
            first.Milestones = second.Milestones;

            return first;
        }
    }
}
