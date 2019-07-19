using System;

namespace Dama.Data.Models
{
    public class FixedActivityBuilder : ActivityBuilder<FixedActivityBuilder>
    {
        private int priority;
        private DateTime start;
        private DateTime end;

        public FixedActivityBuilder WithPriority(int priority)
        {
            this.priority = priority;
            return this;
        }
        public FixedActivityBuilder WithStart(DateTime start)
        {
            this.start = start;
            return this;
        }
        public FixedActivityBuilder WithEnd(DateTime end)
        {
            this.end = end;
            return this;
        }

        public static implicit operator FixedActivity(FixedActivityBuilder fb)
        {
            return new FixedActivity(fb.name,
                                     fb.description,
                                     fb.color,
                                     fb.creationType,
                                     fb.labels,
                                     fb.category,
                                     fb.userId,
                                     fb.priority,
                                     fb.start,
                                     fb.end,
                                     fb.baseActivity);
        }
    }
}
