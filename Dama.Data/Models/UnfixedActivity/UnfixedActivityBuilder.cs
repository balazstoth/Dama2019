using System;

namespace Dama.Data.Models
{
    public class UnfixedActivityBuilder : ActivityBuilder<UnfixedActivityBuilder>
    {
        private int priority;
        private TimeSpan timeSpan;

        public UnfixedActivityBuilder WithPriority(int priority)
        {
            this.priority = priority;
            return this;
        }
        public UnfixedActivityBuilder WithTimeSpan(TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
            return this;
        }

        public static implicit operator UnfixedActivity(UnfixedActivityBuilder ufb)
        {
            return new UnfixedActivity(ufb.name,
                                     ufb.description,
                                     ufb.color,
                                     ufb.creationType,
                                     ufb.labels,
                                     ufb.category,
                                     ufb.userId,
                                     ufb.priority,
                                     ufb.timeSpan);
        }
    }
}
