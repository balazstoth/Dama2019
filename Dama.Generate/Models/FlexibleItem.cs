using Dama.Data.Models;
using System;

namespace Dama.Generate
{
    public class FlexibleItem
    {
        public UndefinedActivity UndefinedActivity { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan TimeSpan => End - Start;

        public FlexibleItem(UndefinedActivity undefinedActivity, DateTime start, DateTime end)
        {
            UndefinedActivity = undefinedActivity;
            Start = start;
            End = end;
        }
    }
}
