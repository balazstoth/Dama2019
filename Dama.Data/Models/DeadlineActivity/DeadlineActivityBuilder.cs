using System;
using System.Collections.Generic;

namespace Dama.Data.Models
{
    public class DeadlineActivityBuilder : ActivityBuilder<DeadlineActivityBuilder>
    {
        private DateTime start;
        private DateTime end;
        private List<Milestone> milestones;

        public DeadlineActivityBuilder WithStart(DateTime start)
        {
            this.start = start;
            return this;
        }
        public DeadlineActivityBuilder WithEnd(DateTime end)
        {
            this.end = end;
            return this;
        }
        public DeadlineActivityBuilder WithMilestones(List<Milestone> milestones)
        {
            this.milestones = milestones;
            return this;
        }

        public static implicit operator DeadlineActivity(DeadlineActivityBuilder db)
        {
            return new DeadlineActivity(db.name,
                                     db.description,
                                     db.color,
                                     db.creationType,
                                     db.labels,
                                     db.category,
                                     db.userId,
                                     db.start,
                                     db.end,
                                     db.milestones);
        }
    }
}
