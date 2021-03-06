﻿using Dama.Data.Models;
using System;

namespace Dama.Generate
{
    public class FlexibleActivityItem
    {
        public UndefinedActivity UndefinedActivity { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan TimeSpan => End - Start;

        public FlexibleActivityItem(UndefinedActivity undefinedActivity, DateTime start, DateTime end)
        {
            if (undefinedActivity == null)
                throw new ArgumentNullException("undefinedActivity");

            UndefinedActivity = undefinedActivity;
            Start = start;
            End = end;
        }
    }
}
